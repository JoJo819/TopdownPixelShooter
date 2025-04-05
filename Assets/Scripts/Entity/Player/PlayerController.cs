
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Animator CameraAnimator;
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
    public bool AllowTakeGun;
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
    public Texture2D CursorTexture;
    public float FistForce;
    private void Awake()
    {
        Cursor.SetCursor(CursorTexture, Vector2.zero, CursorMode.Auto);
        _rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();
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

    private void FixedUpdate()
    {

        HandleDelay();
        UpdateSpread();
    }

    private void Update()
    {

        HandleDelay();
        UpdateSpread();


        HandleMovement();

        if (_shooting)
            HandleShooting();
        else
            CameraAnimator.SetBool("Shooting", false);
        UpdateHUD();
    }



    private void HandleMovement()
    {
        KeyboardInput();
        RotateToMouse();
    }

    public void RotateToMouse()
    {
        //    float mouseX = Input.GetAxis("Mouse X");

        //    currentRotation -= mouseX * 1;

        //    transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        Vector3 mousePosition = GetMouseWorldPosition();

        // Вычисляем направление от персонажа к курсору
        Vector3 direction = mousePosition - transform.position;

        // Вычисляем угол поворота (по оси Z)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Добавляем смещение угла
        angle -= 90;

        // Применяем поворот к персонажу (вращение по оси Z)
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Преобразуем экранные координаты курсора в мировые координаты
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z; // Учитываем расстояние до камеры
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }


    private void KeyboardInput()
    {
        if (!AllowMoving) return;
        Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetKeyUp(KeyCode.R)) Reload();
        if (Input.GetMouseButtonDown(0)) _shooting = true;
        if (Input.GetKeyUp(KeyCode.F) && !Gun.fist)
        {
            SpawnGun(); 
        }
        else if (Input.GetMouseButtonUp(0)) _shooting = false;
        if (Input.GetKeyUp(KeyCode.H)) Suicide(this);
    }

    public void Move(float AxisX, float AxisY)
    {
        //Vector2 inputVector = new Vector2(AxisX, AxisY).normalized;

        //Vector2 moveDirection = transform.right * inputVector.x + transform.up * inputVector.y;

        //_rb.velocity = moveDirection * CharacterSpeed;
        Vector2 direction = new Vector2(AxisX, AxisY);

        // Нормализуем вектор направления, чтобы избежать увеличения скорости при диагональном движении
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        //// Устанавливаем скорость персонажа
        _rb.velocity = direction * CharacterSpeed;
    }

    private void UpdateHUD()
    {
        if (AmmoCount != null && Gun != null && !Gun.fist)
        {
            AmmoCount.text = Gun.countBulletsInMagazine.ToString() + "/" + Gun.bulletsInStock;
            AmmoCount.transform.GetChild(0).GetComponent<TMP_Text>().text = Gun.countBulletsInMagazine.ToString() + "/" + Gun.bulletsInStock;
        }
        else
        {
            AmmoCount.text = "Infinity!";
            AmmoCount.transform.GetChild(0).GetComponent<TMP_Text>().text = AmmoCount.text = "Infinity!";
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

        Spread = Mathf.Lerp(Gun.minSpread, Gun.spread, speedPercent);
    }

    public void ChangeWeaponParameters(string gunName, int countBulletsInMagazine, int countBulletsInStock)
    {
        Gun.weaponName = gunName;
        Gun.LoadWeaponDataFromDatabase(gunName);
        Gun.countBulletsInMagazine = countBulletsInMagazine;
        Gun.bulletsInStock = countBulletsInStock;
        UpdateGunVisuals(this, gunName);
    }

    public void SpawnGun()
    {
        _isReloading = false;

        // Создаем объект оружия перед игроком
        GameObject myGun = Instantiate(Resources.Load<GameObject>("Guns/GunTemplate"), transform.position + transform.up * 2f, Quaternion.identity);

        Weapon newGunWeaponScr = myGun.GetComponent<Weapon>();
        newGunWeaponScr.weaponName = Gun.weaponName;
        newGunWeaponScr.LoadWeaponDataFromDatabase(Gun.weaponName);
        newGunWeaponScr.countBulletsInMagazine = Gun.countBulletsInMagazine;
        newGunWeaponScr.bulletsInStock = Gun.bulletsInStock;

        // Добавляем силу выброса вперед
        Rigidbody2D rb = myGun.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Вычисляем направление движения в 2D
            rb.AddForce(transform.up * 40, ForceMode2D.Impulse); // Сила выброса вперед
            StartCoroutine(SlowDownObject(rb));
        }

        // Меняем параметры текущего оружия на "кулаки"
        ChangeWeaponParameters("Fist", 0, 0);
    }

    private IEnumerator SlowDownObject(Rigidbody2D rb)
    {
        float initialSpeed = rb.velocity.magnitude;
        float timeToStop = 5f; // Время до полной остановки
        float elapsed = 0f;

        while (elapsed < timeToStop)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / timeToStop;
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, t);
            yield return null;
        }

        rb.velocity = Vector2.zero; // Полная остановка
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
        if (Gun == null || _fireRateCooldown > 0 || !Gun.fist && Gun.countBulletsInMagazine <= 0 || _isReloading)
        {
            CameraAnimator.SetBool("Shooting", false);
            return;
        }

        if(!Gun.fist)
            Gun.countBulletsInMagazine--;
        else
        {
            Vector2 direction = new Vector2(transform.right.x, transform.right.y).normalized;
            _rb.AddForce(transform.up * FistForce, ForceMode2D.Force);
        }
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
            CameraAnimator.SetBool("Shooting", true);
            //if (hit.collider != null)
            //{
            //    Debug.Log("HIT! " + damage + " " + hit.collider.gameObject.name);
            //    var target = hit.collider.GetComponent<EnemyController>();
            //    if (target != null && target != this)
            //    {
            //        target.ApplyDamage(damage, armorPenetration);
            //    }
            //}
            SpawnBulletVisual(position, spreadDirection, damage, armorPenetration);
        }
    }


    public void Suicide(PlayerController target)
    {
    }

    private void ApplyDamage(EnemyController target, float damage, float armorPenetration)
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

    private void SpawnBulletVisual(Vector2 startPosition, Vector2 direction, float damage, float armorPenetration)
    {
        GameObject bullet = Instantiate(RayVisualPrefab, startPosition, Quaternion.identity);
        BulletController bulletScr = bullet.GetComponent<BulletController>();
        bulletScr.Direction = direction;
        bulletScr.MaxDistance= Gun.distance;
        bulletScr.Damage = damage;
        bulletScr.ArmorPenetration = armorPenetration;
        bulletScr.Push();
    }
}

