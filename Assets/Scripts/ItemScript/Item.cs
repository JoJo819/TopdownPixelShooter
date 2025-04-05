using UnityEngine;

public enum ItemType { Weapon, Armor, Potion, Misc }

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public float timeToCollect;
    public float collectRadius;

    public interface IItem
    {
    }
}