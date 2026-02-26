/* =============================================================================
СКРИПТ: PlayerDefender.cs
НАЗНАЧЕНИЕ: Управляет орбитальным щитом игрока. Щит плавно летает за курсором 
мыши по кругу, а также вычисляет силу и направление "взмаха" для отбивания нот.

НОВОВВЕДЕНИЯ И ЗВУКИ: 
Добавлен массив swingSounds. Теперь при нажатии на кнопку мыши (когда игрок 
начинает движение/взмах), щит издает случайный звук (например, "Вжух!" или лязг).
=============================================================================
*/

using UnityEngine;

public class PlayerDefender : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform centerCore; 
    [SerializeField] private float radius = 2f;    
    [SerializeField] private float rotationSpeed = 25f;

    // ==========================================
    // ЗВУКИ ЩИТА
    // ==========================================
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] swingSounds; // Звуки взмаха/активации щита
    // ==========================================

    public Vector2 MouseVelocity { get; private set; }
    private Vector2 lastMouseWorldPos;

    private float targetAngle = 90f; 
    private float currentAngle = 90f;

    private void Start()
    {
        lastMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        Vector2 currentMouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Time.deltaTime > 0)
        {
            MouseVelocity = (currentMouseWorldPos - lastMouseWorldPos) / Time.deltaTime;
        }
        lastMouseWorldPos = currentMouseWorldPos;

        // ОТСЛЕЖИВАЕМ КЛИК ДЛЯ ЗВУКА
        if (Input.GetMouseButtonDown(0))
        {
            PlayRandomSound(swingSounds);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 direction = (Vector3)currentMouseWorldPos - centerCore.position;
            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        float radians = currentAngle * Mathf.Deg2Rad;
        float x = centerCore.position.x + Mathf.Cos(radians) * radius;
        float y = centerCore.position.y + Mathf.Sin(radians) * radius;

        transform.position = new Vector3(x, y, 0);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90f);
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