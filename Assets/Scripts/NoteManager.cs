using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NoteManager : Singleton<NoteManager>
{
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
    GameObject toRemove = null;
    
    private List<Vector3> notePositions = new List<Vector3>();
    private GameObject noteInJudge = null;
    private GameObject nextNoteInJudge = null;
    
    protected override void Awake()
    {
        intervalTime = 60d / bpm;
        
        xDelta = (startPos.x - endPos.x)/maxNoteInScreen;
        yDelta = (startPos.y - endPos.y)/maxNoteInScreen;
        for (int i = 0; i < maxNoteInScreen; i++)
        {
            notePositions.Add(NotePosFunc(i));
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
        currentTime += Time.deltaTime;

        if (currentTime >= intervalTime / 2d)
        {
            noteInJudge = nextNoteInJudge ? nextNoteInJudge : null;
        }
        if (currentTime >= intervalTime)
        {
            NoteMove();
            currentTime -= intervalTime;
        }
    }

    public void CheckTiming(NoteType noteType)
    {
        if (noteInJudge == null) return;

        //노트 판정
        if (noteInJudge.GetComponent<Note>().noteType != noteType) CheckJudgeType(JudgeType.Miss);
        else if (currentTime <= intervalTime*0.15d || currentTime >= intervalTime*0.85d) CheckJudgeType(JudgeType.Perfect);
        else if (currentTime <= intervalTime*0.2d || currentTime >= intervalTime*0.8d) CheckJudgeType(JudgeType.Good);
        else if (currentTime <= intervalTime*0.3d || currentTime >= intervalTime*0.7d) CheckJudgeType(JudgeType.Bad);
        else CheckJudgeType(JudgeType.Miss);
        
        RemoveNote(noteInJudge);
    }
    
    void NoteMove()
    {
        GenerateNote();
        
        //느림 판정을 위해 1비트 뒤에 제거
        if (toRemove) RemoveNote(toRemove);
        else toRemove = null;
    
        foreach (GameObject note in notes)
        {
            Note n = note.GetComponent<Note>();

            n.currentNoteIndex++;  // 먼저 증가

            if (n.currentNoteIndex >= maxNoteInScreen)  // 증가 후 체크
            {
                CheckJudgeType(JudgeType.Miss);
                toRemove = note;
                continue;  // 범위 초과면 SetNoteMove 호출하지 않음
            }

            if (n.currentNoteIndex == maxNoteInScreen - 1) nextNoteInJudge = note;
        
            n.SetNoteMove(notePositions[n.currentNoteIndex]);
        }
    }

    void GenerateNote()
    {
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        
        notes.Add(note);
    }

    void RemoveNote(GameObject note)
    {
        noteInJudge = null;
        notes.Remove(note);
        note.GetComponent<Note>().KillTween();  // Tween 종료 후 Destroy
        Destroy(note);
    }
    public void CheckJudgeType(JudgeType judgeType)
    {
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


