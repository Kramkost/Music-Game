using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using YG; // Подключаем пространство имен YG

public class AudioSettings : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Имена параметров в Микшере")]
    [SerializeField] private string musicParam = "MusicVol";
    [SerializeField] private string sfxParam = "SFXVol";

    private void OnEnable()
    {
        // В v2 событие называется onGetSDKData
        YG2.onGetSDKData += LoadVolume;
        
        // Если данные уже загружены, применяем их сразу
        if (YG2.isSDKEnabled) // Проверка инициализации в v2
        {
            LoadVolume();
        }
    }

    private void OnDisable()
    {
        YG2.onGetSDKData -= LoadVolume;

        // Сохраняем прогресс при закрытии окна настроек
        YG2.SaveProgress();
    }

    void Start()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        
        // Попытка загрузки на старте
        LoadVolume();
    }

    private void LoadVolume()
    {
        // В версии v2 доступ к сохранениям через YG2.saves
        float musicVol = YG2.saves.musicVolume;
        float sfxVol = YG2.saves.sfxVolume;

        // Обновляем слайдеры без вызова событий (если нужно, можно временно отключить listener)
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        // Применяем к микшеру
        UpdateMixer(musicParam, musicVol);
        UpdateMixer(sfxParam, sfxVol);
    }

    public void SetMusicVolume(float value)
    {
        UpdateMixer(musicParam, value);
        
        // Запись в v2
        YG2.saves.musicVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        UpdateMixer(sfxParam, value);
        
        // Запись в v2
        YG2.saves.sfxVolume = value;
    }

    private void UpdateMixer(string parameterName, float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        audioMixer.SetFloat(parameterName, dbValue);
    }
}