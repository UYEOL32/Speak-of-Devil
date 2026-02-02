using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonPersistence<GameManager>
{
    public GameState gameState;
    public event Action OnGameStateChange;
    private int hp;
    public int maxHp = 100;
    public string currSong;

    protected override void Awake()
    {
        base.Awake();

        if (SceneManager.GetActiveScene().name == "Title") UpdateGameState(GameState.Menu);
        else UpdateGameState(GameState.Playing);
    }

    public void UpdateGameState(GameState newGameState)
    {
        gameState = newGameState;

        switch (gameState)
        {
            case GameState.Playing:
                if(SceneManager.GetActiveScene().name != "Stage") SceneManager.LoadScene("Stage");
                hp = maxHp;
                NoteManager.Instance.Setting();
                break;
            case GameState.Menu:
                break;
            case GameState.Clear:
                break;
            case GameState.GameOver:
                UIManager.Instance.GameOverEffect();
                break;
        }
        OnGameStateChange?.Invoke();
    }

    public void HpCheck(JudgeType judgeType)
    {
        switch (judgeType)
        {
            case JudgeType.Perfect:
                hp += 1;
                break;
            case JudgeType.Good:
                return;
            case JudgeType.Bad:
                hp -= 3;
                break;
            case JudgeType.Miss:
                hp -= 10;
                break;
        }
        UIManager.Instance.TakeDamage(hp);

        if (hp <= 0)
        {
            hp = 0;
            UpdateGameState(GameState.GameOver);
        }
    }
}

public enum GameState
{
    Playing,
    Menu,
    Clear,
    GameOver,
}