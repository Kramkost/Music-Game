/* =============================================================================
СКРИПТ: NoteObject.cs
НАЗНАЧЕНИЕ: Управляет поведением отдельной "ноты" (блока/препятствия) на сцене.
Она летит к ядру, проверяет столкновения со щитом игрока и определяет успех/провал.

*/

using UnityEngine;
using DG.Tweening;

public class NoteObject : MonoBehaviour
{

    public enum NoteType { Def, Kill, Long, Swipe }
    public enum HitDirection { Any, Top, Bottom, Left, Right }


    [Header("Note Type Setup")]
    [SerializeField] private NoteType initialType = NoteType.Def;

    [Header("Note Settings")]
    [SerializeField] private float approachDuration = 2f;
    [SerializeField] private Ease moveEase = Ease.Linear;
    [SerializeField] private float requiredHoldTime = 0.5f; 
    
    //Дистанция для Слайдера (насколько далеко нужно "провести" мечом)
    [SerializeField] private float requiredSwipeDistance = 1.0f; 

    [Header("References")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private LayerMask coreLayer;
    [SerializeField] private Transform arrowVisual; 
    [SerializeField] private SpriteRenderer noteRenderer; 

    [Header("Audio Setup")]
    [SerializeField] private AudioClip[] spawnSounds;   
    [SerializeField] private AudioClip[] successSounds; 
    [SerializeField] private AudioClip[] failSounds;    

    private HitDirection requiredHitDir = HitDirection.Any;
    private NoteType currentType = NoteType.Def; 
    private float currentHoldTime = 0f; 


    private Vector2 swipeEntryPoint; 
    private bool isSwiping = false;

    private void Start()
    {
        switch (initialType)
        {
            case NoteType.Def: changeToDef(); break;
            case NoteType.Kill: changeToKill(); break;
            case NoteType.Long: changeToLong(); break;
            case NoteType.Swipe: changeToSwipe(); break;
        }

        PlayRandomSound(spawnSounds);

        transform.DOMove(Vector3.zero, approachDuration)
                 .SetEase(moveEase)
                 .OnComplete(FailNote); 
    }

    public void Initialize(HitDirection dir)
    {
        requiredHitDir = dir;
        SetupArrowVisual();
    }

    // ==========================================
    // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ СМЕНЫ ТИПА БЛОКА
    // ==========================================
    
    public void changeToKill()
    {
        currentType = NoteType.Kill;
        if (noteRenderer != null) noteRenderer.color = Color.red; 
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false); 
    }

    public void changeToDef()
    {
        currentType = NoteType.Def;
        if (noteRenderer != null) noteRenderer.color = Color.white; 
        SetupArrowVisual(); 
    }

    public void changeToLong()
    {
        currentType = NoteType.Long;
        if (noteRenderer != null) noteRenderer.color = new Color(1f, 1f, 1f, 0.5f); 
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false); 
        currentHoldTime = 0f; 
    }

    // НОВОЕ: Метод для Слайдера
    public void changeToSwipe()
    {
        currentType = NoteType.Swipe;
        if (noteRenderer != null) noteRenderer.color = Color.cyan; // Сделаем его голубым/бирюзовым для отличия
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false); // Убираем стрелку (тут важно просто провести)
        isSwiping = false;
    }

    // ==========================================

    private void SetupArrowVisual()
    {
        if (arrowVisual == null) return;

        if (requiredHitDir == HitDirection.Any || currentType != NoteType.Def)
        {
            arrowVisual.gameObject.SetActive(false); 
            return;
        }

        arrowVisual.gameObject.SetActive(true);

        switch (requiredHitDir)
        {
            case HitDirection.Right: arrowVisual.localRotation = Quaternion.Euler(0, 0, 0f); break;
            case HitDirection.Top: arrowVisual.localRotation = Quaternion.Euler(0, 0, 90f); break;
            case HitDirection.Left: arrowVisual.localRotation = Quaternion.Euler(0, 0, 180f); break;
            case HitDirection.Bottom: arrowVisual.localRotation = Quaternion.Euler(0, 0, -90f); break;
        }

        arrowVisual.localScale = Vector3.zero;
        arrowVisual.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    // СРАБАТЫВАЕТ ПРИ ПЕРВОМ КАСАНИИ
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            if (currentType == NoteType.Def)
            {
                CheckShieldHitDirection(collision.transform);
            }
            else if (currentType == NoteType.Kill)
            {
                Debug.Log("СМЕРТЬ! Ты задел красный блок :(");
                FailNote();
            }
            // НОВОЕ: Если это Слайдер, запоминаем точку, где игрок коснулся блока
            else if (currentType == NoteType.Swipe)
            {
                swipeEntryPoint = collision.transform.position;
                isSwiping = true;
            }
        }
        else if (((1 << collision.gameObject.layer) & coreLayer) != 0)
        {
            FailNote();
        }
    }

    // СРАБАТЫВАЕТ ПОКА ЩИТ НАХОДИТСЯ ВНУТРИ БЛОКА
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            // Логика для Long-блока (удержание)
            if (currentType == NoteType.Long)
            {
                currentHoldTime += Time.deltaTime; 
                if (currentHoldTime >= requiredHoldTime)
                {
                    Debug.Log("Успех! Ты удержал прозрачный блок!");
                    SuccessNote();
                }
            }
            // НОВОЕ: Логика для Swipe-блока (проведение)
            else if (currentType == NoteType.Swipe && isSwiping)
            {
                // Вычисляем дистанцию: как далеко игрок утянул щит от точки входа?
                float distanceDragged = Vector2.Distance(swipeEntryPoint, collision.transform.position);
                
                // Если протащил достаточно далеко — рубим блок!
                if (distanceDragged >= requiredSwipeDistance)
                {
                    Debug.Log("Успех! Ты разрезал слайдер!");
                    SuccessNote();
                }
            }
        }
    }

    // СРАБАТЫВАЕТ ЕСЛИ ИГРОК УБРАЛ ЩИТ С БЛОКА ДОСРОЧНО
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            if (currentType == NoteType.Long)
            {
                currentHoldTime = 0f; 
            }
            // Сбрасываем свайп, если игрок соскользнул с блока
            else if (currentType == NoteType.Swipe)
            {
                isSwiping = false; 
            }
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

            if (requiredHitDir == HitDirection.Top && velocity.y > threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Bottom && velocity.y < -threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Right && velocity.x > threshold) isHitCorrect = true;
            else if (requiredHitDir == HitDirection.Left && velocity.x < -threshold) isHitCorrect = true;

            if (isHitCorrect) SuccessNote();
            else 
            {
                Debug.Log($"Мимо! Нужное направление: {requiredHitDir}, а мышка двигалась: {velocity}");
                FailNote(); 
            }
        }
        else FailNote();
    }

    private void SuccessNote()
    {
        if (currentType == NoteType.Def) Debug.Log("Ты Ударил в правильном направлении");
        
        PlayRandomSound(successSounds); 
        GameEventManager.OnNoteResolved?.Invoke(true);
        DestroyNote();
    }

    private void FailNote()
    {
        if (currentType == NoteType.Def) Debug.Log("Ты НЕ ударил в правильном направлении");
        
        PlayRandomSound(failSounds); 
        GameEventManager.OnNoteResolved?.Invoke(false);
        DestroyNote();
    }

    private void DestroyNote()
    {
        transform.DOKill(); 
        Destroy(gameObject);
    }

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