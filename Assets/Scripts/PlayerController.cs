using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using FishNet.Connection;

public class PlayerController : NetworkBehaviour
{
    [SyncVar] private float _rigidbody2DX;
    [SyncVar] private float _rigidbody2DY;
    [SyncVar] private float _velocity;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private EventTrigger _shootButton;
    [SerializeField] private float _moveSpeed;
    [SyncVar] public float Health = 1500;
    [SyncVar] public float Armor = 0;
    [SyncVar] public float MaxArmor = 1500;
    [SyncVar] public float ReloadDelay = 0f;
    [SyncVar] public Weapon Gun;
    [SyncVar] public NetworkConnection PlayerConn;
    public float Spread;
    public float CharacterSpeedFirst;
    public float CharacterSpeed;
    private float _fireRateCooldown;
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

        _rb = gameObject.GetComponent<Rigidbody2D>();
        _playerSource = GetComponent<AudioSource>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            PlayerConn = base.Owner;
            DisablePlayerComponents();
            return;
        }

        // Инициализация значений для владельца
        CharacterSpeed = CharacterSpeedFirst;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        CharacterSpeed = CharacterSpeedFirst;
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
        if (base.IsServer)
        {
            HandleDelay();
            UpdateSpread();

        }
        if (!base.IsOwner) return;

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
            TakeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TakeItemAction);
            TakeButton.SetActive(true);
            return;
        }
        else if (collision.tag == "Armor")
        {
            UnityAction TakeItemAction = collision.GetComponent<ArmorBottom>().TakeArmor;
            TakeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(TakeItemAction);
            TakeButton.SetActive(true);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Gun")
        {
            TakeButton.SetActive(false);
        }
    }

    private void HandleMovement()
    {
        Vector2 MoveDirection = new Vector2(_rigidbody2DY, _rigidbody2DX);
        _rb.velocity = transform.TransformDirection(MoveDirection);

        if (IsPC)
            KeyboardInput();
        else
            JoystickInput();

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

    [ServerRpc]
    public void Move(float AxisY, float AxisX)
    {
        if (AxisY != 0)
            _rigidbody2DY = AxisY * CharacterSpeed * 0.7f;

        _rigidbody2DX = AxisX * CharacterSpeed;
    }

    private void JoystickInput()
    {
        if (!AllowMoving) return;
        Move(_joystick.Horizontal, _joystick.Vertical);
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

    public void ShootUp() => _shooting = false;

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

    [Server]
    private void UpdateSpread()
    {
        float currentSpeed;
        _rb.velocity = new Vector2(_rigidbody2DX, _rigidbody2DY);

        currentSpeed = _rb.velocity.magnitude;
        float speedPercent = currentSpeed / 8;

        Spread = Mathf.Lerp(Gun.minSpread, Gun.spread, speedPercent);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeWeaponParametersRpc(string gunName, int countBulletsInMagazine, int countBulletsInStock)
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
        //ChangeGunObserverRpc(damage, fireRate, distance, spread, armorPenetration, maxBulletsInMagazine, gunName);
    }

    [Server]
    public void SpawnGun()
    {
        _isReloading = false;
        Debug.Log(Gun.weaponName);
        int countBulletsInMagazine = Gun.countBulletsInMagazine;
        int countBulletsInStock = Gun.bulletsInStock;

        GameObject myGun = Instantiate(Resources.Load<GameObject>("Prefabs/Guns/" + Gun.weaponName), transform.position, Quaternion.identity);
        ServerManager.Spawn(myGun);
        myGun.GetComponent<Weapon>().countBulletsInMagazine = countBulletsInMagazine;
        myGun.GetComponent<Weapon>().bulletsInStock = countBulletsInStock;
    }


    [ObserversRpc]
    private void UpdateGunVisuals(PlayerController player, string gunName)
    {
        player.GunObject.name = gunName;
        player.GunObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/GunsTop/" + gunName);
        player._playerSource.clip = Resources.Load<AudioClip>("Sounds/Gun/" + gunName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeArmorRpc(PlayerController player)
    {
        if (player == null) return;

        player.Armor = player.MaxArmor;
        TakeArmorObserverRpc(player);
    }

    [ObserversRpc]
    private void TakeArmorObserverRpc(PlayerController player)
    {
        if (player == null) return;

        player.Armor = player.MaxArmor;
    }

    [ServerRpc]
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

    [Server]
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

    [Server]
    public void HandleDelay()
    {
        _fireRateCooldown -= Time.deltaTime;

        if (_fireRateCooldown <= 0)
        {
            HandleShooting();
        }
    }

    [ServerRpc]
    private void HandleShooting()
    {
        if (Gun == null || _fireRateCooldown > 0 || Gun.countBulletsInMagazine <= 0 || _isReloading) return;

        Gun.countBulletsInMagazine--;
        _fireRateCooldown = Gun.fireRate;
        ShootServerRpc(Gun.damage, Gun.armorPenetration, ShootStartPoint.transform.position, transform.up, Spread);
    }

    [Server]
    private void ShootServerRpc(float damage, float armorPenetration, Vector2 position, Vector2 direction, float bulletSpread)
    {
        if (_isReloading) return;

        for (int i = 0; i < Gun.countBulletsInShoot; i++)
        {
            float spreadAngle = Random.Range(-bulletSpread, bulletSpread);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * direction;

            // Игнорируем свой коллайдер
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

    [ServerRpc]
    public void Suicide(PlayerController target)
    {
        ApplyDamage(target, 99999999, 100);
    }

    [Server]
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
                Despawn(target.gameObject);
                GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), target.transform.position, Quaternion.identity);
                target.Spawn(Tombstone);
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
            Despawn(target.gameObject);
            GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), target.transform.position, Quaternion.identity);
            target.Spawn(Tombstone);
        }
    }

    [ObserversRpc]
    private void Death(PlayerController target)
    {
        if (!target.enabled) return;
        Transform cameraTransform = target.GetComponentInChildren<Camera>().transform;

        if (cameraTransform != null)
        {
            cameraTransform.SetParent(null);
        }
    }

    [Server]
    private void SpawnBulletVisual(Vector2 startPosition, Vector2 direction)
    {
        GameObject bullet = Instantiate(RayVisualPrefab, startPosition, Quaternion.identity);
        ServerManager.Spawn(bullet);
        bullet.GetComponent<BulletController>().Direction = direction;
        bullet.GetComponent<BulletController>().MaxDistance = Gun.distance;
        bullet.GetComponent<BulletController>().Push();
    }
}

