/* =============================================================================
СКРИПТ: NoteObject.cs
НАЗНАЧЕНИЕ: Управляет поведением отдельной "ноты" (блока/препятствия) на сцене.
Она летит к ядру, проверяет столкновения со щитом игрока и определяет успех/провал.

НОВОВВЕДЕНИЯ И ЗВУКИ: 
Теперь блоки умеют "звучать"! Добавлены массивы для рандомных звуков:
1. spawnSounds - Звук при появлении (взводе) блока.
2. successSounds - Звук при успешном отбивании/удержании.
3. failSounds - Звук при ошибке/пропуске/смерти.

Скрипт сам выберет случайный звук из массива. Мы используем PlayClipAtPoint, 
чтобы звук успел доиграть до конца, даже если сам блок уже уничтожен!
=============================================================================
*/

using UnityEngine;
using DG.Tweening;

public class NoteObject : MonoBehaviour
{
    
    public enum NoteType { Def, Kill, Long }
    public enum HitDirection { Any, Top, Bottom, Left, Right }


    [Header("Note Type Setup")]
    [SerializeField] private NoteType initialType = NoteType.Def;

    [Header("Note Settings")]
    [SerializeField] private float approachDuration = 2f;
    [SerializeField] private Ease moveEase = Ease.Linear;
    [SerializeField] private float requiredHoldTime = 0.5f; // Сколько секунд нужно держать Long-блок

    [Header("References")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private LayerMask coreLayer;
    [SerializeField] private Transform arrowVisual; 
    [SerializeField] private SpriteRenderer noteRenderer; // НОВОЕ: Ссылка на спрайт (чтобы менять цвет)

    // ==========================================
    // НОВЫЕ НАСТРОЙКИ ЗВУКОВ
    // ==========================================
    [Header("Audio Setup")]
    [SerializeField] private AudioClip[] spawnSounds;   // Звуки при появлении
    [SerializeField] private AudioClip[] successSounds; // Звуки при успехе
    [SerializeField] private AudioClip[] failSounds;    // Звуки при ошибке
    // ==========================================

    private HitDirection requiredHitDir = HitDirection.Any;
    private NoteType currentType = NoteType.Def; // По умолчанию блок обычный
    private float currentHoldTime = 0f; // Таймер для Long-блока

    private void Start()
    {
        //Change type of the note from inspector
        switch (initialType)
        {
            case NoteType.Def: changeToDef(); break;
            case NoteType.Kill: changeToKill(); break;
            case NoteType.Long: changeToLong(); break;
        }

        // ИГРАЕМ РАНДОМНЫЙ ЗВУК ПОЯВЛЕНИЯ
        PlayRandomSound(spawnSounds);

        // Fly into player
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
        if (noteRenderer != null) noteRenderer.color = Color.red; // Красим в красный
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false); // Убираем стрелку
    }

    public void changeToDef()
    {
        currentType = NoteType.Def;
        if (noteRenderer != null) noteRenderer.color = Color.white; // Обычный цвет
        SetupArrowVisual(); // Возвращаем стрелку, если нужно
    }

    public void changeToLong()
    {
        currentType = NoteType.Long;
        if (noteRenderer != null) noteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // Делаем полупрозрачным (Alpha = 0.5)
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false); // Убираем стрелку
        currentHoldTime = 0f; // Сбрасываем таймер на всякий случай
    }

    // ==========================================

    private void SetupArrowVisual()
    {
        if (arrowVisual == null) return;

        // Если блок не дефолтный или направление Any — прячем стрелку
        if (requiredHitDir == HitDirection.Any || currentType != NoteType.Def)
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

    // СРАБАТЫВАЕТ ПРИ ПЕРВОМ КАСАНИИ
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Касание со ЩИТОМ
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            if (currentType == NoteType.Def)
            {
                // Обычный блок: проверяем куда игрок махнул мышкой
                CheckShieldHitDirection(collision.transform);
            }
            else if (currentType == NoteType.Kill)
            {
                // Убийственный блок: трогать нельзя!
                Debug.Log("СМЕРТЬ! Ты задел красный блок :(");
                FailNote();
            }
            // Если тип Long - ничего не делаем при первом касании, ждем удержания (ниже)
        }
        // Касание с ЯДРОМ (игрок пропустил)
        else if (((1 << collision.gameObject.layer) & coreLayer) != 0)
        {
            FailNote();
        }
    }

    // СРАБАТЫВАЕТ ПОКА ЩИТ НАХОДИТСЯ ВНУТРИ БЛОКА (ДЛЯ LONG-ТИПА)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentType == NoteType.Long && (((1 << collision.gameObject.layer) & shieldLayer) != 0))
        {
            currentHoldTime += Time.deltaTime; // Копим время удержания
            
            if (currentHoldTime >= requiredHoldTime)
            {
                Debug.Log("Успех! Ты удержал прозрачный блок!");
                SuccessNote();
            }
        }
    }

    // СРАБАТЫВАЕТ ЕСЛИ ИГРОК УБРАЛ ЩИТ С БЛОКА ДОСРОЧНО
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentType == NoteType.Long && (((1 << collision.gameObject.layer) & shieldLayer) != 0))
        {
            currentHoldTime = 0f; // Сбрасываем таймер удержания, если игрок убрал щит
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
        if (currentType == NoteType.Def) Debug.Log("Ты Ударил в правильном направлении");
        
        PlayRandomSound(successSounds); // ИГРАЕМ ЗВУК УСПЕХА
        
        GameEventManager.OnNoteResolved?.Invoke(true);
        DestroyNote();
    }

    private void FailNote()
    {
        if (currentType == NoteType.Def) Debug.Log("Ты НЕ ударил в правильном направлении");
        
        PlayRandomSound(failSounds); // ИГРАЕМ ЗВУК ПРОВАЛА
        
        GameEventManager.OnNoteResolved?.Invoke(false);
        DestroyNote();
    }

    private void DestroyNote()
    {
        transform.DOKill(); 
        Destroy(gameObject);
    }

    // ==========================================
    // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ЗВУКОВ
    // ==========================================
    private void PlayRandomSound(AudioClip[] clips)
    {
        // Проверяем, есть ли вообще звуки в массиве
        if (clips != null && clips.Length > 0)
        {
            // Выбираем случайный
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            
            // Играем его на позиции камеры (чтобы всегда было хорошо слышно)
            if (Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(randomClip, Camera.main.transform.position);
            }
        }
    }
}