using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ArmorBottom : MonoBehaviour
{
    Collider2D Player;
    GameObject TakeItemButton;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == ("Player"))
        {
            Player = collision;
            GameObject _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            TakeItemButton.SetActive(true);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == Player)
        {
            Player = null;
            TakeItemButton.SetActive(false);
        }
    }
    public void TakeArmor()
    {
        Player.gameObject.GetComponentInParent<PlayerController>().TakeArmorRpc(Player.gameObject.GetComponent<PlayerController>());
    }
}
