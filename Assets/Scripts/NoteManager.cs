using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class NoteManager : Singleton<NoteManager>
{
    public Transform center = null;
    public GameObject[] timingRect = null;
    public Vector2[] timingBoxes;
    List<GameObject> judgeNoteList = new List<GameObject>();
    public double noteSpeed; 
    public int bpm;
    public int maxNoteInScreen;
    public double intervalTime;
    public Vector3 startPos;
    public Vector3 endPos;
    private float xDelta;
    private float yDelta;
    
    private double currentTime = 0d;
    List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;
    public GameObject judgeNotePrefab;
    
    private List<Vector3> notePositions = new List<Vector3>();
    
    protected override void Awake()
    {
        base.Awake();
    }

    public void Setting()
    {
        intervalTime = 60d / bpm;
        noteSpeed = bpm / 60d;
        
        xDelta = (startPos.x - endPos.x)/maxNoteInScreen;
        yDelta = (startPos.y - endPos.y)/maxNoteInScreen;
        for (int i = 0; i < maxNoteInScreen; i++)
        {
            notePositions.Add(NotePosFunc(i));
        }

        timingBoxes = new Vector2[timingRect.Length];

        for (int i = 0; i < timingBoxes.Length; i++)
        {
            float width = timingRect[i].GetComponent<BoxCollider2D>().size.x/2;
            timingBoxes[i].Set(center.position.x - width, center.position.x + width);
        }
    }

    private Vector3 NotePosFunc(int x)
    {
        if(x == maxNoteInScreen-1) return new Vector3(-6.5f,-3, 0);
        Vector3 v = new Vector3((x+1)*xDelta,(x+1)*yDelta,0);
        return startPos - v;
    }
    void Update()
    {
        if (GameManager.Instance.gameState == GameState.Playing)
        {
            currentTime += Time.deltaTime;

            if (!(currentTime >= intervalTime)) return;
            NoteMove();
            UIManager.Instance.CallBeatEffect();
            currentTime -= intervalTime;
        }
    }

    public void CheckTiming(NoteType noteType)
    {
        List<int> toRemoveList = new List<int>();
        for (int i = 0; i < judgeNoteList.Count; i++)
        {
            float pos = judgeNoteList[i].transform.localPosition.x;

            for (int x = 0; x < timingBoxes.Length; x++)
            {
                if (timingBoxes[x].x <= pos && timingBoxes[x].y >= pos)
                {
                    CheckJudgeType((JudgeType)x);
                    toRemoveList.Add(i);
                    break;
                }
            }
        }

        foreach (int i in toRemoveList)
        {
            RemoveNote(judgeNoteList[i]);
        }
    }
    
    void NoteMove()
    {
        GenerateNote();
    
        foreach (GameObject note in notes)
        {
            Note n = note.GetComponent<Note>();

            n.currentNoteIndex++;  // 먼저 증가

            if (n.currentNoteIndex >= maxNoteInScreen)  // 증가 후 체크
            {
                continue;  // 범위 초과면 SetNoteMove 호출하지 않음
            }
        
            n.SetNoteMove(notePositions[n.currentNoteIndex]);
        }
    }

    void GenerateNote()
    {
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        GameObject judgeNote = Instantiate(judgeNotePrefab, new Vector3(maxNoteInScreen-1,-8,0), Quaternion.identity);
        note.transform.SetParent(transform);
        judgeNote.transform.SetParent(transform);
        
        judgeNote.GetComponent<JudgeNote>().noteVisual = note;
        notes.Add(note);
        judgeNoteList.Add(judgeNote);
    }

    public void RemoveNote(GameObject judgeNote)
    {
        GameObject visualNote = judgeNote.GetComponent<JudgeNote>().noteVisual;
        notes.Remove(visualNote);
        visualNote.GetComponent<Note>().KillTween();  // Tween 종료 후 Destroy
        Destroy(visualNote);
        
        judgeNoteList.Remove(judgeNote);
        Destroy(judgeNote);
    }
    public void CheckJudgeType(JudgeType judgeType)
    {
        GameManager.Instance.HpCheck(judgeType);
        print(judgeType);
    }
}

public enum JudgeType
{
    Perfect,
    Good,
    Bad,
    Miss
}

public enum NoteType
{
    Up,
    Down,
    Left,
    Right,
}


