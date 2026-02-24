using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, LevelComplete }
    
    [SerializeField] private GameState currentState = GameState.Menu;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalStarsEarned = 0;
    
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

    private void HandleNoteResolved(bool success)
    {
        if (currentState != GameState.Playing) return;

        if (success) combo++;
        else combo = 0; // Сброс комбо при ошибке
    }

    // Логика разблокировки из твоего ТЗ
    public bool IsNextLevelUnlocked(int nextLevelIndex)
    {
        // Формула: StarsNeeded = ⌈(TotalLevels * 5) / 2⌉
        // В данном контексте nextLevelIndex - это по сути TotalLevels для подсчета
        int starsNeeded = Mathf.CeilToInt((nextLevelIndex * 5f) / 2f);
        return totalStarsEarned >= starsNeeded;
    }
}