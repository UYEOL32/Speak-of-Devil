using System.Collections.Generic;
using UnityEngine;

public class NoteManager : Singleton<NoteManager>
{
    public int bpm;
    public int maxNoteInScreen;
    public double intervalTime;
    
    private double currentTime = 0d;
    List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;

    private List<Vector3> notePositions = new List<Vector3>();

    void Awake()
    {
        intervalTime = 60d / bpm;
        
        for (int i = 0; i < maxNoteInScreen; i++)
        {
            notePositions.Add(NotePosFunc(i));
        }
    }

    private Vector3 NotePosFunc(int x)
    {
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
            
            n.currentNoteIndex++;
            n.SetNoteMove(notePositions[n.currentNoteIndex]);
        }

        if (toRemove)
        {
            notes.Remove(toRemove);
            Destroy(toRemove);
        }
    }

    void GenerateNote()
    {
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        
        notes.Add(note);
    }
}
