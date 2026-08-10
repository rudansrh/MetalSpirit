using System.Collections.Generic;
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
        if(PlayerController.Instance.GetComponent<PlayerAbilityManager>().canUseInventory)
        {
            UpdateInventoryUI(inventoryManager.items);
        }
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
            Image[] childImages = slots[i].GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                if (img.gameObject == slots[i])
                {
                    continue;
                }

                //자기 자신을 제외한 collider
                img.sprite = itemImages[(int)items[i].type];
                break;
            }
            slots[i].GetComponentInChildren<Text>().text = items[i].type==ItemType.Empty ? "" : "X" + items[i].count;
        }
    }
}
