/* =============================================================================
СКРИПТ: JuiceManager.cs
НАЗНАЧЕНИЕ: Отвечает за всю "сочность" игры (Juice). Тряска камеры, партиклы, 
пульсация ядра и визуальная отдача при ударах или промахах ну сочно.

НОВОВВЕДЕНИЯ И ЗВУКИ: 
Добавлены массивы для глобальных звуков:
1. beatSounds - Легкий звук на каждый бит (работает как метроном, чтобы игрок ловил ритм).
2. uiClickSounds - Звуки для менюшек. Есть специальный публичный метод PlayUIClick(), 
   который можно повесить на любую кнопку в Canvas (например, на кнопку Play).
=============================================================================
*/

using UnityEngine;
using DG.Tweening;

public class JuiceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform centerCore;
    [SerializeField] private Transform shield;
    [SerializeField] private ParticleSystem hitParticles;
    
    [Header("Juice Settings")]
    [SerializeField] private float punchScale = 0.2f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.3f;

    // ==========================================
    // ГЛОБАЛЬНЫЕ ЗВУКИ
    // ==========================================
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] beatSounds;    // Звуки "тиканья" в бит
    [SerializeField] private AudioClip[] uiClickSounds; // Звуки кликов по UI
    // ==========================================

    private void OnEnable()
    {
        GameEventManager.OnNoteResolved += HandleImpact;
        GameEventManager.OnBeatPulse += HandleBeat;
    }

    private void OnDisable()
    {
        GameEventManager.OnNoteResolved -= HandleImpact;
        GameEventManager.OnBeatPulse -= HandleBeat;
    }

    private void HandleImpact(bool success)
    {
        if (success)
        {
            shield.DORewind();
            shield.DOPunchScale(Vector3.one * punchScale, 0.2f, 5, 1f);
            Camera.main.transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90f);
            
            if (hitParticles != null) hitParticles.Play();
        }
        else
        {
            centerCore.DORewind();
            centerCore.DOShakeScale(0.3f, 0.5f, 10, 90f);
            Camera.main.transform.DOShakePosition(0.3f, shakeStrength * 2f, 10, 90f); 
        }
    }

    private void HandleBeat()
    {
        // 1. Легкая пульсация мира в такт музыке
        centerCore.DOPunchScale(Vector3.one * 0.05f, 0.15f, 1, 0f);

        // 2. Играем звук метронома/бита
        PlayRandomSound(beatSounds);
    }

    // ==========================================
    // ПУБЛИЧНЫЙ МЕТОД ДЛЯ КНОПОК UI
    // ==========================================
    public void PlayUIClick()
    {
        PlayRandomSound(uiClickSounds);
    }

    // Вспомогательный метод для проигрывания звука
    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            if (Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(randomClip, Camera.main.transform.position);
            }
        }
    }
}