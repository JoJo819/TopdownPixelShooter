
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private float _rigidbody2DX;
    private float _rigidbody2DY;
    private float _velocity;
    public Rigidbody2D _rb;
    private EventTrigger _shootButton;
    private float _moveSpeed;
    public float Health = 1500;
    public float Armor = 0;
    public float MaxArmor = 1500;
    public float ReloadDelay = 0f;
    public Weapon Gun;
    public float Spread;
    public float CharacterSpeedFirst;
    public float CharacterSpeed;
    private float _fireRateCooldown;

    private float currentRotation = 0f;
    public GameObject Cam;
    public GameObject Hud;
    public GameObject FlashLight;
    public GameObject RayVisualPrefab;
    public GameObject DeathPlayer;
    public GameObject GunObject;
    public GameObject ShootStartPoint;
    public GameObject TakeButton;
    public TMP_Text AmmoCount;
    private bool _shooting;
    private bool _isReloading = false;
    public bool IsPC;
    public bool AllowMoving = true;
    private AudioSource _playerSource;

    private void Awake()
    {
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _playerSource = GetComponent<AudioSource>();
    }


    private void DisablePlayerComponents()
    {
        _rb.bodyType = RigidbodyType2D.Kinematic;
        Cam.SetActive(false);
        Hud.SetActive(false);
        FlashLight.SetActive(false);
        enabled = false;
    }



    private void Update()
    {

        HandleDelay();
        UpdateSpread();


        HandleMovement();

        if (_shooting)
            HandleShooting();
        UpdateHUD();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Gun")
        {
            UnityAction TakeItemAction = collision.GetComponent<ItemBottom>().TakeItem;
            //TakeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TakeItemAction);
            //TakeButton.SetActive(true);
            return;
        }
        else if (collision.tag == "Armor")
        {
            UnityAction TakeItemAction = collision.GetComponent<ArmorBottom>().TakeArmor;
            //TakeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TakeItemAction);
            //TakeButton.SetActive(true);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Gun")
        {
            //TakeButton.SetActive(false);
        }
    }

    private void HandleMovement()
    {
        KeyboardInput();
        RotateToMouse();
    }

    public void RotateToMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");

        currentRotation -= mouseX * 1;

        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
    }



    private void KeyboardInput()
    {
        if (!AllowMoving) return;
        Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetKeyUp(KeyCode.R)) Reload();
        if (Input.GetMouseButtonDown(0)) _shooting = true;
        else if (Input.GetMouseButtonUp(0)) _shooting = false;
        if (Input.GetKeyUp(KeyCode.H)) Suicide(this);
    }

    public void Move(float AxisX, float AxisY)
    {
        Vector2 inputVector = new Vector2(AxisX, AxisY).normalized;

        Vector2 moveDirection = transform.right * inputVector.x + transform.up * inputVector.y;

        _rb.velocity = moveDirection * CharacterSpeed;
    }

    private void UpdateHUD()
    {
        if (AmmoCount != null && Gun != null)
        {
            AmmoCount.text = Gun.countBulletsInMagazine.ToString() + "/" + Gun.bulletsInStock;
        }
    }

    public void ShootDown()
    {
        if (!AllowMoving) return;
        _shooting = true;
    }


    public void Reload()
    {
        CallReloadGunServerRpc();
    }

    IEnumerator CalculateReloadDelay(float duration, System.Action onComplete)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            ReloadDelay = remainingTime;
            if (!_isReloading) yield break;
            yield return null;
        }
        remainingTime = 0;
        ReloadDelay = 0;
        onComplete?.Invoke();
    }

    // ------------------ RPC METHODS ------------------ \\

    private void UpdateSpread()
    {
        float currentSpeed;

        currentSpeed = _rb.velocity.magnitude;
        float speedPercent = currentSpeed / 8;

        //Spread = Mathf.Lerp(Gun.minSpread, Gun.spread, speedPercent);
    }

    public void ChangeWeaponParameters(string gunName, int countBulletsInMagazine, int countBulletsInStock)
    {
        if (Gun.weaponName != null)
        {
            SpawnGun();
        }
        Gun.weaponName = gunName;
        Gun.LoadWeaponDataFromDatabase(gunName);
        Gun.countBulletsInMagazine = countBulletsInMagazine;
        Gun.bulletsInStock = countBulletsInStock;
        UpdateGunVisuals(this, gunName);
    }
    
    public void SpawnGun()
    {
        _isReloading = false;
        Debug.Log(Gun.weaponName);
        GameObject myGun = Instantiate(Resources.Load<GameObject>("Guns/" + Gun.weaponName), transform.position, Quaternion.identity);
        myGun.GetComponent<Weapon>().countBulletsInMagazine = Gun.countBulletsInMagazine;
        myGun.GetComponent<Weapon>().bulletsInStock = Gun.bulletsInStock;
    }


    private void UpdateGunVisuals(PlayerController player, string gunName)
    {
        //player.GunObject.name = gunName;
        //player.GunObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/GunsTop/" + gunName);
        //player._playerSource.clip = Resources.Load<AudioClip>("Sounds/Gun/" + gunName);
    }
    
    public void TakeArmorRpc(PlayerController player)
    {
        if (player == null) return;

        player.Armor = player.MaxArmor;
        TakeArmorObserverRpc(player);
    }

    private void TakeArmorObserverRpc(PlayerController player)
    {
        if (player == null) return;

        player.Armor = player.MaxArmor;
    }

    private void CallReloadGunServerRpc()
    {
        if (Gun.bulletsInStock <= 0) return;
        _isReloading = !_isReloading;

        if (!_isReloading)
        {
            StopAllCoroutines();
            ReloadDelay = 0;
            return;
        }

        StartCoroutine(CalculateReloadDelay(Gun.reloadDelay, ReloadGunServerRpc));
    }

    private void ReloadGunServerRpc()
    {
        if (Gun.bulletsInStock > Gun.maxBulletsInMagazine - Gun.countBulletsInMagazine)
        {
            Gun.bulletsInStock -= Gun.maxBulletsInMagazine - Gun.countBulletsInMagazine;
            Gun.countBulletsInMagazine = Gun.maxBulletsInMagazine;
        }
        else
        {
            Gun.countBulletsInMagazine += Gun.bulletsInStock;
            Gun.bulletsInStock -= Gun.bulletsInStock;
        }
        _isReloading = false;
    }

    public void HandleDelay()
    {
        _fireRateCooldown -= Time.deltaTime;
    }

    private void HandleShooting()
    {
        if (Gun == null || _fireRateCooldown > 0 || Gun.countBulletsInMagazine <= 0 || _isReloading) return;

        Gun.countBulletsInMagazine--;
        _fireRateCooldown = Gun.fireRate;
        ShootServerRpc(Gun.damage, Gun.armorPenetration, ShootStartPoint.transform.position, transform.up, Spread);
    }

    private void ShootServerRpc(float damage, float armorPenetration, Vector2 position, Vector2 direction, float bulletSpread)
    {
        if (_isReloading) return;

        for (int i = 0; i < Gun.countBulletsInShoot; i++)
        {
            float spreadAngle = Random.Range(-bulletSpread, bulletSpread);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * direction;

            RaycastHit2D hit = Physics2D.Raycast(position, spreadDirection, Gun.distance, 7);

            if (hit.collider != null)
            {
                Debug.Log("HIT! " + damage + " " + hit.collider.gameObject.name);
                var target = hit.collider.GetComponent<PlayerController>();
                if (target != null && target != this)
                {
                    ApplyDamage(target, damage, armorPenetration);
                }
            }
            SpawnBulletVisual(position, spreadDirection);
        }
    }


    public void Suicide(PlayerController target)
    {
        ApplyDamage(target, 99999999, 100);
    }

    private void ApplyDamage(PlayerController target, float damage, float armorPenetration)
    {
        float damageToArmor = damage * (1 - armorPenetration / 100);
        float damageToHP = damage * (armorPenetration / 100);

        if (target.Armor > 0 && target.Health > 0)
        {
            if (target.Armor >= damageToArmor)
                target.Armor -= damageToArmor;
            else
            {
                float remainingDamage = damageToArmor - target.Armor;
                target.Armor = 0;
                damageToHP += remainingDamage;
            }
            target.Health -= Mathf.Round(damageToHP);
            if (target.Health <= 0)
            {
                target.Armor = 0;
                if (target.Gun.weaponName != null)
                {
                    target.SpawnGun();
                }

                Death(target);
                GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), target.transform.position, Quaternion.identity);
            }
            return;
        }
        else
        {
            target.Health -= Mathf.Round(damage);
        }

        if (target.Health <= 0)
        {
            target.Armor = 0;
            if (target.Gun.weaponName != null)
            {
                target.SpawnGun();
            }

            Death(target);
            GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), target.transform.position, Quaternion.identity);
        }
    }

    private void Death(PlayerController target)
    {
        if (!target.enabled) return;
        Transform cameraTransform = target.GetComponentInChildren<Camera>().transform;

        if (cameraTransform != null)
        {
            cameraTransform.SetParent(null);
        }
    }

    private void SpawnBulletVisual(Vector2 startPosition, Vector2 direction)
    {
        GameObject bullet = Instantiate(RayVisualPrefab, startPosition, Quaternion.identity);
        bullet.GetComponent<BulletController>().Direction = direction;
        bullet.GetComponent<BulletController>().MaxDistance = Gun.distance;
        bullet.GetComponent<BulletController>().Push();
    }
}

