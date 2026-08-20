using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemType type;
    public float amount; //체력, 스태미나 회복량
    public int count; //아이템 보유량


    public InventoryItem(ItemType type, float amount, int count = 1)
    {
        this.type = type;
        this.amount = amount;
        this.count = count;
    }
}