using UnityEngine;
using DG.Tweening;

public class DeadBodyController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset;

    private Sequence currentSequence;
    private bool isSubscribed = false;

    void Awake()
    {
        if (target == null)
        {
            target = transform;
        }
    }

    void OnEnable()
    {
        SubscribeToBeat();
    }

    void OnDisable()
    {
        UnsubscribeFromBeat();
        currentSequence?.Kill();
    }

    public void PlayAnimation()
    {
        if (NoteManager.Instance == null) return;

        currentSequence?.Kill();

        float quarterTime = (float)NoteManager.Instance.intervalTime / 4f;
        float halfTime = quarterTime * 2f;
        Vector3 startPos = target.localPosition;

        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(target.DOLocalMove(startPos + localOffset, quarterTime).SetEase(Ease.InOutCirc))
            .AppendInterval(quarterTime)
            .Append(target.DOLocalMove(startPos, halfTime).SetEase(Ease.InOutCirc))
            .SetAutoKill(true);
    }

    public void SubscribeToBeat()
    {
        if (isSubscribed) return;
        if (NoteManager.Instance == null) return;
        NoteManager.Instance.OnEveryBeat += PlayAnimation;
        isSubscribed = true;
    }

    public void UnsubscribeFromBeat()
    {
        if (!isSubscribed) return;
        if (NoteManager.Instance != null)
        {
            NoteManager.Instance.OnEveryBeat -= PlayAnimation;
        }
        isSubscribed = false;
    }
}