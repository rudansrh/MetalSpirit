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

    public bool AddItem(ItemType type, float amount, string itemText = "")
    {
        return AddItem(type, amount, 1, itemText);
    }

    public bool AddItem(ItemType type, float amount, int count, string itemText = "")
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
                if (!string.IsNullOrEmpty(itemText)) items[i].itemText = itemText; // 텍스트 갱신
                RefreshInventoryUI();
                return true;
            }
        }

        // 빈 슬롯 찾기
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type == ItemType.Empty)
            {
                items[i] = new InventoryItem(type, amount, count, itemText); // 텍스트 저장
                RefreshInventoryUI();
                return true;
            }
        }
        Debug.Log("인벤토리 가득 참");
        return false;
    }

    public bool HasItem(ItemType type, int count = 1)
    {
        if (type == ItemType.Empty || count <= 0)
        {
            return false;
        }

        int slotIndex = FindItemSlotIndex(type);
        return slotIndex >= 0 && items[slotIndex].count >= count;
    }

    public bool TryConsumeItem(ItemType type, int count = 1)
    {
        if (type == ItemType.Empty || count <= 0)
        {
            return false;
        }

        int slotIndex = FindItemSlotIndex(type);
        if (slotIndex < 0 || items[slotIndex].count < count)
        {
            return false;
        }

        items[slotIndex].count -= count;
        if (items[slotIndex].count <= 0)
        {
            items[slotIndex].type = ItemType.Empty;
            items[slotIndex].amount = 0f;
            items[slotIndex].count = 1;
        }

        RefreshInventoryUI();
        return true;
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

    public void RemoveItem(ItemType type, int count = 1)
    {
        if (type == ItemType.Empty || count <= 0)
        {
            return;
        }

        int slotIndex = FindItemSlotIndex(type);
        if (slotIndex < 0 || items[slotIndex].count < count)
        {
            return;
        }

        items[slotIndex].count -= count;
        if (items[slotIndex].count <= 0)
        {
            items[slotIndex].type = ItemType.Empty;
            items[slotIndex].amount = 0f;
            items[slotIndex].count = 1;
        }

        RefreshInventoryUI();
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

            case ItemType.BinaryCode:
                if (DocumentUIManager.Instance != null)
                {
                    if (DocumentUIManager.Instance.isOpen)
                    {
                        DocumentUIManager.Instance.CloseDocument();
                        if (PlayerController.Instance != null)
                        {
                            PlayerController.Instance.isUIopen = false; // 조작 다시 활성화
                        }
                    }
                    else
                    {
                        DocumentUIManager.Instance.ShowDocument(item.itemText, null);
                        if (PlayerController.Instance != null)
                        {
                            PlayerController.Instance.isUIopen = true; // 읽는 동안 조작 막기
                        }
                    }
                }
                shouldConsume = false;
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

        if (--items[index].count == 0)
        {
            items[index].type = ItemType.Empty;
            items[index].amount = 0f;
            items[index].itemText = "";
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

    int FindItemSlotIndex(ItemType type)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].type == type)
            {
                return i;
            }
        }

        return -1;
    }
    
    public void RemoveScissors()
    {
        RemoveItem(ItemType.Scissors, 1);
    }
}
