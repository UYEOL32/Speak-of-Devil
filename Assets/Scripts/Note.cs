using System;
using UnityEngine;
using DG.Tweening;
public class Note : MonoBehaviour
{
    private double intervalTime;
    public int currentNoteIndex = 0;
    public NoteType noteType;
    public void Awake()
    {
        intervalTime = NoteManager.Instance.intervalTime;
        noteType = NoteType.Up;
    }

    public void SetNoteMove(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, (float)intervalTime/2);
    }
}
