using UnityEngine;

public interface IInteractable
{
    public string Purpose { get; }
    // 상호작용을 실행한 주체
    void Interact(GameObject interactor);
}