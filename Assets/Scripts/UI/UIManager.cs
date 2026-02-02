using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using AYellowpaper.SerializedCollections;

public class UIManager : Singleton<UIManager>
{
    [Header("UI Elements")]
    [SerializeField] GameObject beatEffect;
    [SerializeField] RectTransform gameOverEffect;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] Button restartButton;
    [SerializeField] Button exitButton;
    [SerializeField] GameObject hpBarObject;
    
    [SerializeField] List<JudgementIcon> judgeIcons = new List<JudgementIcon>();
    [SerializeField] Image arrowImage;
    [SerializeField] List<Sprite> judgeTexts = new List<Sprite>();
    [SerializeField] Image judgeTextImage;
    [SerializeField] Image speechBubble;
    
    [SerializeField] private float animationDuration;
    private Coroutine beatCoroutine;
    
    public void UIReset()
    {
        beatEffect?.SetActive(false);
        restartButton?.gameObject.SetActive(false);
        exitButton?.gameObject.SetActive(false);
        arrowImage?.gameObject.SetActive(true);
        judgeTextImage?.gameObject.SetActive(true);
        speechBubble?.gameObject.SetActive(true);
        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0f);
        gameOverEffect.anchoredPosition = new Vector2(0, 1080);
        hpBarObject.GetComponent<HPVisualController>().GenerateHpVisual();
        
    }

    public void CallBeatEffect()
    {
        if (beatCoroutine != null)
        {
            StopCoroutine(beatCoroutine); // 기존 코루틴 중단
        }
        beatCoroutine = StartCoroutine(BeatEffect()); // 새로운 코루틴 시작
    }

    private IEnumerator BeatEffect()
    {
        beatEffect.SetActive(true);
        float elapsedTime = 0f;
        float fadedTime = (float)NoteManager.Instance.intervalTime*0.6f;
        var renderer = beatEffect.GetComponent<CanvasRenderer>();

        while (elapsedTime <= fadedTime)
        {
            renderer.SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadedTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderer.SetAlpha(0f);
        beatEffect.SetActive(false);
    }
    
    public void TakeDamage(int currHp)
    {
        if(currHp<0) currHp = 0;
        
        // hpBarObject.GetComponent<HPVisualController>().UpdateHpBar(currHp);
    }

    public void GameOverEffect()
    {
        arrowImage.gameObject.SetActive(false);
        judgeTextImage.gameObject.SetActive(false);
        speechBubble.gameObject.SetActive(false);
        Vector2 targetPos = new Vector2(0, 0);
        gameOverEffect.anchoredPosition = new Vector2(0,1080);
        
        gameOverEffect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => 
        {
            gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0f);
            gameOverText.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    restartButton.gameObject.SetActive(true);
                    exitButton.gameObject.SetActive(true);
                });
        });
    }

    public void OnClickExit()
    {
        GameManager.Instance.UpdateGameState(GameState.Title);
    }

    public void OnClickRestart()
    {
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }

    public void ChangeJudgeIconAndJudgeText(JudgeType judgeType, NoteType noteType)
    {
        arrowImage.sprite = judgeIcons[(int)noteType].judgementSprites[(int)judgeType];
        judgeTextImage.sprite = judgeTexts[(int)judgeType];
    }
}

[Serializable]
public class JudgementIcon
{
    public List<Sprite> judgementSprites;
}
