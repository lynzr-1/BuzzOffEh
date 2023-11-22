using UnityEngine;

[CreateAssetMenu(menuName = "Item")]

public class Item : ScriptableObject
{
    public string ObjectName;
    public Sprite sprite;
    public int quantity;
    public bool stackable;
    
    public enum ItemType
    {
        NANAIMO,
        HEALTH
    }
    public ItemType itemType;

}
