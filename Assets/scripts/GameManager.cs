using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, LevelComplete }
    
    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalStarsEarned = 0;
    
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup mainMenuPanel;

    private int combo = 0;

    private void OnEnable()
    {
        GameEventManager.OnNoteResolved += HandleNoteResolved;
        GameEventManager.OnGameStart += () => currentState = GameState.Playing;
    }

    private void OnDisable()
    {
        GameEventManager.OnNoteResolved -= HandleNoteResolved;
        GameEventManager.OnGameStart -= () => currentState = GameState.Playing;
    }


    public void UI_StartGameButton()
    {
    
        mainMenuPanel.interactable = false; 

       
        mainMenuPanel.DOFade(0f, 0.5f).OnComplete(() =>
        {
            mainMenuPanel.gameObject.SetActive(false); // Выключаем объект полностью
            GameEventManager.OnGameStart?.Invoke();    // Запускаем геймплей и музыку!
        });
    }

    private void HandleNoteResolved(bool success)
    {
        if (currentState != GameState.Playing) return;

        if (success) combo++;
        else combo = 0; 
    }
}