using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public int maxSlotCount { get; private set; } = 2;
    public InventoryItem[] items { get; private set; }

    public InventoryUIManager inventoryUI;

    private void Awake()
    {
        items = new InventoryItem[maxSlotCount];
        for (int i = 0; i < maxSlotCount; i++)
        {
            items[i] = new InventoryItem(ItemType.Empty, 0, 1);
        }
    }

    public bool AddItem(ItemType type, float amount)
    {
        // 같은 아이템 찾기
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type != ItemType.Empty && items[i].type == type)
            {
                items[i].count++;
                inventoryUI.UpdateInventoryUI(items);
                return true;
            }
        }

        // 빈 슬롯 찾기
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type == ItemType.Empty)
            {
                items[i] = new InventoryItem(type, amount, 1);
                inventoryUI.UpdateInventoryUI(items);
                return true;
            }
        }
        Debug.Log("인벤토리 가득 참");
        return false;
    }

    #region OnUseItem
    public void OnUseSlot1(InputValue value)
    {
        UseItem(0, gameObject);
    }

    public void OnUseSlot2(InputValue value)
    {
        UseItem(1, gameObject);
    }

    public void OnUseSlot3(InputValue value)
    {
        UseItem(2, gameObject);
    }

    public void OnUseSlot4(InputValue value)
    {
        UseItem(3, gameObject);
    }
    #endregion

    public void UseItem(int index, GameObject player)
    {
        if (items.Length <= index || items[index].type == ItemType.Empty) return;

        InventoryItem item = items[index];
        Debug.Log("아이템 사용");

        switch (item.type)
        {
            case ItemType.Health:
                player.GetComponent<Health>().RestoreHealth(item.amount);
                break;

            case ItemType.Stamina:
                player.GetComponent<Stamina>().RestoreStamina(item.amount);
                break;
        }

        if(--items[index].count == 0)
        {
            items[index].type = ItemType.Empty;
        }
        inventoryUI.UpdateInventoryUI(items);
    }

    public void AddSlot(int addedSlotCnt) //슬롯 칸수 확장
    {
        maxSlotCount += addedSlotCnt;
        InventoryItem[] newItems = new InventoryItem[maxSlotCount];
        for (int i = 0; i < maxSlotCount; i++)
        {
            newItems[i] = i < items.Length ? items[i] : new InventoryItem(ItemType.Empty, 0, 1);
        }

        items = newItems;
        inventoryUI.UpdateInventoryUI(items);
    }
}