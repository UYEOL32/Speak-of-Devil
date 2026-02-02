using System;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] List<GameObject> titleObjects = new List<GameObject>();
    [SerializeField] List<GameObject> stageSelectObjects = new List<GameObject>();
    public UIState uiState;

    private void Awake()
    {
        UpdateUIState(UIState.Title);
    }

    private void UpdateUIState(UIState newUIState)
    {
        uiState = newUIState;

        switch (uiState)
        {
            case UIState.Title:
                ControlActive(true,titleObjects);
                ControlActive(false,stageSelectObjects);
                break;
            case UIState.LevelSelect:
                ControlActive(false,titleObjects);
                ControlActive(true,stageSelectObjects);
                break;
        }
    }

    void ControlActive(bool active, List<GameObject> objects)
    {
        foreach (GameObject obj in objects) obj.SetActive(active);
    }
    public void OnClickStart()
    {
        UpdateUIState(UIState.LevelSelect);
    }
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

    public void OnClickStage(string songName)
    {
        GameManager.Instance.currSong = songName;
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }
    
}

public enum UIState
{
    Title,
    LevelSelect,
}