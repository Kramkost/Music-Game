using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SpriteEvolver : MonoBehaviour
{
    [Header("Tag Setup")]
    [Tooltip("Тег существа, при касании которого будет меняться спрайт")]
    [SerializeField] private string targetTag = "Enemy"; 

    [Header("Evolution Sprites")]
    [Tooltip("Закинь сюда скрытые спрайты по порядку (0 - первая смена, 1 - вторая и т.д.)")]
    [SerializeField] private Sprite[] hiddenSprites;

    [Header("Juice Setup")]
    [SerializeField] private float punchScaleForce = 0.3f;
    [SerializeField] private float punchDuration = 0.3f;   

    [Header("Death Sequence Setup")]
    [Tooltip("Ссылка на панель смерти (UI Canvas)")]
    [SerializeField] private GameObject deathPanel;
    [Tooltip("За сколько секунд остановится время и звук")]
    [SerializeField] private float timeSlowdownDuration = 1.5f;

    private SpriteRenderer _spriteRenderer;
    private int _currentSpriteIndex = -1;
    private bool _isDead = false;

 
    private Dictionary<AudioSource, float> _originalAudioPitches = new Dictionary<AudioSource, float>();

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

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
        }

        if (_currentSpriteIndex == hiddenSprites.Length - 1 && !_isDead)
        {
            TriggerDeathSequence();
        }
        else if (_currentSpriteIndex >= hiddenSprites.Length)
        {
            _currentSpriteIndex = hiddenSprites.Length - 1;
        }
    }

    private void TriggerDeathSequence()
    {
        _isDead = true;
        
        if (deathPanel != null) deathPanel.SetActive(true);


        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, timeSlowdownDuration)
            .SetUpdate(true);

        DOTween.To(() => AudioListener.volume, v => AudioListener.volume = v, 0f, timeSlowdownDuration)
            .SetUpdate(true);

        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        foreach (var source in allAudioSources)
        {
            if (source.isPlaying)
            {

                _originalAudioPitches[source] = source.pitch;
                

                source.DOPitch(0f, timeSlowdownDuration).SetUpdate(true);
            }
        }
    }

    public void ResetSprite(Sprite baseSprite)
    {
        _currentSpriteIndex = -1;
        _isDead = false;
        
        Time.timeScale = 1f; 
        

        AudioListener.volume = 1f;


        foreach (var kvp in _originalAudioPitches)
        {
            if (kvp.Key != null) 
            {
                kvp.Key.DOKill(); 
                kvp.Key.pitch = kvp.Value;
            }
        }
        _originalAudioPitches.Clear();
        
        if (deathPanel != null) deathPanel.SetActive(false);

        _spriteRenderer.sprite = baseSprite;
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * punchScaleForce, punchDuration, 5, 1f);
    }
}