using UnityEngine;
using UnityEngine.UI;

public class TurotialUIManager : MonoBehaviour
{
    public static TurotialUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text contentText;
    [SerializeField] private Text[] pages;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject nextButton;

    private int totalPages;
    private int currentPageIndex = 0;

    public bool IsOpen => tutorialPanel != null && tutorialPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        totalPages = pages.Length;
    }

    void Start()
    {
        totalPages = pages.Length;
    }

    public void ShowPrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            contentText.text = pages[currentPageIndex].text;

            if (currentPageIndex == 0)
            {
                prevButton.SetActive(false);
            }
            else
            {
                nextButton.SetActive(true);
            }
        }
    }

    public void ShowNextPage()
    {
        if (currentPageIndex < totalPages - 1)
        {
            currentPageIndex++;
            contentText.text = pages[currentPageIndex].text;

            if (currentPageIndex == totalPages - 1)
            {
                nextButton.SetActive(false);
            }
            else
            {
                prevButton.SetActive(true);
            }
        }
    }

    public void ToggleTutorial()
    {
        if (IsOpen)
        {
            CloseTutorial();
        }
        else
        {
            OpenTutorial();
        }
    }

    public void OpenTutorial()
    {
        currentPageIndex = 0;
        contentText.text = pages[0].text;
        prevButton.SetActive(false);
        nextButton.SetActive(true);

        gameObject.SetActive(true);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = true;
        }
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = false;
        }
    }
}
