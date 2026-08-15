using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Password : MonoBehaviour, IInteractable
{
    [Header("Password Settings")]
    [SerializeField] string correctPassword = "";
    [SerializeField] string successMessage = "암호가 일치합니다.";
    [SerializeField] string failureMessage = "암호가 틀렸습니다.";
    [SerializeField] bool stayUnlockedAfterSuccess = true;
    [SerializeField] UnityEvent onPasswordMatched;

    bool isUnlocked;

    public int MaxInputLength => string.IsNullOrEmpty(correctPassword) ? 8 : correctPassword.Length;
    public string DefaultMessage => "암호를 입력하세요. (E / ESC 닫기)";

    public void Interact(GameObject interactor)
    {
        if (stayUnlockedAfterSuccess && isUnlocked)
        {
            gameObject.SetActive(false);
            Debug.Log($"{name}: 이미 해제된 암호입니다.");
            return;
        }

        if (PasswordUIManager.Instance == null)
        {
            Debug.LogWarning("PasswordUIManager가 씬에 없습니다.");
            return;
        }

        PasswordUIManager.Instance.Open(this, interactor);
        PlayerController.Instance.isUIopen = true;
    }

    public bool Validate(string input, out string resultMessage)
    {
        if (input == correctPassword)
        {
            resultMessage = successMessage;
            Debug.Log($"{name}: {resultMessage}");

            if (stayUnlockedAfterSuccess)
            {
                isUnlocked = true;
            }

            PasswordUIManager.Instance.unlocked = true;
            StartCoroutine(openDoor());
           
            return true;
        }

        resultMessage = failureMessage;
        Debug.Log($"{name}: {resultMessage}");
        return false;
    }

    IEnumerator openDoor()
    {
        yield return new WaitForSeconds(1f);
        onPasswordMatched?.Invoke();
        gameObject.SetActive(false);
    }

    [ContextMenu("Reset Unlock State")]
    public void ResetUnlockState()
    {
        isUnlocked = false;
    }
}
