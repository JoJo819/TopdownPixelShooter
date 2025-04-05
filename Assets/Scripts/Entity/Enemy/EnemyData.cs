using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemys/EnemyData")]
public class EnemyData : MonoBehaviour
{
    public float Health;
    public float DropChance;
    public float EnemyFireRate;

    public float EnemySpeed;
    public string EnemyName;
}
