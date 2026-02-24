using UnityEngine;
using DG.Tweening;

public class JuiceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform centerCore;
    [SerializeField] private Transform shield;
    [SerializeField] private ParticleSystem hitParticles;
    
    [Header("Settings")]
    [SerializeField] private float punchScale = 0.2f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.3f;

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
            // Сочный удар: щит пульсирует, камера немного трясется
            shield.DORewind();
            shield.DOPunchScale(Vector3.one * punchScale, 0.2f, 5, 1f);
            
            // ИСПРАВЛЕНО ЗДЕСЬ: обращаемся к transform и убираем лишние bool
            Camera.main.transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90f);
            
            if (hitParticles != null) hitParticles.Play();
        }
        else
        {
            // Ошибка: Ядро сжимается и краснеет
            centerCore.DORewind();
            centerCore.DOShakeScale(0.3f, 0.5f, 10, 90f);
            
            // ИСПРАВЛЕНО ЗДЕСЬ: также добавлено обращение к transform
            Camera.main.transform.DOShakePosition(0.3f, shakeStrength * 2f, 10, 90f); 
        }
    }

    private void HandleBeat()
    {
        // Легкая пульсация мира в такт музыке
        centerCore.DOPunchScale(Vector3.one * 0.05f, 0.15f, 1, 0f);
    }
}