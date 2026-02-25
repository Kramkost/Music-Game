using UnityEngine;
using DG.Tweening; // Для анимации меню

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, LevelComplete }
    
    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalStarsEarned = 0;
    
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup mainMenuPanel; // CanvasGroup нужен для плавного изменения прозрачности (Alpha)

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

    // ЭТУ ФУНКЦИЮ МЫ ПОВЕСИМ НА КНОПКУ PLAY
    public void UI_StartGameButton()
    {
        // Отключаем взаимодействие, чтобы игрок не спамил кнопку
        mainMenuPanel.interactable = false; 

        // DOTween: Плавно растворяем меню за 0.5 секунд
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