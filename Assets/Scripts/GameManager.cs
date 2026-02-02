using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonPersistence<GameManager>
{
    public GameState gameState;
    public event Action OnGameStateChange;
    private int hp;
    public int maxHp = 200;
    public string currSong;

    protected override void Awake()
    {
        base.Awake();

        if (SceneManager.GetActiveScene().name == "Title") gameState = GameState.Title;
        else
        {
            gameState = GameState.Playing;
            hp = maxHp;
            StartCoroutine(DelayedStageSetup());
        }
    }

    public void UpdateGameState(GameState newGameState)
    {
        gameState = newGameState;

        switch (gameState)
        {
            case GameState.Playing:

                if(SceneManager.GetActiveScene().name != "Stage") SceneManager.LoadScene("Stage");

                hp = maxHp;
                StartCoroutine(DelayedStageSetup());
                break;
            case GameState.Title:
                SceneManager.LoadScene("Title");
                break;
            case GameState.Clear:
                break;
            case GameState.GameOver:
                if (NoteManager.Instance != null)
                {
                    NoteManager.Instance.ClearAllNotes();
                }
                UIManager.Instance.GameOverEffect();
                break;
        }
        OnGameStateChange?.Invoke();
    }

    private System.Collections.IEnumerator DelayedStageSetup()
    {
        yield return null;
        NoteManager.Instance.songId = currSong;
        NoteManager.Instance.Setting();
        UIManager.Instance.UIReset();
    }

    public void HpCheck(JudgeType judgeType)
    {
        switch (judgeType)
        {
            case JudgeType.Perfect:
                hp += 2;
                break;
            case JudgeType.Good:
                return;
            case JudgeType.Bad:
                hp -= 6;
                break;
            case JudgeType.Miss:
                hp -= 20;
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
    Title,
    Clear,
    GameOver,
}
