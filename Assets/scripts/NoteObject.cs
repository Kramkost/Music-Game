using UnityEngine;
using DG.Tweening;

public class NoteObject : MonoBehaviour
{

    public enum HitDirection { Any, Top, Bottom, Left, Right }

    [Header("Note Settings")]
    [SerializeField] private float approachDuration = 2f;
    [SerializeField] private Ease moveEase = Ease.Linear;

    [Header("References")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private LayerMask coreLayer;
    [SerializeField] private Transform arrowVisual; 

    private HitDirection requiredHitDir = HitDirection.Any;

    private void Start()
    {
       
        transform.DOMove(Vector3.zero, approachDuration)
                 .SetEase(moveEase)
                 .OnComplete(FailNote); 
    }

    
    public void Initialize(HitDirection dir)
    {
        requiredHitDir = dir;
        SetupArrowVisual();
    }

    private void SetupArrowVisual()
    {
        if (arrowVisual == null) return;

        if (requiredHitDir == HitDirection.Any)
        {
            arrowVisual.gameObject.SetActive(false); 
            return;
        }

        arrowVisual.gameObject.SetActive(true);

        
        switch (requiredHitDir)
        {
            case HitDirection.Right:
                arrowVisual.localRotation = Quaternion.Euler(0, 0, 0f);
                break;
            case HitDirection.Top:
                arrowVisual.localRotation = Quaternion.Euler(0, 0, 90f);
                break;
            case HitDirection.Left:
                arrowVisual.localRotation = Quaternion.Euler(0, 0, 180f);
                break;
            case HitDirection.Bottom:
                arrowVisual.localRotation = Quaternion.Euler(0, 0, -90f);
                break;
        }

       
        arrowVisual.localScale = Vector3.zero;
        arrowVisual.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            CheckShieldHitDirection(collision.transform);
        }
        else if (((1 << collision.gameObject.layer) & coreLayer) != 0)
        {
            FailNote();
        }
    }

private void CheckShieldHitDirection(Transform shieldTransform)
    {
        if (requiredHitDir == HitDirection.Any)
        {
            SuccessNote();
            return;
        }

        
        PlayerDefender defender = shieldTransform.GetComponent<PlayerDefender>();
        if (defender == null) defender = shieldTransform.GetComponentInParent<PlayerDefender>();

        if (defender != null)
        {
            
            Vector2 velocity = defender.MouseVelocity;
            bool isHitCorrect = false;

           
            float threshold = 2f; 

            // Проверяем: совпадает ли ВЗМАХ МЫШЬЮ со СТРЕЛКОЙ
            if (requiredHitDir == HitDirection.Top && velocity.y > threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Bottom && velocity.y < -threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Right && velocity.x > threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Left && velocity.x < -threshold) isHitCorrect = true;

            if (isHitCorrect) 
            {
                SuccessNote();
            }
            else 
            {
                Debug.Log($"Мимо! Нужное направление: {requiredHitDir}, а мышка двигалась: {velocity}");
                FailNote(); 
            }
        }
        else
        {
            FailNote();
        }
    }

    private void SuccessNote()
    {
        Debug.Log("Ты Ударил в правильном направлении");
        GameEventManager.OnNoteResolved?.Invoke(true);
        DestroyNote();
    }

    private void FailNote()
    {

        GameEventManager.OnNoteResolved?.Invoke(false);
        DestroyNote();
    }

    private void DestroyNote()
    {
        transform.DOKill(); 
        Destroy(gameObject);
    }
}