using System.Collections.Generic;
using UnityEngine;

public class NoteManager : Singleton<NoteManager>
{
    public int bpm;
    public int maxNoteInScreen;
    public double intervalTime;
    public string chartName = "Test";
    public int leadBeats = 0;
    
    private double currentTime = 0d;
    private double songStartTime = 0d;
    private double leadTimeMs = 0d;
    private int nextEventIndex = 0;
    private ChartData chart;
    List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;

    private List<Vector3> notePositions = new List<Vector3>();

    void Awake()
    {
        chart = LoadChart(chartName);
        if (chart != null)
        {
            bpm = Mathf.RoundToInt(chart.bpm);
        }

        intervalTime = 60d / bpm;
        int beats = leadBeats > 0 ? leadBeats : (maxNoteInScreen - 1);
        leadTimeMs = beats * intervalTime * 1000d;
        
        for (int i = 0; i < maxNoteInScreen; i++)
        {
            notePositions.Add(NotePosFunc(i));
        }
    }
    
    void Start()
    {
        songStartTime = Time.time;
    }

    private Vector3 NotePosFunc(int x)
    {
        return new Vector3(9 - x*2f, 2 - x*0.7f, 0);
    }
    void Update()
    {
        currentTime += Time.deltaTime;
        TrySpawnScheduledNotes();

        if (currentTime >= intervalTime)
        {
            NoteMove();
            currentTime -= intervalTime;
        }
    }
    
    void NoteMove()
    {
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

    void GenerateNote(ChartEvent chartEvent)
    {
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        Note n = note.GetComponent<Note>();
        if (n != null)
        {
            n.action = chartEvent.action;
        }
        notes.Add(note);
    }
    
    void TrySpawnScheduledNotes()
    {
        if (chart == null || chart.events == null || chart.events.Length == 0)
        {
            return;
        }

        double songTimeMs = (Time.time - songStartTime) * 1000d;
        while (nextEventIndex < chart.events.Length)
        {
            ChartEvent e = chart.events[nextEventIndex];
            double spawnTimeMs = e.timeMs - leadTimeMs;
            if (songTimeMs + 0.001d < spawnTimeMs)
            {
                break;
            }
            GenerateNote(e);
            nextEventIndex++;
        }
    }

    ChartData LoadChart(string name)
    {
        TextAsset json = Resources.Load<TextAsset>(name);
        if (json == null)
        {
            Debug.LogError("Chart not found: " + name);
            return null;
        }
        return JsonUtility.FromJson<ChartData>(json.text);
    }
}

[System.Serializable]
public class ChartEvent
{
    public int timeMs;
    public int action;
}

[System.Serializable]
public class ChartData
{
    public string songId;
    public int version;
    public float bpm;
    public int snapOffsetMs;
    public ChartEvent[] events;
}
