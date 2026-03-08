using UnityEngine;

/* =============================================================================
СКРИПТ: RhythmSequencer.cs
НАЗНАЧЕНИЕ: Сердце нашей ритм-игры. Главный спавнер, который работает СТРОГО под бит музыки.
ЧТО ДЕЛАЕТ: 
1. Читает аудиодорожку и математически вычисляет идеальные тайминги для битов (идеально для WebGL).
2. В нужный момент (в такт) выбирает случайную точку спавна.
3. Отслеживает конец трека и вызывает Экран Победы!
=============================================================================
*/

[RequireComponent(typeof(AudioSource))]
public class RhythmSequencer : MonoBehaviour
{
    [Header("Настройки ритма")]
    [SerializeField] private float bpm = 120f;
    
    [Header("Настройки спавна")]
    [SerializeField] private GameObject[] notePrefabs; 
    [SerializeField] private Transform[] spawnPoints;
    
    private AudioSource audioSource;
    private float beatInterval;
    private int lastBeatIndex = -1;
    
    // Флаги состояний
    private bool isPlaying = false; 
    private bool isFinished = false; // Флаг конца игры

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Вычисляем, сколько секунд длится один удар (beat)
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
        isFinished = false;
        
        // На случай, если трек был сброшен отмоткой рекламы
        if (!audioSource.isPlaying) 
        {
            audioSource.Play(); 
        }
    }

    private void Update()
    {
        // Если игра не идет или уровень пройден — ничего не считаем
        if (!isPlaying || isFinished) return;

        // ==========================================
        // ЛОГИКА ПОБЕДЫ (КОНЕЦ ТРЕКА)
        // ==========================================
        // Если до конца трека осталось меньше 0.1 секунды — уровень пройден!
        if (audioSource.clip != null && audioSource.time >= audioSource.clip.length - 0.1f)
        {
            isFinished = true;
            isPlaying = false;
            
            Debug.Log("Трек завершен! ПОБЕДА!");
            
            // Вызываем событие завершения уровня. Передаем 0, так как GameManager 
            // сам умно посчитает звезды на основе твоего комбо.
            GameEventManager.OnLevelComplete?.Invoke(0); 
            return;
        }

        // Если музыка на паузе (например, из-за рекламы), ждем
        if (!audioSource.isPlaying) return;

        // ==========================================
        // МАТЕМАТИКА СПАВНА ПО BPM
        // ==========================================
        // Используем timeSamples, так как это самый точный способ узнать время в аудио
        float trackTime = (float)audioSource.timeSamples / audioSource.clip.frequency;
        
        // Вычисляем, какой сейчас по счету бит
        int currentBeatIndex = Mathf.FloorToInt(trackTime / beatInterval);

        // Если начался новый бит — стреляем блоком!
        if (currentBeatIndex > lastBeatIndex)
        {
            lastBeatIndex = currentBeatIndex;
            GameEventManager.OnBeatPulse?.Invoke();
            SpawnNote();
        }
    }

    private void SpawnNote()
    {
        // Защита от пустых массивов
        if (spawnPoints == null || spawnPoints.Length == 0 || notePrefabs == null || notePrefabs.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject randomPrefab = notePrefabs[Random.Range(0, notePrefabs.Length)];
        
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