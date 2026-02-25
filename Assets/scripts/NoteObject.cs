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
        
        transform.DOMove(Vector3.zero, approachDuration)
                 .SetEase(moveEase)
                 .OnComplete(FailNote); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & shieldLayer) != 0)
        {
            
            GameEventManager.OnNoteResolved?.Invoke(true);
            DestroyNote();
        }
        else if (((1 << collision.gameObject.layer) & coreLayer) != 0)
        {
            
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
        transform.DOKill(); 
        Destroy(gameObject);
    }
}