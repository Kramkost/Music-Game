/* =============================================================================
СКРИПТ: LevelRestarter.cs
НАЗНАЧЕНИЕ: Управляет перезапуском игры с ЖЕЛЕЗОБЕТОННОЙ защитой от багов аудио.
ЧТО ИЗМЕНИЛОСЬ:
1. Автоматически находит ВСЕ AudioSource на сцене.
2. Буквально "отматывает" время (эффект диджейской перемотки за 1 секунду).
3. Снимает баг плагина YG2, из-за которого пропадал звук (AudioListener.pause).
4. Сбрасывает спавнер, чтобы блоки снова начали лететь!
=============================================================================
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Reflection; 
using YG;

public class LevelRestarter : MonoBehaviour
{
    [Header("Настройки перемотки")]
    [Tooltip("На сколько секунд отматываем трек после рекламы")]
    [SerializeField] private float rewindSeconds = 5f; 
    
    [Tooltip("За сколько секунд происходит анимация самой отмотки")]
    [SerializeField] private float rewindAnimationTime = 1f;

    private void Awake()
    {
        
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        Time.timeScale = 1f;
    }

    public void RestartNormal()
    {
        
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartWithRewardedAd()
    {
        YG2.RewardedAdvShow("Rewind5Sec", () =>
        {
            
            StartCoroutine(RewindRoutine());
        });
    }

    // ==========================================
    // ЭПИЧНАЯ ОТМОТКА ВРЕМЕНИ
    // ==========================================
private IEnumerator RewindRoutine()
    {
      
        NoteObject[] activeNotes = FindObjectsOfType<NoteObject>();
        foreach (NoteObject note in activeNotes)
        {
            Destroy(note.gameObject);
        }

        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();

        float elapsed = 0f;
        float[] startTimes = new float[allAudio.Length];
        float[] targetTimes = new float[allAudio.Length];

        for (int i = 0; i < allAudio.Length; i++)
        {
            startTimes[i] = allAudio[i].time;
            targetTimes[i] = Mathf.Max(0f, startTimes[i] - rewindSeconds);
            allAudio[i].pitch = -2f; 
            if (!allAudio[i].isPlaying) allAudio[i].Play();
        }

        while (elapsed < rewindAnimationTime)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = elapsed / rewindAnimationTime;
            for (int i = 0; i < allAudio.Length; i++)
            {
                allAudio[i].time = Mathf.Lerp(startTimes[i], targetTimes[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < allAudio.Length; i++)
        {
            allAudio[i].pitch = 1f;
            if (!allAudio[i].isPlaying) allAudio[i].Play();
        }



        SpriteEvolver playerEvolver = FindObjectOfType<SpriteEvolver>();
        if (playerEvolver != null)
        {
            playerEvolver.AutoRevive();
        }

       
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
       
        GameEventManager.OnGameStart?.Invoke();
        
        Debug.Log("Время отмотано, игрок воскрешен!");
    }
}