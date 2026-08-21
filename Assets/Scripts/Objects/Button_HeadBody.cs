using System;
using UnityEngine;
using UnityEngine.Events;

public class Button_HeadBody : MonoBehaviour, IInteractable
{
    private string purpose = "버튼 누르기";
    public string Purpose => purpose;

    [SerializeField] private UnityEvent onClickEvent;

    public void Interact(GameObject interactor)
    {
        onClickEvent?.Invoke();
    }
}
