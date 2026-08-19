using UnityEngine;

public class MossObstacle : MonoBehaviour, IInteractable
{
    [Header("Hidden Object (Optional)")]
    public GameObject objectToReveal;

    private void Start()
    {
        if (objectToReveal != null)
        {
            objectToReveal.SetActive(false);
        }
    }

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
        {
            if (inventoryManager.TryConsumeItem(ItemType.Scissors, 1))
            {
                if (objectToReveal != null)
                {
                    objectToReveal.SetActive(true);
                }

                // 이끼 제거
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("[가위]가 필요");
            }
        }
    }
}