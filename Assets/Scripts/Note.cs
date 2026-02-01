using System;
using UnityEngine;
using DG.Tweening;

public class Note : MonoBehaviour
{
    private double intervalTime;
    public int currentNoteIndex = 0;
    public NoteType noteType;
    private Tweener currentTween;  // Tween 참조 저장

    public void Awake()
    {
        intervalTime = NoteManager.Instance.intervalTime;
        noteType = NoteType.Up;
    }

    public void SetNoteMove(Vector3 targetPosition)
    {
        currentTween?.Kill();  // 이전 Tween 종료
        currentTween = transform.DOMove(targetPosition, (float)intervalTime / 2);
    }

    public void KillTween()
    {
        currentTween?.Kill();
    }
}