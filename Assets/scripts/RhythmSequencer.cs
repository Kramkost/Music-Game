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

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        beatInterval = 60f / bpm;
    }

    private void Update()
    {
        if (!audioSource.isPlaying) return;

        // Точное время трека в секундах на основе сэмплов
        float trackTime = (float)audioSource.timeSamples / audioSource.clip.frequency;
        int currentBeatIndex = Mathf.FloorToInt(trackTime / beatInterval);

        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            GameEventManager.OnBeatPulse?.Invoke();
            
            // Простейший спавн на каждый бит (можно усложнить паттернами)
            SpawnNote();
        }
    }

    private void SpawnNote()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(notePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}