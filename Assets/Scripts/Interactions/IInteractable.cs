using UnityEngine;

public interface IInteractable
{
    // 상호작용을 실행한 주체
    void Interact(GameObject interactor);
}