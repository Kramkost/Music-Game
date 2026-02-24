using UnityEngine;

public class PlayerDefender : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform centerCore; 
    [SerializeField] private float radius = 2f;    
    [SerializeField] private float rotationSpeed = 25f;

    private float targetAngle = 90f; 
    private float currentAngle = 90f;

    private void Update()
    {
        // Если зажата левая кнопка мыши — обновляем целевой угол
        if (Input.GetMouseButton(0))
        {
            UpdateTargetAngleFromMouse();
        }

        // Плавно вращаем к цели. 
        // ВАЖНО: Используем LerpAngle вместо Lerp, чтобы щит не делал полный оборот 
        // в обратную сторону при переходе через отметку в 360/0 градусов!
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // --- МАГИЯ ОРБИТЫ ---
        float radians = currentAngle * Mathf.Deg2Rad;
        
        float x = centerCore.position.x + Mathf.Cos(radians) * radius;
        float y = centerCore.position.y + Mathf.Sin(radians) * radius;

        transform.position = new Vector3(x, y, 0);

        // Поворачиваем щит, чтобы он смотрел "наружу" от центра
        transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90f);
    }

    private void UpdateTargetAngleFromMouse()
    {
        // 1. Получаем позицию мыши в мировых координатах (на сцене)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Обнуляем Z, так как у нас 2D

        // 2. Находим вектор направления от ядра к мыши
        Vector3 direction = mousePos - centerCore.position;

        // 3. Вычисляем угол в градусах с помощью Atan2
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}