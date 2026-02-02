using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;


public class UIManager : Singleton<UIManager>
{
    [Header("UI Elements")]
    [SerializeField] GameObject beatEffect;
    [SerializeField] Slider hpBar;
    [SerializeField] RectTransform gameOverEffect;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] Button restartButton;
    [SerializeField] Button exitButton;

    [SerializeField] private float animationDuration;
    private Coroutine beatCoroutine;

    public void UIReset()
    {
        beatEffect.SetActive(false);
        restartButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0f);
        gameOverEffect.anchoredPosition = new Vector2(0, 1080);
        hpBar.maxValue = GameManager.Instance.maxHp;
        hpBar.value = hpBar.maxValue;
    }
    
    void Start()
    {
        // UIReset();
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
    
    public void TakeDamage(float currHp)
    {
//         currHp = Mathf.Clamp(currHp, 0, hpBar.maxValue);
        
        // Slider value를 부드럽게 애니메이션
        hpBar.DOValue(currHp, animationDuration).SetEase(Ease.OutExpo);
    }

    public void GameOverEffect()
    {
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
}
