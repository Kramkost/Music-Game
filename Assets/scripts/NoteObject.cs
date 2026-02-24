using UnityEngine;
using DG.Tweening;

public class NoteObject : MonoBehaviour
{
    [SerializeField] private float approachDuration = 2f;
    [SerializeField] private Ease moveEase = Ease.Linear;
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private LayerMask coreLayer;

    private void Start()
    {
        // Летим в центр (0,0,0). Ease.Linear обязателен для ритм-игр, чтобы игрок мог читать скорость!
        transform.DOMove(Vector3.zero, approachDuration)
                 .SetEase(moveEase)
                 .OnComplete(FailNote); // Если твин закончился и мы не столкнулись
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            // Удар о щит - Успех
            GameEventManager.OnNoteResolved?.Invoke(true);
            DestroyNote();
        }
        else if (((1 << collision.gameObject.layer) & coreLayer) != 0)
        {
            // Удар о ядро - Провал
            FailNote();
        }
    }

    private void FailNote()
    {
        GameEventManager.OnNoteResolved?.Invoke(false);
        DestroyNote();
    }

    private void DestroyNote()
    {
        transform.DOKill(); // Обязательно убиваем твин перед уничтожением
        Destroy(gameObject);
    }
}