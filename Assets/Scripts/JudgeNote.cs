using System;
using JetBrains.Annotations;
using UnityEngine;

public class JudgeNote : MonoBehaviour
{
    private Double noteSpeed;
    public GameObject noteVisual;
    public NoteType noteType;
    private bool isResolved;
    void Awake()
    {
        noteSpeed = NoteManager.Instance.noteSpeed;
    }
    void Update()
    {
        if (isResolved) return;
        if (GameManager.Instance.gameState == GameState.Playing)
        {
            transform.position += Vector3.left * ((float)noteSpeed * Time.deltaTime);

            if (!(NoteManager.Instance.timingBoxes[2].x > transform.position.x)) return;
            NoteManager.Instance.CheckJudgeType(JudgeType.Miss, noteType);
            NoteManager.Instance.RemoveFrontNote(JudgeType.Miss);
            isResolved = true;
            enabled = false;
        }
    }

    public void MarkResolved()
    {
        isResolved = true;
        enabled = false;
    }
}
