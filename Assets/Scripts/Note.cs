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
    public int tutorialCycleId = -1;
    private Tweener currentTween;  // Tween 참조 저장
    private List<Vector3> notePositions;
    private int maxNoteInScreen;
    private Coroutine moveRoutine;
    [SerializeField] private GameObject missEffectPrefab;
    [SerializeField] private float hitMoveDuration = 0.1f;

    public void InitMove(List<Vector3> path, double interval, int max, float offsetRate)
    {

        notePositions = new List<Vector3>(path.Count);
        
        for(int i = 0; i<path.Count; i++)
        {
            if (i == 0) notePositions.Add(path[i]); 
            else if (i < path.Count - 1)
            {
                Vector3 tmp = Vector3.Lerp(path[i], path[i - 1], offsetRate);
                notePositions.Add(tmp);
            }
            else notePositions.Add(new Vector3(path[i].x + offsetRate, path[i].y, path[i].z));
        }
        
        intervalTime = interval;
        maxNoteInScreen = max;
        currentNoteIndex = 0;
        
        SetNoteMove(notePositions[currentNoteIndex]);

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine());
    }

    public void InitMove(List<Vector3> path, double interval, int max) => InitMove(path, interval, max, 0);

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

    public void PlayHit(Transform target)
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        KillTween();
        currentTween = transform.DOMove(target.position, hitMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));
    }

    public void PlayMiss()
    {
        KillTween();
        if (missEffectPrefab != null)
        {
            Instantiate(missEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
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
