using UnityEngine;
using DG.Tweening;
public class SpriteEvolver : MonoBehaviour
{
    [Header("Tag Setup")]
    [Tooltip("Тег существа, при касании которого будет меняться спрайт")]
    [SerializeField] private string targetTag = "Enemy"; 

    [Header("Evolution Sprites")]
    [Tooltip("Закинь сюда скрытые спрайты по порядку (0 - первая смена, 1 - вторая и т.д.)")]
    [SerializeField] private Sprite[] hiddenSprites;

    [Header("Juice Setup")]
    [SerializeField] private float punchScaleForce = 0.3f; // Сила вздутия при смене
    [SerializeField] private float punchDuration = 0.3f;   // Длительность вздутия живота

    private SpriteRenderer _spriteRenderer;
    private int _currentSpriteIndex = -1;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag(targetTag))
        {
            ChangeToNextSprite();
        }
    }

    private void ChangeToNextSprite()
    {
        
        _currentSpriteIndex++;

       
        if (_currentSpriteIndex < hiddenSprites.Length)
        {
            
            _spriteRenderer.sprite = hiddenSprites[_currentSpriteIndex];
            
            
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * punchScaleForce, punchDuration, 5, 1f);

            Debug.Log($"Спрайт изменен!: {_currentSpriteIndex}");
        }
        else
        {
            
            _currentSpriteIndex = hiddenSprites.Length - 1;
            Debug.Log("Спрайтов больше нет.");
        }
    }

    public void ResetSprite(Sprite baseSprite)
    {
        _currentSpriteIndex = -1;
        _spriteRenderer.sprite = baseSprite;
        transform.DOPunchScale(Vector3.one * punchScaleForce, punchDuration, 5, 1f);
    }
}
