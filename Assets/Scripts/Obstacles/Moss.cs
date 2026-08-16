using UnityEngine;

public class MossObstacle : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {

        if (interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
        {
            if (inventoryManager.TryConsumeItem(ItemType.Scissors, 1))
            {
                Destroy(gameObject);
            }
            else
            {

                Debug.Log("가위 필요");
            }
        }
    }
}