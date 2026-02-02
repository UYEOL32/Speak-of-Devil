using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NoteManager : Singleton<NoteManager>
{
    public Transform center = null;
    public GameObject[] timingRect = null;
    public Vector2[] timingBoxes;
    private readonly Queue<GameObject> judgeNoteQueue = new Queue<GameObject>();
    public double noteSpeed;    
    public int bpm;
    public int maxNoteInScreen;
    public double intervalTime;
    public Vector3 startPos;
    public Vector3 endPos;
    private float xDelta;
    private float yDelta;

    [Header("AUDIO SOURCE AND...!!")] public AudioSource audioSource;
    public double startDelaySec = 0.1d;
    public TMP_Text dspTime;
    
    private double currentTime = 0d;
    private readonly List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;
    public GameObject judgeNotePrefab;

    [Header("CHART")]
    public string chartResourceName = "Test";
    public float leadBeats = 5f;
    public bool debugLeadBeat = false;
    public bool debugFourBeat = false;
    
    private List<Vector3> notePositions = new List<Vector3>();
    private NoteChart chart;
    private int nextEventIndex = 0;
    private int nextFourBeatIndex = 0;
    private float leadTimeMs;
    private float fourBeatMs;
    private int lastBeatIndex = -1;
    private double startDspTime;
    private bool isPlaybackScheduled = false;

    public Action OnEveryBeat;
    public void Setting()
    {
        LoadChart();
        if (bpm <= 0)
        {
            Debug.LogError("BPM must be greater than 0.");
            return;
        }
        if (maxNoteInScreen <= 0)
        {
            Debug.LogError("maxNoteInScreen must be greater than 0.");
            return;
        }
        intervalTime = 60d / bpm;
        noteSpeed = bpm / 60d;
        leadTimeMs = leadBeats * (60000f / bpm);
        fourBeatMs = 4f * (60000f / bpm);
        currentTime = 0d;
        nextEventIndex = 0;
        nextFourBeatIndex = 0;
        lastBeatIndex = -1;
        SchedulePlayback();
        
        notePositions.Clear();
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
        // dspTime.text = AudioSettings.dspTime.ToString("0.00");
        if (GameManager.Instance.gameState == GameState.Playing)
        {
            if (!isPlaybackScheduled) return;
            if (AudioSettings.dspTime < startDspTime) return;


            double songTimeMs = GetSongTimeMs();
            SpawnDueNotes(songTimeMs);
            HandleBeatEffect(songTimeMs);
        }
    }

    private void OnDestroy()
    {
        // DOTween 등의 트윈 정리
        foreach (var note in notes)
        {
            if (note != null)
            {
                Note noteComponent = note.GetComponent<Note>();
                if (noteComponent != null)
                {
                    noteComponent.KillTween();
                }
            }
        }

        // 리스트 정리
        notes.Clear();
        judgeNoteList.Clear();
        notePositions.Clear();

        // 오디오 정리
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 이벤트 구독 해제
        OnEveryBeat = null;
    }
    public void CheckTiming(NoteType noteType)
    {
        if (judgeNoteQueue.Count == 0) return;

        GameObject judgeObject = judgeNoteQueue.Peek();
        if (judgeObject == null) return;

        JudgeNote judgeNote = judgeObject.GetComponent<JudgeNote>();
        if (judgeNote == null || judgeNote.noteVisual == null) return;

        Note targetNote = judgeNote.noteVisual.GetComponent<Note>();
        if (targetNote == null || targetNote.noteType != noteType) return;

        float pos = judgeObject.transform.localPosition.x;
        for (int x = 0; x < timingBoxes.Length; x++)
        {
            if (timingBoxes[x].x <= pos && timingBoxes[x].y >= pos)
            {
                CheckJudgeType((JudgeType)x);
                RemoveFrontNote();
                break;
            }
        }
    }

    void GenerateNote(NoteType noteType, int inBeatOffset)
    {
        if (notePositions.Count == 0) return;
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        Note noteComponent = note.GetComponent<Note>();
        if (noteComponent != null)
        {
            noteComponent.noteType = noteType;
            noteComponent.SetVisualDirection(noteType);
            noteComponent.InitMove(notePositions, intervalTime, maxNoteInScreen, inBeatOffset/((float)intervalTime*1000));
        }
        GameObject judgeNote = Instantiate(judgeNotePrefab, new Vector3(maxNoteInScreen,-8,0), Quaternion.identity);
        note.transform.SetParent(transform);
        
        judgeNote.GetComponent<JudgeNote>().noteVisual = note;
        
        notes.Add(note);
        judgeNoteQueue.Enqueue(judgeNote);
    }
    
    void GenerateNote(NoteType noteType) => GenerateNote(noteType, 0);

    public void RemoveFrontNote()
    {
        if (judgeNoteQueue.Count == 0) return;

        GameObject judgeNote = judgeNoteQueue.Dequeue();
        if (judgeNote != null)
        {
            GameObject visualNote = judgeNote.GetComponent<JudgeNote>().noteVisual;
            if (visualNote != null)
            {
                notes.Remove(visualNote);
                Note note = visualNote.GetComponent<Note>();
                if (note != null) note.KillTween();
                Destroy(visualNote);
            }
            Destroy(judgeNote);
        }
    }
    public void CheckJudgeType(JudgeType judgeType)
    {
        GameManager.Instance.HpCheck(judgeType);
        print(judgeType);
    }

    private void LoadChart()
    {
        if (chart != null) return;

        TextAsset textAsset = Resources.Load<TextAsset>(chartResourceName);
        if (textAsset == null)
        {
            Debug.LogError($"Chart not found: Resources/{chartResourceName}.json");
            return;
        }

        chart = JsonUtility.FromJson<NoteChart>(textAsset.text);
        if (chart != null && chart.bpm > 0f)
        {
            bpm = Mathf.RoundToInt(chart.bpm);
        }
    }

    private void SpawnDueNotes(double currentMs)
    {
        if (chart == null || chart.events == null || chart.events.Count == 0) return;

        if (debugFourBeat)
        {
            while (nextFourBeatIndex < chart.events.Count)
            {
                NoteEvent evt = chart.events[nextFourBeatIndex];
                double logTimeMs = evt.timeMs - fourBeatMs;
                if (currentMs < logTimeMs) break;

                Debug.Log($"[4Beat] Incoming {((NoteType)evt.action)}");
                nextFourBeatIndex++;
            }
        }

        while (nextEventIndex < chart.events.Count)
        {
            NoteEvent evt = chart.events[nextEventIndex];
            double spawnTimeMs = evt.timeMs - leadTimeMs;
            if (currentMs < spawnTimeMs) break;

            if (debugLeadBeat)
            {
                double remainingMs = evt.timeMs - currentMs;
                Debug.Log($"[LeadBeat] Spawn {((NoteType)evt.action)} | remaining {remainingMs:F0}ms");
            }
            GenerateNote((NoteType)evt.action, ((int)(currentMs - chart.snapOffsetMs) % (int)(intervalTime * 1000)));
            nextEventIndex++;
        }
    }

    private double GetSongTimeMs()
    {
        if (isPlaybackScheduled)
        {
            return (AudioSettings.dspTime - startDspTime) * 1000.0;
        }

        return currentTime * 1000.0;
    }

    private void HandleBeatEffect(double songTimeMs)
    {
        if (chart == null) return;

        double beatTimeMs = songTimeMs - chart.snapOffsetMs;
        if (beatTimeMs < 0) return;

        double beatIntervalMs = intervalTime * 1000.0;
        int beatIndex = (int)Math.Floor(beatTimeMs / beatIntervalMs);
        if (beatIndex > lastBeatIndex)
        {
            lastBeatIndex = beatIndex;
            UIManager.Instance.CallBeatEffect();
            OnEveryBeat?.Invoke();
            currentTime -= intervalTime;
        }
    }

    private void SchedulePlayback()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is missing.");
            return;
        }

        audioSource.Stop();
        startDspTime = AudioSettings.dspTime + startDelaySec;
        
        Debug.Log(startDspTime);
        
        audioSource.PlayScheduled(startDspTime);
        isPlaybackScheduled = true;
    }
}

[Serializable]
public class NoteEvent
{
    public int timeMs;
    public int action;
}

[Serializable]
public class NoteChart
{
    public string songId;
    public int version;
    public float bpm;
    public int snapOffsetMs;
    public List<NoteEvent> events;
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
