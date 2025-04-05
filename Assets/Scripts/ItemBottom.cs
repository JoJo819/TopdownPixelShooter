
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Armor : MonoBehaviour
{
    public int countArmor;
}

public class ItemBottom : MonoBehaviour
{
    private Weapon _gun;

    [Header("UI Elements")]
    public Slider DamageSlider;
    public Slider FireRateSlider;
    public Slider DistanceSlider;
    public Slider SpreadSlider;

    public GameObject Canvas;
    public GameObject CharacteristicsPanel;

    [Header("Player Interaction")]
    private Collider2D _player;

    private Camera _mainCamera;

    public void Start()
    {
        _gun = GetComponent<Weapon>();
        _mainCamera = Camera.main;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.GetComponent<PlayerController>().enabled)
        {
            Canvas.transform.rotation = _mainCamera.transform.rotation;
            _player = collision;
            CharacteristicsPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _player)
        {
            _player = null;
            CharacteristicsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (_player != null)
        {
            Canvas.transform.rotation = _mainCamera.transform.rotation;

            if (_gun != null)
            {
                DamageSlider.value = _gun.damage;
                FireRateSlider.value = -_gun.fireRate;
                DistanceSlider.value = _gun.distance;
                SpreadSlider.value = _gun.spread;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                TakeItem();
            }
        }
    }

    public void DespawnItem()
    {
        Destroy(gameObject);
    }

    public void TakeItem()
    {
        if (_player == null) return;

        PlayerController playerScript = _player.GetComponent<PlayerController>();

        if (_gun != null)
        {
            playerScript.ChangeWeaponParameters(gameObject.name, _gun.countBulletsInMagazine, _gun.bulletsInStock);
        }

        DespawnItem();
    }
}
