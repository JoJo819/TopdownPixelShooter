using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData EnemyData;
    public Weapon Gun;
    public float Health;
    public float Armor;
    private void Start()
    {
        Gun = gameObject.GetComponent<Weapon>();
        Health = EnemyData.Health;
    }
    public void ApplyDamage(float damage, float armorPenetration)
    {
        float damageToArmor = damage * (1 - armorPenetration / 100);
        float damageToHP = damage * (armorPenetration / 100);

        if (Armor > 0 && Health > 0)
        {
            if (Armor >= damageToArmor)
                Armor -= damageToArmor;
            else
            {
                float remainingDamage = damageToArmor - Armor;
                Armor = 0;
                damageToHP += remainingDamage;
            }
            Health -= Mathf.Round(damageToHP);
            if (Health <= 0)
            {
                Armor = 0;
                if (Gun.weaponName != null)
                {
                    SpawnGun();
                }

                GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), transform.position, Quaternion.identity);
            }
            return;
        }
        else
        {
            Health -= Mathf.Round(damage);
        }

        if (Health <= 0)
        {
            Armor = 0;
            if (Gun.weaponName != null)
            {
                SpawnGun();
            }

            GameObject Tombstone = Instantiate(Resources.Load<GameObject>("Prefabs/Death"), transform.position, Quaternion.identity);
        }
    }

    public void SpawnGun()
    {

        // Создаем объект оружия перед игроком
        GameObject myGun = Instantiate(Resources.Load<GameObject>("Guns/" + Gun.weaponName), transform.position + transform.up * 2f, Quaternion.identity);

        // Устанавливаем параметры оружия
        myGun.GetComponent<Weapon>().countBulletsInMagazine = Gun.countBulletsInMagazine;
        myGun.GetComponent<Weapon>().bulletsInStock = Gun.bulletsInStock;

        // Добавляем силу выброса вперед
        Rigidbody2D rb = myGun.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Вычисляем направление движения в 2D
            rb.AddForce(transform.up * 40, ForceMode2D.Impulse); // Сила выброса вперед
            StartCoroutine(SlowDownObject(rb));
        }

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
}
