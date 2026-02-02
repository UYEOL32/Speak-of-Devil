using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Note : MonoBehaviour
{
    private double intervalTime;
    public int currentNoteIndex = 0;
    public NoteType noteType;
    private Tweener currentTween;  // Tween 참조 저장
    private List<Vector3> notePositions;
    private int maxNoteInScreen;
    private Coroutine moveRoutine;

    public void Awake()
    {
        noteType = NoteType.Up;
    }

    public void InitMove(List<Vector3> path, double interval, int max)
    {
        notePositions = path;
        intervalTime = interval;
        maxNoteInScreen = max;
        currentNoteIndex = 0;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        while (currentNoteIndex < maxNoteInScreen - 1)
        {
            yield return new WaitForSeconds((float)intervalTime);
            currentNoteIndex++;

            if (notePositions == null || currentNoteIndex >= notePositions.Count) yield break;
            SetNoteMove(notePositions[currentNoteIndex]);
        }
    }

    public void SetNoteMove(Vector3 targetPosition)
    {
        currentTween?.Kill();  // 이전 Tween 종료
        currentTween = transform.DOMove(targetPosition, (float)intervalTime / 2).SetEase(Ease.OutExpo);
    }

    public void SetVisualDirection(NoteType type)
    {
        if (transform.childCount == 0) return;

        float angle;
        switch (type)
        {
            case NoteType.Up:
                angle = 0f;
                break;
            case NoteType.Right:
                angle = -90f;
                break;
            case NoteType.Down:
                angle = 180f;
                break;
            case NoteType.Left:
                angle = 90f;
                break;
            default:
                angle = 0f;
                break;
        }

        Transform visual = transform.GetChild(0);
        visual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void KillTween()
    {
        currentTween?.Kill();
        if (moveRoutine != null) StopCoroutine(moveRoutine);
    }
}
