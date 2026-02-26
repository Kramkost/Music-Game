using UnityEngine;

/* =============================================================================
СКРИПТ: RhythmSequencer.cs
НАЗНАЧЕНИЕ: Сердце нашей ритм-игры. Главный спавнер, который работает СТРОГО под бит музыки.
ЧТО ДЕЛАЕТ: 
1. Читает аудиодорожку и математически вычисляет идеальные тайминги для битов.
2. В нужный момент (в такт) выбирает случайную точку спавна.
3. Достает рандомный блок из массива (Обычный, Убийственный или Длинный).
4. Задает блоку случайное направление для удара (как в Beat Saber).
=============================================================================
*/

[RequireComponent(typeof(AudioSource))]
public class RhythmSequencer : MonoBehaviour
{
    [SerializeField] private float bpm = 120f;
    
    // ИСПРАВЛЕНО: Теперь тут массив (GameObject[]), чтобы мы могли закинуть разные типы блоков!
    [SerializeField] private GameObject[] notePrefabs; 
    [SerializeField] private Transform[] spawnPoints;
    
    private AudioSource audioSource;
    private float beatInterval;
    private int lastBeatIndex = -1;
    private bool isPlaying = false; 

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        beatInterval = 60f / bpm;
    }

    private void OnEnable()
    {
        GameEventManager.OnGameStart += StartRhythm;
    }

    private void OnDisable()
    {
        GameEventManager.OnGameStart -= StartRhythm;
    }

    private void StartRhythm()
    {
        isPlaying = true;
        audioSource.Play(); 
    }

    private void Update()
    {
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
        
        // ИСПРАВЛЕНО: Используем квадратные скобки [] для массивов, а не круглые ()
        GameObject randomPrefab = notePrefabs[Random.Range(0, notePrefabs.Length)];
        
        // ИСПРАВЛЕНО: Спавним именно randomPrefab, а не старый notePrefab
        GameObject spawnedNote = Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);
        
        NoteObject noteObj = spawnedNote.GetComponent<NoteObject>();
        if (noteObj != null)
        {
            // Выбираем рандомное направление (Any, Top, Bottom, Left, Right)
            int randomDirIndex = Random.Range(0, 5); 
            noteObj.Initialize((NoteObject.HitDirection)randomDirIndex);
        }
    }
}