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
        

        GameObject spawnedNote = Instantiate(notePrefab, spawnPoint.position, spawnPoint.rotation);
        

        NoteObject noteObj = spawnedNote.GetComponent<NoteObject>();
        if (noteObj != null)
        {

            int randomDirIndex = Random.Range(0, 5); 
            
 
            noteObj.Initialize((NoteObject.HitDirection)randomDirIndex);
        }
    }
}