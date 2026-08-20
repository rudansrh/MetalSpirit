using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private const string BgmVolumePrefKey = "AudioManager.BgmVolume";
    private const string SfxVolumePrefKey = "AudioManager.SfxVolume";

    [Header("BGM")]
    public AudioClip bgmClip;
    [SerializeField] private bool playSceneBgmOnAwake = true;
    [Range(0f, 1f)] public float Bvolume = 1f;
    private AudioSource bgmPlayer;

    [Header("SFX")]
    public AudioClip[] sfxClips;
    [Range(0f, 1f)] public float Svolume = 1f;
    public int Schannels = 8;
    private AudioSource[] sfxPlayers;

    private readonly List<BgmZoneTrigger> activeBgmZones = new List<BgmZoneTrigger>();
    private readonly Dictionary<Sfx, float> lastPlayTime = new Dictionary<Sfx, float>();
    private AudioClip sceneBgmClip;
    private int channelIdx;
    private bool initialized;

    public enum Sfx
    {
        Possession, Depossession, BossDeath, BossExplosion, BoxOpen, Dash,
        ElevatorArrive, ElevatorDing, EnemyDash, EnemyDeath, EnemyHit, EnemyKick,
        EnemyLazer, EnemyPunch, EnemyTalk, FloorDown, GameOver, HallBeforeBoss,
        HpHeal, ItemGet, Jump, LazerHit, MenuMove, MenuSelect, PaperFlip, PasswordCorrect,
        PasswordEnter, PasswordIncorrect, PlayerDash, PlayerHit, PlayerKick, PlayerLazer,
        PlayerPunch,PlayerRocketPunch, PlayerTalk, PlayerWalk, Save, Scissors, StaminaHeal, Thump, Water
    }

    [Header("SFX Spam Protection")]
    public float defaultCooldown = 0.05f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple AudioManager instances detected in the same scene. Replacing the previous instance.");
        }

        instance = this;

        LoadSavedVolumes();
        Init();
        sceneBgmClip = bgmClip;
        activeBgmZones.Clear();
        ApplyResolvedBgm();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        if (playSceneBgmOnAwake)
        {
            PlayBgm();
        }

        RefreshUiSfxBindings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUiSfxBindings();
    }

    public void RefreshUiSfxBindings()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            UiButtonSfxRelay relay = button.GetComponent<UiButtonSfxRelay>();
            if (relay == null)
            {
                relay = button.gameObject.AddComponent<UiButtonSfxRelay>();
            }

            relay.Configure(ResolveButtonClickSfx(button));
        }
    }

    private Sfx? ResolveButtonClickSfx(Button button)
    {
        if (button == null)
        {
            return null;
        }

        if (IsUnderTransformNamed(button.transform, "Password"))
        {
            return null;
        }

        if (HasComponentInParents<SaveSlotUIManager>(button.transform) || IsUnderTransformNamed(button.transform, "SavePanel"))
        {
            return Sfx.Save;
        }

        return Sfx.MenuSelect;
    }

    private static bool IsUnderTransformNamed(Transform target, string expectedName)
    {
        Transform current = target;
        while (current != null)
        {
            if (current.name == expectedName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HasComponentInParents<T>(Transform target) where T : Component
    {
        Transform current = target;
        while (current != null)
        {
            if (current.GetComponent<T>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /* private void OnDestroy()
    {
        if (instance == this && GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.BgmVolumeChanged -= HandleBgmVolumeChanged;
            GameSettingsManager.Instance.SfxVolumeChanged -= HandleSfxVolumeChanged;
        }
    } */

    private void Init()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform, false);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;

        int channelCount = Mathf.Max(1, Schannels);
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.SetParent(transform, false);

        sfxPlayers = new AudioSource[channelCount];
        for (int i = 0; i < channelCount; i++)
        {
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.bypassListenerEffects = true;
            sfxPlayers[i] = source;
        }

        ApplyAudioSourceVolumes();
    }

    /* private void SubscribeToSettings()
    {
        GameSettingsManager settingsManager = GameSettingsManager.EnsureInstance();

        settingsManager.BgmVolumeChanged -= HandleBgmVolumeChanged;
        settingsManager.SfxVolumeChanged -= HandleSfxVolumeChanged;
        settingsManager.BgmVolumeChanged += HandleBgmVolumeChanged;
        settingsManager.SfxVolumeChanged += HandleSfxVolumeChanged;

        ApplyVolumes(settingsManager.BgmVolume, settingsManager.SfxVolume);
    } */

    /* private void HandleBgmVolumeChanged(float volume)
    {
        Bvolume = Mathf.Clamp01(volume);
        if (bgmPlayer != null)
        {
            bgmPlayer.volume = Bvolume;
        }
    }

    private void HandleSfxVolumeChanged(float volume)
    {
        Svolume = Mathf.Clamp01(volume);

        if (sfxPlayers == null)
        {
            return;
        }

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (sfxPlayers[i] != null)
            {
                sfxPlayers[i].volume = Svolume;
            }
        }
    } */

    /* private void ApplyVolumes(float bgmVolume, float sfxVolume)
    {
        HandleBgmVolumeChanged(bgmVolume);
        HandleSfxVolumeChanged(sfxVolume);
    } */

    public void PlayBgm()
    {
        if (bgmPlayer == null || bgmClip == null)
        {
            return;
        }

        bgmPlayer.clip = bgmClip;
        if (!bgmPlayer.isPlaying)
        {
            bgmPlayer.Play();
        }
    }

    public void StopBgm()
    {
        if (bgmPlayer != null && bgmPlayer.isPlaying)
        {
            bgmPlayer.Stop();
        }
    }

    public void EnterBgmZone(BgmZoneTrigger zone)
    {
        if (zone == null)
        {
            return;
        }

        if (activeBgmZones.Contains(zone))
        {
            return;
        }

        activeBgmZones.Add(zone);
        ApplyResolvedBgm();
    }

    public void ExitBgmZone(BgmZoneTrigger zone)
    {
        if (zone == null)
        {
            return;
        }

        if (activeBgmZones.Remove(zone))
        {
            ApplyResolvedBgm();
        }
    }

    public void SetSceneBgm(AudioClip clip)
    {
        sceneBgmClip = clip;
        activeBgmZones.Clear();
        ApplyResolvedBgm();
    }

    public void SetBgmVolume(float volume)
    {
        Bvolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BgmVolumePrefKey, Bvolume);
        ApplyAudioSourceVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        Svolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, Svolume);
        ApplyAudioSourceVolumes();
    }

    private void OnValidate()
    {
        Bvolume = Mathf.Clamp01(Bvolume);
        Svolume = Mathf.Clamp01(Svolume);
        ApplyAudioSourceVolumes();
    }

    private void ApplyAudioSourceVolumes()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.volume = Bvolume;
        }

        if (sfxPlayers == null)
        {
            return;
        }

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (sfxPlayers[i] != null)
            {
                sfxPlayers[i].volume = Svolume;
            }
        }
    }

    private void LoadSavedVolumes()
    {
        if (PlayerPrefs.HasKey(BgmVolumePrefKey))
        {
            Bvolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumePrefKey));
        }

        if (PlayerPrefs.HasKey(SfxVolumePrefKey))
        {
            Svolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefKey));
        }
    }

    private void ApplyResolvedBgm()
    {
        AudioClip targetClip = sceneBgmClip;
        BgmZoneTrigger activeZone = GetActiveBgmZone();
        if (activeZone != null && activeZone.BgmClip != null)
        {
            targetClip = activeZone.BgmClip;
        }

        SetResolvedBgm(targetClip);
    }

    private BgmZoneTrigger GetActiveBgmZone()
    {
        for (int i = activeBgmZones.Count - 1; i >= 0; i--)
        {
            if (activeBgmZones[i] == null)
            {
                activeBgmZones.RemoveAt(i);
            }
        }

        BgmZoneTrigger selectedZone = null;
        int highestPriority = int.MinValue;
        int selectedIndex = -1;

        for (int i = 0; i < activeBgmZones.Count; i++)
        {
            BgmZoneTrigger zone = activeBgmZones[i];
            if (zone == null)
            {
                continue;
            }

            if (zone.Priority > highestPriority || (zone.Priority == highestPriority && i > selectedIndex))
            {
                highestPriority = zone.Priority;
                selectedIndex = i;
                selectedZone = zone;
            }
        }

        return selectedZone;
    }

    private void SetResolvedBgm(AudioClip clip)
    {
        bgmClip = clip;

        if (bgmPlayer == null)
        {
            return;
        }

        if (clip == null)
        {
            bgmPlayer.Stop();
            bgmPlayer.clip = null;
            return;
        }

        bool wasPlaying = bgmPlayer.isPlaying;
        bool clipChanged = bgmPlayer.clip != clip;
        if (clipChanged)
        {
            bgmPlayer.Stop();
            bgmPlayer.clip = clip;
        }
        else if (bgmPlayer.clip == null)
        {
            bgmPlayer.clip = clip;
        }

        bool shouldAutoPlay = playSceneBgmOnAwake || wasPlaying;
        if (shouldAutoPlay && !bgmPlayer.isPlaying)
        {
            bgmPlayer.Play();
        }
    }

    private float GetCooldown(Sfx sfx)
    {
        switch (sfx)
        {
            default:
                return defaultCooldown;
        }
    }

    private bool CanPlayNow(Sfx sfx)
    {
        float now = Time.unscaledTime;
        float cooldown = GetCooldown(sfx);

        if (lastPlayTime.TryGetValue(sfx, out float last) && now - last < cooldown)
        {
            return false;
        }

        lastPlayTime[sfx] = now;
        return true;
    }

    public void PlaySfx(Sfx sfx)
    {
        if (!CanPlayNow(sfx) || sfxPlayers == null || sfxPlayers.Length == 0)
        {
            return;
        }

        int clipIdx = (int)sfx;
        if (sfxClips == null || clipIdx < 0 || clipIdx >= sfxClips.Length || sfxClips[clipIdx] == null)
        {
            return;
        }

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIdx = (i + channelIdx) % sfxPlayers.Length;
            if (sfxPlayers[loopIdx].isPlaying)
            {
                continue;
            }

            channelIdx = loopIdx;
            sfxPlayers[loopIdx].clip = sfxClips[clipIdx];
            sfxPlayers[loopIdx].Play();
            return;
        }

        int stealIdx = channelIdx;
        channelIdx = (channelIdx + 1) % sfxPlayers.Length;

        sfxPlayers[stealIdx].Stop();
        sfxPlayers[stealIdx].clip = sfxClips[clipIdx];
        sfxPlayers[stealIdx].Play();
    }
}

public sealed class UiButtonSfxRelay : MonoBehaviour, IPointerEnterHandler
{
    private Button button;
    private AudioManager.Sfx? clickSfx;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Configure(AudioManager.Sfx? nextClickSfx)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(HandleClick);
        clickSfx = nextClickSfx;

        if (clickSfx.HasValue)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.MenuMove);
    }

    private void HandleClick()
    {
        if (!clickSfx.HasValue)
        {
            return;
        }

        AudioManager.instance?.PlaySfx(clickSfx.Value);
    }
}
