using UnityEngine;
using UnityEngine.UI;

public class SettingUIManager : MonoBehaviour
{
    public static SettingUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider brightnessSlider;

    private bool listenersRegistered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    private void Start()
    {
        RegisterSliderListeners();
        SyncSlidersWithAudioSettings();
    }

    private void OnDestroy()
    {
        UnregisterSliderListeners();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsOpen => settingPanel != null && settingPanel.activeSelf;

    public void ToggleSetting()
    {
        if (IsOpen)
        {
            CloseSetting();
            return;
        }

        OpenSetting();
    }

    public void OpenSetting()
    {
        SyncSlidersWithAudioSettings();
        gameObject.SetActive(true);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = true;
        }
    }

    public void CloseSetting()
    {
        gameObject.SetActive(false);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isUIopen = false;
        }
    }

    private void RegisterSliderListeners()
    {
        if (listenersRegistered)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(HandleBgmSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(HandleSfxSliderChanged);
        }

        listenersRegistered = true;
    }

    private void UnregisterSliderListeners()
    {
        if (!listenersRegistered)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(HandleBgmSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);
        }

        listenersRegistered = false;
    }

    private void SyncSlidersWithAudioSettings()
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(AudioManager.instance.Bvolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.instance.Svolume);
        }
    }

    private void HandleBgmSliderChanged(float value)
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        AudioManager.instance.SetBgmVolume(value);
    }

    private void HandleSfxSliderChanged(float value)
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        AudioManager.instance.SetSfxVolume(value);
    }
}
