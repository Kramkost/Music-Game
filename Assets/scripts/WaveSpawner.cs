using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Настройки аудио")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _threshold = 0.5f; 
    [SerializeField] private float _cooldown = 0.15f; 

    [Header("Настройки спавна")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private Transform[] _spawnPoints;

    private float[] _spectrum = new float[512]; 
    private float _timer;
    private bool _isPlaying = false;
    private bool _isFinished = false; 

    private void OnEnable()
    {
        GameEventManager.OnGameStart += StartSpawning;
    }

    private void OnDisable()
    {
        GameEventManager.OnGameStart -= StartSpawning;
    }

    private void StartSpawning()
    {
        _isPlaying = true;
        _isFinished = false;
        if (!_audioSource.isPlaying) _audioSource.Play();
    }

    private void Update()
    {
        if (!_isPlaying || _isFinished) return;

        // ==========================================
        // НОВОЕ: ПРОВЕРКА НА КОНЕЦ ТРЕКА
        // ==========================================
        // 
        if (_audioSource.time >= _audioSource.clip.length - 0.1f)
        {
            _isFinished = true;
            _isPlaying = false;
            
            Debug.Log("Трек завершен! ПОБЕДА!");
            
            
            GameEventManager.OnLevelComplete?.Invoke(3); 
            return;
        }

        
        if (!_audioSource.isPlaying) return;

        
        _timer += Time.deltaTime;
        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

        float lowFreqIntensity = 0;
        for (int i = 0; i < 7; i++)
        {
            lowFreqIntensity += _spectrum[i];
        }
        
        if (lowFreqIntensity > _threshold && _timer >= _cooldown)
        {
            SpawnBlock();
            _timer = 0; 
        }
    }

    private void SpawnBlock()
    {
        if (_spawnPoints.Length == 0 || _enemyPrefabs.Length == 0) return;
        
        Transform targetPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];

        Instantiate(prefab, targetPoint.position, targetPoint.rotation);
    }
}