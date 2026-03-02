using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Настройки аудио")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _threshold = 0.5f; // Порог чувствительности (настраивай в инспекторе)
    [SerializeField] private float _cooldown = 0.15f; // Минимальная пауза между блоками

    [Header("Настройки спавна")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private Transform[] _spawnPoints;

    private float[] _spectrum = new float[512]; // Массив для данных спектра
    private float _timer;

    private void Update()
    {
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