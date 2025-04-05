using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/WeaponData")]
public class WeaponData : Item
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
    public int countBulletsInShoot;
}
