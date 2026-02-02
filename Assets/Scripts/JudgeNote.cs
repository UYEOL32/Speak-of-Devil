using System;
using JetBrains.Annotations;
using UnityEngine;

public class JudgeNote : MonoBehaviour
{
    private Double noteSpeed;
    public GameObject noteVisual;

    void Awake()
    {
        noteSpeed = NoteManager.Instance.noteSpeed;
    }
    void Update()
    {
        if (GameManager.Instance.gameState == GameState.Playing)
        {
            transform.position += Vector3.left * ((float)noteSpeed * Time.deltaTime);

        if (!(NoteManager.Instance.timingBoxes[2].x > transform.position.x)) return;
        NoteManager.Instance.CheckJudgeType(JudgeType.Miss);
        NoteManager.Instance.RemoveFrontNote();
        }
    }
}
