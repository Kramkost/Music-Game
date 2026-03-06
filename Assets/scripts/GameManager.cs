using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement; 
using YG;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, LevelComplete }
    
    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalStarsEarned = 0;
    
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private GameObject levelCompletePanel; 

    private int combo = 0;
    private int maxComboInThisRun = 0; 

    private void OnEnable()
    {
        GameEventManager.OnNoteResolved += HandleNoteResolved;
        GameEventManager.OnGameStart += () => currentState = GameState.Playing;
        GameEventManager.OnLevelComplete += HandleLevelComplete;
    }

    private void OnDisable()
    {
        GameEventManager.OnNoteResolved -= HandleNoteResolved;
        GameEventManager.OnGameStart -= () => currentState = GameState.Playing;
        GameEventManager.OnLevelComplete -= HandleLevelComplete;
    }

    private void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    // ==========================================
    // КНОПКА: НАЧАТЬ ИГРУ
    // ==========================================
    public void UI_StartGameButton()
    {
        mainMenuPanel.interactable = false; 
        combo = 0;
        maxComboInThisRun = 0;

        mainMenuPanel.DOFade(0f, 0.5f).OnComplete(() =>
        {
            mainMenuPanel.gameObject.SetActive(false);
            GameEventManager.OnGameStart?.Invoke();
        });
    }

    // ==========================================
    // КНОПКА: ВЫЙТИ В МЕНЮ (ПОСЛЕ ПОБЕДЫ)
    // ==========================================
    public void UI_ReturnToMenuButton()
    {

        Time.timeScale = 1f;
        AudioListener.pause = false;
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleNoteResolved(bool success)
    {
        if (currentState != GameState.Playing) return;

        if (success) 
        {
            combo++;
            if (combo > maxComboInThisRun) maxComboInThisRun = combo;
        }
        else 
        {
            combo = 0; 
        }
    }

    // ==========================================
    // ЛОГИКА ПОБЕДЫ, ЗВЕЗД И СОХРАНЕНИЯ В ЯНДЕКС
    // ==========================================
private void HandleLevelComplete(int defaultStarsFromSpawner)
    {
        currentState = GameState.LevelComplete;
        
        if (levelCompletePanel != null) 
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.localScale = Vector3.zero;
            levelCompletePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        int earnedStars = 0;
        if (maxComboInThisRun >= 30) earnedStars = 3;
        else if (maxComboInThisRun >= 15) earnedStars = 2;
        else if (maxComboInThisRun >= 5) earnedStars = 1;

        // ==========================================
        // ЗАПИСЬ РЕКОРДА И ОТПРАВКА В ЛИДЕРБОРД
        // ==========================================
        if (maxComboInThisRun > YG2.saves.maxCombo)
        {
            
            YG2.saves.maxCombo = maxComboInThisRun;
            
            
            YG2.SetLeaderboard("MaxComboLB", maxComboInThisRun);
            
            Debug.Log("Новый рекорд комбо: " + YG2.saves.maxCombo + " отправлен в лидерборд!");
        }

        YG2.saves.totalStars += earnedStars;

        
        Debug.Log($"Уровень пройден! Получено звезд: {earnedStars}. Макс. комбо: {maxComboInThisRun}. Прогресс сохранен.");
    }
}