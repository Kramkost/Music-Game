using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening; 

public class MenuController : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animDuration = 0.2f;

    [Header("Настройки аудио")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [System.Serializable]
    public class MenuItem
    {
        public string labelName;       
        public Button button;          
        public MenuActionType actionType; 

        [Header("Параметры")]
        public GameObject objectToToggle; 
        public string sceneName;          
        public UnityEvent customEvent;    

        [HideInInspector] public Vector3 originalScale; 
    }

    public enum MenuActionType
    {
        ToggleObject,
        LoadScene,
        CustomEvent
    }

    public List<MenuItem> menuItems = new List<MenuItem>();

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        foreach (var item in menuItems)
        {
            if (item.button == null) continue;

            item.originalScale = item.button.transform.localScale;
            item.button.onClick.AddListener(() => HandleClick(item));
            
            AddHoverEvents(item); 
        }
    }

    void HandleClick(MenuItem item)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        
        item.button.transform.DOScale(item.originalScale * clickScale, 0.1f).SetUpdate(true).OnComplete(() =>
        {
            item.button.transform.DOScale(item.originalScale * hoverScale, 0.1f).SetUpdate(true);
            ExecuteAction(item);
        });
    }

    void ExecuteAction(MenuItem item)
    {
        switch (item.actionType)
        {
            case MenuActionType.ToggleObject:
                if (item.objectToToggle != null)
                {
                    bool isActive = item.objectToToggle.activeSelf;
                    item.objectToToggle.SetActive(!isActive);
                }
                break;

            case MenuActionType.LoadScene:
                if (!string.IsNullOrEmpty(item.sceneName))
                {
                    SceneManager.LoadScene(item.sceneName);
                }
                break;

            case MenuActionType.CustomEvent:
                break;
        }

        item.customEvent?.Invoke();
    }

    void AddHoverEvents(MenuItem item)
    {
        Button btn = item.button;
        
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
  
        entryEnter.callback.AddListener((data) => { btn.transform.DOScale(item.originalScale * hoverScale, animDuration).SetUpdate(true); });
        trigger.triggers.Add(entryEnter);

        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        
        entryExit.callback.AddListener((data) => { btn.transform.DOScale(item.originalScale, animDuration).SetUpdate(true); });
        trigger.triggers.Add(entryExit);
    }
}