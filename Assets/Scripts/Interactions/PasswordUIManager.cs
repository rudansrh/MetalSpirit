using System.Collections;
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
    Coroutine closeAfterSuccessCoroutine;
    bool isClosingAfterSuccess;

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

        if (closeAfterSuccessCoroutine != null)
        {
            StopCoroutine(closeAfterSuccessCoroutine);
            closeAfterSuccessCoroutine = null;
        }

        isClosingAfterSuccess = false;

        currentPassword = password;
        currentPlayer = interactor != null ? interactor.GetComponent<PlayerController>() : null;
        currentInput = string.Empty;

        if (currentPlayer != null)
        {
            currentPlayer.canMove = false;
        }

        passwordUI.SetActive(true);
        RefreshInputText();
        SetMessage(password.DefaultMessage);
    }

    public void Close()
    {
        if (isClosingAfterSuccess)
        {
            return;
        }

        CloseInternal();
    }

    void CloseInternal()
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
        closeAfterSuccessCoroutine = null;
        isClosingAfterSuccess = false;
        RefreshInputText();
        SetMessage(string.Empty);

        PlayerController.Instance.isUIopen = false;
    }

    public void InputNumber(string number)
    {
        if (isClosingAfterSuccess || currentPassword == null || string.IsNullOrEmpty(number))
        {
            return;
        }

        if (currentInput.Length >= currentPassword.MaxInputLength)
        {
            return;
        }

        currentInput += number;
        RefreshInputText();
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.PasswordEnter); //***
    }

    public void RemoveLastNumber()
    {
        if (isClosingAfterSuccess || string.IsNullOrEmpty(currentInput))
        {
            return;
        }

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        RefreshInputText();
    }

    public void Submit()
    {
        if (isClosingAfterSuccess || currentPassword == null)
        {
            return;
        }

        bool isCorrect = currentPassword.Validate(currentInput, out string message);
        SetMessage(message);

        if (isCorrect)
        {
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.PasswordCorrect); //***
            closeAfterSuccessCoroutine = StartCoroutine(CloseAfterSuccess(currentPassword));
            return;
        }

        if (!isCorrect)
        {
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.PasswordIncorrect); //***
            currentInput = string.Empty;
            RefreshInputText();
        }
    }

    IEnumerator CloseAfterSuccess(Password solvedPassword)
    {
        isClosingAfterSuccess = true;

        float waitTime = solvedPassword != null ? Mathf.Max(0f, solvedPassword.ReamainTime) : 0f;
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        CloseInternal();

        if (solvedPassword != null)
        {
            solvedPassword.CompleteSuccessfulInteraction();
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
