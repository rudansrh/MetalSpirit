using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] Sprite[] itemImages;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform slotParent;

    List<GameObject> slots = new List<GameObject>();
    InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = PlayerController.Instance.GetComponent<InventoryManager>();
        inventoryManager.inventoryUI = this;
        UpdateInventoryUI(inventoryManager.items); //시작할때 인벤토리UI보이기 (임시)
    }

    public void UpdateInventoryUI(InventoryItem[] items) //인벤토리 UI 업데이트
    {
        int t = inventoryManager.maxSlotCount - slots.Count;
        for (int i = 0; i < t; i++)
        {
            slots.Add(Instantiate(slotPrefab, slotParent));
        }

        for (int i = 0; i < inventoryManager.maxSlotCount; i++)
        {
            slots[i].GetComponent<Image>().sprite = itemImages[(int)items[i].type];
            slots[i].GetComponentInChildren<Text>().text = items[i].type==ItemType.Empty ? "" : "X" + items[i].count;
        }
    }
}
