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
        
        if (Input.GetMouseButton(0))
        {
            UpdateTargetAngleFromMouse();
        }

        
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

       
        float radians = currentAngle * Mathf.Deg2Rad;
        
        float x = centerCore.position.x + Mathf.Cos(radians) * radius;
        float y = centerCore.position.y + Mathf.Sin(radians) * radius;

        transform.position = new Vector3(x, y, 0);

        
        transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90f);
    }

    private void UpdateTargetAngleFromMouse()
    {
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; 

        
        Vector3 direction = mousePos - centerCore.position;

        
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}