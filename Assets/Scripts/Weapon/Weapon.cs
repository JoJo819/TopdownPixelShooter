using UnityEngine;
using static Item;

public class Weapon : MonoBehaviour, IItem
{
    public float damage;
    public float fireRate;
    public float distance;
    public float spread;
    public float minSpread;
    public float reloadDelay;
    public int armorPenetration;
    public int maxBulletsInMagazine;
    public int bulletsInStock;
    public int countBulletsInMagazine;
    public int countBulletsInShoot;
    public string weaponName;
    public bool fist;

    public void Start()
    {
        if(gameObject.GetComponent<PlayerController>() == null && gameObject.GetComponent<EnemyController>() == null)
            gameObject.name = weaponName;

        LoadWeaponDataFromDatabase(weaponName);
    }

    public void LoadWeaponDataFromDatabase(string weaponName)
    {
        WeaponData weaponData = Resources.Load<WeaponData>("WeaponsData/" + weaponName);
        damage = weaponData.damage;
        fireRate = weaponData.fireRate;
        distance = weaponData.distance;
        spread = weaponData.spread;
        minSpread = weaponData.minSpread;
        reloadDelay = weaponData.reloadDelay;
        armorPenetration = weaponData.armorPenetration;
        maxBulletsInMagazine = weaponData.maxBulletsInMagazine;
        if (bulletsInStock == 0)
        {
            bulletsInStock = weaponData.bulletsInStock;
            countBulletsInMagazine = weaponData.maxBulletsInMagazine;
        }
        countBulletsInShoot = weaponData.countBulletsInShoot;
        fist = weaponData.fist;
    }
}
