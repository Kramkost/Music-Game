using System;

public static class GameEventManager
{
    // События ритма и геймплея
    public static Action<bool> OnNoteResolved; // true = отбили, false = пропустили (урон)
    public static Action OnBeatPulse;          // Пульсация в такт
    
    // События состояний
    public static Action OnGameStart;
    public static Action<int> OnLevelComplete; // Передаем количество звезд
    public static Action OnGameOver;
}