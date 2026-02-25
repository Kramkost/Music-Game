using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RhythmSequencer : MonoBehaviour
{
    [SerializeField] private float bpm = 120f;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    private AudioSource audioSource;
    private float beatInterval;
    private int lastBeatIndex = -1;
    private bool isPlaying = false; // Флаг активности

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        beatInterval = 60f / bpm;
    }

    private void OnEnable()
    {
        // Подписываемся на старт игры
        GameEventManager.OnGameStart += StartRhythm;
    }

    private void OnDisable()
    {
        GameEventManager.OnGameStart -= StartRhythm;
    }

    private void StartRhythm()
    {
        isPlaying = true;
        audioSource.Play(); // Запускаем музыку только сейчас!
    }

    private void Update()
    {
        // Если игра не началась или музыка не играет — ничего не спавним
        if (!isPlaying || !audioSource.isPlaying) return;

        float trackTime = (float)audioSource.timeSamples / audioSource.clip.frequency;
        int currentBeatIndex = Mathf.FloorToInt(trackTime / beatInterval);

        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            GameEventManager.OnBeatPulse?.Invoke();
            SpawnNote();
        }
    }

    private void SpawnNote()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(notePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}