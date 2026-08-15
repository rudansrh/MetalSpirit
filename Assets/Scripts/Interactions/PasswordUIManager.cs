using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class PasswordUIManager : MonoBehaviour
{
    public static PasswordUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] GameObject passwordUI;
    [SerializeField] Text inputText;
    [SerializeField] Text messageText;

    Password currentPassword;
    PlayerController currentPlayer;
    string currentInput = string.Empty;
    public bool unlocked = false;

    public static bool IsUiOpen => Instance != null && Instance.passwordUI != null && Instance.passwordUI.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (passwordUI != null)
        {
            passwordUI.SetActive(false);
        }
    }

    public void Open(Password password, GameObject interactor)
    {
        if (password == null || passwordUI == null)
        {
            return;
        }

        currentPassword = password;
        currentPlayer = interactor != null ? interactor.GetComponent<PlayerController>() : null;
        currentInput = string.Empty;

        if (currentPlayer != null)
        {
            currentPlayer.canMove = false;
        }

        passwordUI.SetActive(true);
        RefreshInputText();
        SetMessage("암호를 입력하세요.");
    }

    public void Close()
    {
        if (passwordUI != null)
        {
            passwordUI.SetActive(false);
        }

        if (currentPlayer != null)
        {
            currentPlayer.canMove = true;
        }

        currentPassword = null;
        currentPlayer = null;
        currentInput = string.Empty;
        RefreshInputText();
        SetMessage(string.Empty);

        PlayerController.Instance.isUIopen = false;
    }

    public void InputNumber(string number)
    {
        if (currentPassword == null || string.IsNullOrEmpty(number) || unlocked)
        {
            return;
        }

        if (currentInput.Length >= currentPassword.MaxInputLength)
        {
            return;
        }

        currentInput += number;
        RefreshInputText();
    }

    public void RemoveLastNumber()
    {
        if (string.IsNullOrEmpty(currentInput) || unlocked)
        {
            return;
        }

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        RefreshInputText();
    }

    public void Submit()
    {
        if (currentPassword == null || unlocked)
        {
            return;
        }

        bool isCorrect = currentPassword.Validate(currentInput, out string message);
        SetMessage(message);

        if (!isCorrect)
        {
            currentInput = string.Empty;
            RefreshInputText();
        }
    }

    void RefreshInputText()
    {
        string text = string.IsNullOrEmpty(currentInput) ? string.Empty : currentInput;

        if (inputText != null)
        {
            inputText.text = text;
        }
    }

    void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
