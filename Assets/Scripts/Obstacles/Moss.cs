using UnityEngine;

public class MossObstacle : MonoBehaviour, IInteractable
{
    private string purpose = "이끼 치우기";
    public string Purpose => purpose;
    public void Interact(GameObject interactor)
    {

        if (interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
        {
            if (inventoryManager.TryConsumeItem(ItemType.Scissors, 1))
            {
                AudioManager.instance?.PlaySfx(AudioManager.Sfx.Scissors); //***
                Destroy(gameObject);
            }
            else
            {

                Debug.Log("가위 필요");
            }
        }
    }
}
