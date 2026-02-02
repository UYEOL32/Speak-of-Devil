using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HPVisualController : MonoBehaviour
{
    [SerializeField] GameObject _hpVisualPrefab;
    [SerializeField] float delayBetweenBars = 0.01f;
    [SerializeField] float moveDistance = 0.2f;
    [SerializeField] float moveDuration = 0.5f;
    [SerializeField] float fadeDuration = 0.3f;
    [SerializeField] private Color StartColor;
    [SerializeField] private Color EndColor;
    private int hp;
    List<GameObject> hpBars = new List<GameObject>();
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    
    public void GenerateHpVisual()
    {
        hp = GameManager.Instance.maxHp;

        // 기존 바 제거
        foreach (var bar in hpBars)
        {
            if (bar != null)
            {
                bar.transform.DOKill(); // DOTween 애니메이션 종료
                Destroy(bar);
            }
        }
        hpBars.Clear();
        spriteRenderers.Clear();

        int barCount = Mathf.CeilToInt(GameManager.Instance.maxHp / 5f);

        for (int i = 0; i < barCount; i++)
        {
            Vector3 delta = new Vector3(0, i * 0.1f, 0);
            GameObject hpBar = Instantiate(_hpVisualPrefab, transform.position + delta, Quaternion.identity, transform);
            hpBars.Add(hpBar);
            spriteRenderers.Add(hpBar.GetComponent<SpriteRenderer>());
    
            Color color = StartColor; // StartColor로 시작
            color.a = 1f - (float)i / (barCount - 1); // 투명도 그라데이션 유지
    
            spriteRenderers[i].color = color;
        }

        StartWaveAnimation();
    }
    
    void StartWaveAnimation()
    {
        for (int i = 0; i < hpBars.Count; i++)
        {
            float delay = i * delayBetweenBars;
            
            hpBars[i].transform.DOLocalMoveX(moveDistance, moveDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay)
                .SetLink(hpBars[i]); // 오브젝트와 링크 (중요!)
        }
    }

    public void UpdateHpBar(int newHp)
    {
        if (newHp == hp) return;
    
        bool isDecreasing = newHp < hp;
        int start = isDecreasing ? newHp : hp;
        int end = isDecreasing ? hp : newHp;
    
        // 직접 계산 (리스트 생성 없이)
        int firstMultiple = ((start / 5) + 1) * 5;
    
        for (int multiple = firstMultiple; multiple <= end; multiple += 5)
        {
            int barIndex = (multiple / 5) - 1;
        
            if (barIndex >= 0 && barIndex < spriteRenderers.Count)
            {
                if (isDecreasing)
                    FadeOutBar(barIndex);
                else
                    FadeInBar(barIndex);
            }
        }
    
        hp = newHp;
    
        // 모든 HP 바의 색상 업데이트
        UpdateAllBarColors();
    }

    void UpdateAllBarColors()
    {
        // HP 비율 계산 (0: EndColor, 1: StartColor)
        float hpRatio = Mathf.Clamp01((float)hp / GameManager.Instance.maxHp);
    
        // 색상 보간
        Color targetColor = Color.Lerp(EndColor, StartColor, hpRatio);
    
        // 모든 바에 적용
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color newColor = targetColor;
                // 기존 알파값 유지 (투명도 그라데이션)
                newColor.a = spriteRenderers[i].color.a;
                spriteRenderers[i].color = newColor;
            }
        }
    }

    void FadeOutBar(int index)
    {
        if (spriteRenderers[index] != null)
        {
            spriteRenderers[index].DOFade(0f, fadeDuration)
                .SetLink(hpBars[index]); // 오브젝트와 링크
        }
    }

    void FadeInBar(int index)
    {
        if (spriteRenderers[index] != null)
        {
            spriteRenderers[index].DOFade(1 - (float)index/spriteRenderers.Count, fadeDuration)
                .SetLink(hpBars[index]); // 오브젝트와 링크
        }
    }
    
    void OnDestroy()
    {
        // 컴포넌트가 파괴될 때 모든 DOTween 애니메이션 종료
        transform.DOKill();
        foreach (var bar in hpBars)
        {
            if (bar != null) bar.transform.DOKill();
        }
        foreach (var sr in spriteRenderers)
        {
            if (sr != null) sr.DOKill();
        }
    }
}