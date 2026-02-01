using System.Collections.Generic;
using UnityEngine;

public class NoteManager : Singleton<NoteManager>
{
    public int bpm;
    public int maxNoteInScreen;
    public double intervalTime;
    public Vector2 startPos;
    public Vector2 endPos;
    private float xDelta;
    private float yDelta;
    
    private double currentTime = 0d;
    List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;

    private List<Vector3> notePositions = new List<Vector3>();
    private GameObject noteInJudge = null;
    protected override void Awake()
    {
        intervalTime = 60d / bpm;
        
        xDelta = 
        for (int i = 0; i < maxNoteInScreen; i++)
        {
            notePositions.Add(NotePosFunc(i));
        }
    }

    private Vector3 NotePosFunc(int x)
    {
        if(x == maxNoteInScreen-1) return new Vector3(-6.5f,-3, 0);
        return new Vector3(9 - x*2f, 2 - x*0.7f, 0);
    }
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= intervalTime)
        {
            NoteMove();
            currentTime -= intervalTime;
        }
    }

    public void CheckTiming(NoteType noteType)
    {
        JudgeType currJudgeType;
        
        if (noteInJudge == null) return;

        //노트 판정
        if (noteInJudge.GetComponent<Note>().noteType != noteType) currJudgeType = JudgeType.Miss;
        else if (currentTime <= intervalTime*0.15d || currentTime >= intervalTime*0.85d) currJudgeType = JudgeType.Perfect;
        else if (currentTime <= intervalTime*0.2d || currentTime >= intervalTime*0.8d) currJudgeType = JudgeType.Good;
        else if (currentTime <= intervalTime*0.3d || currentTime >= intervalTime*0.7d) currJudgeType = JudgeType.Bad;
        else currJudgeType = JudgeType.Miss;
        
        print(currJudgeType);
        RemoveNote(noteInJudge);
    }
    
    void NoteMove()
    {
        GenerateNote();

        GameObject toRemove = null;
        
        foreach (GameObject note in notes)
        {
            Note n = note.GetComponent<Note>();

            if (n.currentNoteIndex + 1 >= maxNoteInScreen)
            {
                toRemove = note;
                continue;
            }
            if (n.currentNoteIndex + 2 == maxNoteInScreen) noteInJudge = note;
            
            n.currentNoteIndex++;
            n.SetNoteMove(notePositions[n.currentNoteIndex]);
        }

        if (toRemove) RemoveNote(toRemove);
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
        Destroy(note);
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
