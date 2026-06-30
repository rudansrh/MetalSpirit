using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStatsUIManager : MonoBehaviour
{
    const string HealthSliderName = "Slider_Health";
    const string StaminaSliderName = "Slider_Stamina";
    const string HealthValueTextName = "Text_HealthValue";
    const string StaminaValueTextName = "Text_StaminaValue";

    [Header("UI References")]
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider staminaSlider;
    [SerializeField] Text healthValueText;
    [SerializeField] Text staminaValueText;

    Health health;
    Stamina stamina;

    // 싱글톤 패턴을 사용하여 UIManager 초기화하고, Scene이 로드될 때마다 UI를 갱신하도록 설정
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindFirstObjectByType<PlayerStatsUIManager>() != null)
        {
            return;
        }

        GameObject uiManagerObject = new GameObject(nameof(PlayerStatsUIManager));
        DontDestroyOnLoad(uiManagerObject);
        uiManagerObject.AddComponent<PlayerStatsUIManager>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void Start()
    {
        BindToScene();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnsubscribeFromStats();
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindToScene();
    }

    void BindToScene()
    {
        UnsubscribeFromStats();
        CacheUiReferences();
        CacheStatReferences();
        SubscribeToStats();
        RefreshAll();
    }

    // UI 참조를 캐싱
    void CacheUiReferences()
    {
        healthSlider = FindNamedComponent(healthSlider, HealthSliderName);
        staminaSlider = FindNamedComponent(staminaSlider, StaminaSliderName);
        healthValueText = FindNamedComponent(healthValueText, HealthValueTextName);
        staminaValueText = FindNamedComponent(staminaValueText, StaminaValueTextName);
    }

    // Stats 참조를 캐싱
    void CacheStatReferences()
    {
        health = FindFirstObjectByType<Health>();
        stamina = FindFirstObjectByType<Stamina>();
    }

    // Stats 이벤트에 구독
    void SubscribeToStats()
    {
        if (health != null)
        {
            health.OnHealthChanged += RefreshHealthUI;
        }

        if (stamina != null)
        {
            stamina.OnStaminaChanged += RefreshStaminaUI;
        }
    }

    // Stats 이벤트 구독 해제
    void UnsubscribeFromStats()
    {
        if (health != null)
        {
            health.OnHealthChanged -= RefreshHealthUI;
        }

        if (stamina != null)
        {
            stamina.OnStaminaChanged -= RefreshStaminaUI;
        }
    }

    // 모든 UI를 갱신
    void RefreshAll()
    {
        RefreshHealthUI();
        RefreshStaminaUI();
    }

    // 체력 UI 갱신
    void RefreshHealthUI()
    {
        if (health == null)
        {
            return;
        }

        UpdateStatDisplay(healthSlider, healthValueText, health.CurrentHealth, health.MaxHealth);
    }

    // 스태미나 UI 갱신
    void RefreshStaminaUI()
    {
        if (stamina == null)
        {
            return;
        }

        UpdateStatDisplay(staminaSlider, staminaValueText, stamina.CurrentStamina, stamina.MaxStamina);
    }

    // UI 슬라이더와 텍스트를 갱신하는 공통 메서드
    void UpdateStatDisplay(Slider slider, Text valueText, float currentValue, float maxValue)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = maxValue;
            slider.value = currentValue;
        }

        if (valueText != null)
        {
            valueText.text = $"{Mathf.RoundToInt(currentValue)} / {Mathf.RoundToInt(maxValue)}";
        }
    }

    // 이름으로 컴포넌트를 찾는 유틸리티 메서드
    static T FindNamedComponent<T>(T currentReference, string objectName) where T : Component
    {
        if (currentReference != null)
        {
            return currentReference;
        }

        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponent<T>();
    }
}
