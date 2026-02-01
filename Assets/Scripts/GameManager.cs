using System;
using UnityEngine;

public class GameManager : SingletonPersistence<GameManager>
{
    public GameState gameState;
    public event Action OnGameStateChange;
    
    public void UpdateGameState(GameState newGameState)
    {
        this.gameState = newGameState;
        
        this.OnGameStateChange?.Invoke();
    }
}

public enum GameState
{
    Playing,
    Menu,
    
}