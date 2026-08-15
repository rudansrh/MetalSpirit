using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public int maxSlotCount = 0;
    public InventoryItem[] items { get; private set; }

    public InventoryUIManager inventoryUI;

    private void Awake()
    {
        if (items != null) return;

        items = new InventoryItem[maxSlotCount];
        for (int i = 0; i < maxSlotCount; i++)
        {
            items[i] = new InventoryItem(ItemType.Empty, 0, 1);
        }
    }

    public bool AddItem(ItemType type, float amount)
    {
        return AddItem(type, amount, 1);
    }

    public bool AddItem(ItemType type, float amount, int count)
    {
        if (type == ItemType.Empty || count <= 0)
        {
            return false;
        }

        // 같은 아이템 찾기
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type != ItemType.Empty && items[i].type == type)
            {
                items[i].count += count;
                items[i].amount = amount;
                RefreshInventoryUI();
                return true;
            }
        }

        // 빈 슬롯 찾기
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type == ItemType.Empty)
            {
                items[i] = new InventoryItem(type, amount, count);
                RefreshInventoryUI();
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
    public void OnUseSlot5(InputValue value)
    {
        UseItem(4, gameObject);
    }
    #endregion

    public void UseItem(int index, GameObject player)
    {
        if (items.Length <= index || items[index].type == ItemType.Empty) return;

        InventoryItem item = items[index];
        bool shouldConsume = true;

        switch (item.type)
        {
            case ItemType.Health:
                player.GetComponent<Health>().RestoreHealth(item.amount);
                break;

            case ItemType.Stamina:
                player.GetComponent<Stamina>().RestoreStamina(item.amount);
                break;

            default:
                shouldConsume = false;
                Debug.Log($"{item.type} is not consumed by direct slot use.");
                break;
        }

        if (!shouldConsume)
        {
            return;
        }

        Debug.Log("아이템 사용");

        if(--items[index].count == 0)
        {
            items[index].type = ItemType.Empty;
            items[index].amount = 0f;
        }
        RefreshInventoryUI();
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
        RefreshInventoryUI();
    }

    public void LoadInventory(InventoryItem[] savedItems)
    {
        if (savedItems == null || savedItems.Length == 0) return;

        items = savedItems;
        maxSlotCount = savedItems.Length;

        if (inventoryUI != null)
        {
            RefreshInventoryUI();
        }

        Debug.Log("인벤토리 로드");
    }

    void RefreshInventoryUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(items);
        }
    }
}
