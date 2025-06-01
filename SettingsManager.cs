// Управляет настройками игры, включая громкость шепота, разговоров и музыки
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private float whisperVolume = 0.5f; // Громкость шепота по умолчанию
    private float talkVolume = 0.5f; // Громкость разговоров по умолчанию
    private float musicVolume = 0.5f; // Громкость музыки по умолчанию

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveVolumes(float whisper, float talk, float music)
    {
        whisperVolume = Mathf.Clamp01(whisper);
        talkVolume = Mathf.Clamp01(talk);
        musicVolume = Mathf.Clamp(music, 0f, 0.5f); // Ограничиваем музыку до 0.5
        PlayerPrefs.SetFloat("WhisperVolume", whisperVolume);
        PlayerPrefs.SetFloat("TalkVolume", talkVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        Debug.Log("Настройки громкости сохранены!");
    }

    private void LoadVolumes()
    {
        whisperVolume = PlayerPrefs.GetFloat("WhisperVolume", 0.5f);
        talkVolume = PlayerPrefs.GetFloat("TalkVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        Debug.Log("Настройки громкости загружены!");
    }

    public float GetWhisperVolume() => whisperVolume;
    public float GetTalkVolume() => talkVolume;
    public float GetMusicVolume() => musicVolume;
}
