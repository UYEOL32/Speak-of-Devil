using System;
using System.Collections.Generic;
using DG.Tweening;
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
                LevelSelectIntro();
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
    
    public RectTransform centerPoint;
    public float radius;
    public float duration;
    public float startAngle; // 시작 기준 각도
    public Ease easeType = Ease.OutExpo;

    public void LevelSelectIntro()
    {
        int count = stageSelectObjects.Count - 4;

        for (int i = count - 1; i >= 0; i--)
        {
            int index = i;
            RectTransform rect = stageSelectObjects[index].GetComponent<RectTransform>();
        
            float targetAngle = startAngle - 165f - (30f / (count-1)) * index;
            if (i == 0) targetAngle += 5f;
            float delay = (count - 1 - index) * 0.2f; // delay도 역순으로 조정
        
            SetPositionByAngle(rect, startAngle);

            DOVirtual.Float(startAngle, targetAngle, duration, (angle) => 
                {
                    SetPositionByAngle(rect, angle);
                })
                .SetEase(easeType)
                .SetDelay(delay)
                .SetLink(stageSelectObjects[index]);
        }
    }

    private void SetPositionByAngle(RectTransform rect, float angle)
    {
        // 유니티의 수학 함수는 라디안을 사용합니다.
        float radian = angle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(
            Mathf.Cos(radian) * radius,
            Mathf.Sin(radian) * radius
        );

        // 중심점으로부터의 상대적 위치 계산
        rect.anchoredPosition = centerPoint.anchoredPosition + offset;

        // 원과 수직이 되도록 회전 (접선 방향)
        // 접선은 반지름에서 90도 더한 각도
        rect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}

public enum UIState
{
    Title,
    LevelSelect,
}