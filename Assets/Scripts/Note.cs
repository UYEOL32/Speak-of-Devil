using System;
using UnityEngine;
using DG.Tweening;
public class Note : MonoBehaviour
{
    private double intervalTime;
    public int currentNoteIndex = 0;
    public int action = 0;
    public void Awake()
    {
        intervalTime = NoteManager.Instance.intervalTime;
    }

    public void SetNoteMove(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, (float)intervalTime/2);
    }
}
