using UnityEngine;
using DG.Tweening;

public class DeadBodyController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset;

    private Sequence currentSequence;
    private bool isSubscribed = false;
    private NoteManager noteManager;

    void Awake()
    {
        if (target == null)
        {
            target = transform;
        }
        noteManager = FindFirstObjectByType<NoteManager>();
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
        if (noteManager == null) return;

        currentSequence?.Kill();

        float quarterTime = (float)noteManager.intervalTime / 4f;
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
        if (noteManager == null) return;
        noteManager.OnEveryBeat += PlayAnimation;
        isSubscribed = true;
    }

    public void UnsubscribeFromBeat()
    {
        if (!isSubscribed) return;
        if (noteManager != null)
        {
            noteManager.OnEveryBeat -= PlayAnimation;
        }
        isSubscribed = false;
    }
}
