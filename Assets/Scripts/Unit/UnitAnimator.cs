using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private List<UnitAnimation> animations = new List<UnitAnimation>();
    [SerializeField] private Vector3 distance;
    [SerializeField] private bool useExternalIndex = false;
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private int idleAfterBeats = 2;
    
    private int currentAnimationIndex = 0;
    private int activeAnimationIndex = -1;
    private int beatsUntilIdle = -1;
    private List<GameObject[]> animationPool = new List<GameObject[]>();
    private Sequence currentSequence;

    void Awake()
    {
        if (animations.Count == 0)
        {
            Debug.LogError("UnitAnimator: No animations assigned!");
            return;
        }
        
        // 미리 모든 애니메이션 생성
        for (int i = 0; i < animations.Count; i++)
        {
            GameObject head = Instantiate(animations[i].headPrefab, transform);
            GameObject body = Instantiate(animations[i].bodyPrefab, transform);
            
            head.transform.localPosition = Vector3.zero;
            body.transform.localPosition = Vector3.zero;
            
            head.SetActive(false);
            body.SetActive(false);
            
            animationPool.Add(new GameObject[] { head, body });
        }
        
        NoteManager.Instance.OnEveryBeat += HandleBeat;
        if (debugLogs)
        {
            Debug.Log($"UnitAnimator[{name}] subscribed. animations={animations.Count}");
        }
    }

    void OnDestroy()
    {
        if (NoteManager.Instance != null)
            NoteManager.Instance.OnEveryBeat -= HandleBeat;
        if (debugLogs)
        {
            Debug.Log($"UnitAnimator[{name}] unsubscribed.");
        }
        
        // DOTween 시퀀스 정리
        currentSequence?.Kill();
        
        // 풀링된 오브젝트 정리
        foreach (var pair in animationPool)
        {
            if (pair[0]) Destroy(pair[0]);
            if (pair[1]) Destroy(pair[1]);
        }
        animationPool.Clear();
    }
    
    public void PlayAnimation()
    {
        if (animations.Count == 0)
        {
            if (debugLogs) Debug.LogWarning($"UnitAnimator[{name}] PlayAnimation skipped: no animations.");
            return;
        }
        // 이전 시퀀스가 실행 중이면 종료
        currentSequence?.Kill();
        
        // 이전 애니메이션 비활성화
        if (activeAnimationIndex >= 0 && activeAnimationIndex < animationPool.Count)
        {
            animationPool[activeAnimationIndex][0].SetActive(false);
            animationPool[activeAnimationIndex][1].SetActive(false);
        }
        
        // 다음 애니메이션으로 전환
        if (!useExternalIndex)
        {
            currentAnimationIndex = (currentAnimationIndex + 1) % animations.Count;
        }
        else
        {
            currentAnimationIndex = Mathf.Clamp(currentAnimationIndex, 0, animations.Count - 1);
        }
        activeAnimationIndex = currentAnimationIndex;
        if (debugLogs)
        {
            Debug.Log($"UnitAnimator[{name}] PlayAnimation index={currentAnimationIndex} useExternalIndex={useExternalIndex}");
        }
        
        // 현재 애니메이션 활성화
        animationPool[currentAnimationIndex][0].SetActive(true);
        animationPool[currentAnimationIndex][1].SetActive(true);
        
        // 애니메이션 실행
        Vector3 currPos = transform.position;
        float quarterTime = (float)NoteManager.Instance.intervalTime / 4f;
        float halfTime = quarterTime * 2f;
        
        Transform headTransform = animationPool[currentAnimationIndex][0].transform;
        headTransform.localPosition = Vector3.zero; // 초기 위치로 리셋
        
        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(headTransform.DOMove(currPos + distance, quarterTime).SetEase(Ease.InOutCirc))
            .AppendInterval(quarterTime)
            .Append(headTransform.DOMove(currPos, halfTime).SetEase(Ease.InOutCirc))
            .OnComplete(() => {
                animationPool[currentAnimationIndex][0].SetActive(false);
                animationPool[currentAnimationIndex][1].SetActive(false);
            })
            .SetAutoKill(true);
    }

    public void SetAnimationIndex(int index)
    {
        if (animations.Count == 0) return;
        currentAnimationIndex = Mathf.Clamp(index, 0, animations.Count - 1);
        useExternalIndex = true;
        beatsUntilIdle = index == 0 ? -1 : idleAfterBeats;
        if (debugLogs)
        {
            Debug.Log($"UnitAnimator[{name}] SetAnimationIndex -> {currentAnimationIndex}");
        }
    }

    public void ClearAnimationIndexOverride()
    {
        useExternalIndex = false;
        beatsUntilIdle = -1;
        if (debugLogs)
        {
            Debug.Log($"UnitAnimator[{name}] ClearAnimationIndexOverride");
        }
    }

    private void HandleBeat()
    {
        if (useExternalIndex && currentAnimationIndex != 0 && idleAfterBeats > 0)
        {
            if (beatsUntilIdle > 0)
            {
                beatsUntilIdle--;
                if (beatsUntilIdle == 0)
                {
                    currentAnimationIndex = 0;
                }
            }
        }
        PlayAnimation();
    }
}

[Serializable]
public class UnitAnimation
{
    [SerializeField] private string animationName;
    public GameObject headPrefab;
    public GameObject bodyPrefab;
}
