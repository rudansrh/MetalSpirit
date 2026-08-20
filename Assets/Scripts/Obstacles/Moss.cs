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

                // ÀÌ³¢ Á¦°Å
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("[°¡À§]°¡ ÇÊ¿ä");
            }
        }
    }
}