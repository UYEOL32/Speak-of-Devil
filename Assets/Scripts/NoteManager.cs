using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    public AudioClip[] devilWarning;

    [Header("AUDIO SOURCE AND...!!")] public AudioSource audioSource;
    public AudioSource sfxAudioSource;
    public AudioClip[] inputSfxClips = new AudioClip[4];
    public AudioClip goodOrBetterSfx;
    public AudioClip goodOrBetterBreakDownSfx;
    public double startDelaySec = 0.1d;
    public TMP_Text dspTime;
    
    private double currentTime = 0d;
    private readonly List<GameObject> notes = new List<GameObject>();
    public GameObject notePrefab;
    public GameObject judgeNotePrefab;

    [Header("CHART")]
    public string chartResourceName = "Test";
    public int leadBeats = 5;
    public bool debugLeadBeat = false;
    public bool debugFourBeat = false;
    public int debugFourBeatOffsetMs = 0;
    public bool debugEveryFourBeats = false;

    [Header("SONG MAPPING")]
    public bool useSongId = true;
    public string songId = "song1";
    public List<SongEntry> songEntries = new List<SongEntry>();

    [Header("BOSS ANIM")]
    public UnitAnimator bossUnitAnimator;
    public bool debugBossAnim = false;
    public int bossIdleLockBeats = 2;

    [Header("PLAYER ANIM")]
    public UnitAnimator playerUnitAnimator;
    public bool debugPlayerAnim = false;

    [Header("TUTORIAL")]
    public bool isTutorial = false;
    public int tutorialRestBeats = 4;
    public int tutorialSpawnBeats = 4;
    public int tutorialMode = 0;
    public int tutorialMaxMode = 2;
    public int tutorialSuccessesToAdvance = 3;
    public NoteType[] tutorialPattern;

    [Header("HIT TARGETS")]
    public Transform[] hitTargets = new Transform[4];
    
    List<GameObject> judgeNoteList = new List<GameObject>();
    
    private List<Vector3> notePositions = new List<Vector3>();
    private NoteChart chart;
    private int nextEventIndex = 0;
    private int nextFourBeatIndex = 0;
    private int nextSpecialEventIndex = 0;
    private float leadTimeMs;
    private float fourBeatMs;
    private int lastBeatIndex = -1;
    private int lastEveryFourBeatIndex = -1;
    private int lastTutorialBeatIndex = -1;
    private int tutorialPatternIndex = 0;
    private int lastTutorialFourBeatIndex = -1;
    private int lastChartFourBeatIndex = -1;
    private int currentTutorialCycleId = -1;
    private int tutorialSuccessCount = 0;
    private readonly Dictionary<int, TutorialCycleState> tutorialCycles = new Dictionary<int, TutorialCycleState>();
    private int tutorialStartBeat = 0;
    private int nextTutorialWarnBeatIndex = -1;
    private int bossIdleLockRemaining = 0;
    private double startDspTime;
    private bool isPlaybackScheduled = false;
    private readonly Queue<double> pendingStartLogs = new Queue<double>();

    public Action OnEveryBeat;
    public TMP_Text tutorialText;
    public bool chatAvailable;

    public int nextTutorialMode;

    public int[] tutorialDialogIndex;
    public string[] tutorialDialog;

    public int currentDialogIndex = -1;
    
    public void ProceedTutorial()
    {
        if (!chatAvailable) return;

        if (nextTutorialMode < tutorialDialogIndex.Length)
        {
            if (currentDialogIndex < tutorialDialogIndex[nextTutorialMode] - 1)
            {
                currentDialogIndex++;
                tutorialText.text = tutorialDialog[currentDialogIndex];
            }
            else
            {
                bool wasPaused = tutorialMode < 0;
                tutorialMode = nextTutorialMode;
                chatAvailable = false;
                tutorialText.text = "";
                if (wasPaused)
                {
                    ScheduleTutorialStart(4);
                }
            }
        }
        else
        {
            if (currentDialogIndex < tutorialDialog.Length - 1)
            {
                currentDialogIndex++;
                tutorialText.text = tutorialDialog[currentDialogIndex];
            }
            else
            {
                tutorialText.text = "";
                Debug.Log("튜토리얼은 끝!입니다~");
            }
        }
        
    }

    public void Setting()
    {
        if (useSongId)
        {
            ResolveSongById();
        }
        ResetBreakDownMode();
        ClearAllNotes();
        LoadChart();
        ProceedTutorial();
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
        nextSpecialEventIndex = 0;
        lastBeatIndex = -1;
        lastEveryFourBeatIndex = -1;
        lastTutorialBeatIndex = -1;
        tutorialPatternIndex = 0;
        lastTutorialFourBeatIndex = -1;
        lastChartFourBeatIndex = -1;
        currentTutorialCycleId = -1;
        tutorialSuccessCount = 0;
        tutorialCycles.Clear();
        tutorialStartBeat = 0;
        nextTutorialWarnBeatIndex = -1;
        bossIdleLockRemaining = 0;
        pendingStartLogs.Clear();
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

    public void ClearAllNotes()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            GameObject note = notes[i];
            if (note == null) continue;
            Note noteComponent = note.GetComponent<Note>();
            if (noteComponent != null) noteComponent.KillTween();
            Destroy(note);
        }
        notes.Clear();

        while (judgeNoteQueue.Count > 0)
        {
            GameObject judgeNote = judgeNoteQueue.Dequeue();
            if (judgeNote != null)
            {
                Destroy(judgeNote);
            }
        }
    }

    public Canvas breakDownCanvas;
    public RectTransform breakPanel;
    public RectTransform downPanel;

    public bool breakDownMode;

    public Volume volume;

    public void BreakEffect()
    {
        breakDownCanvas.gameObject.SetActive(true);
        breakPanel.anchoredPosition = new Vector2(-1400, 0);
        downPanel.anchoredPosition = new Vector2(1400, 0);
        
        breakPanel.DOAnchorPosX(0, 0.2f, false);

        volume.enabled = true;

        sfxAudioSource.PlayOneShot(breakClip);
        
        volume.profile.TryGet(out Bloom color);
        volume.profile.TryGet(out ChromaticAberration abre);

        color.active = true;
        abre.active = true;
    }

    public void DownEffect()
    {
       downPanel.DOAnchorPosX(0, 0.2f, false);
       
       sfxAudioSource.PlayOneShot(downClip);
       
       volume.profile.TryGet(out ColorAdjustments color);
       volume.profile.TryGet(out Vignette abre);
       volume.profile.TryGet(out LensDistortion dist);

       dist.intensity.overrideState = true;
       DOTween.To(
               () => dist.intensity.value,
               x => dist.intensity.value = x,
               -1f,
               (float)intervalTime
           )
           .SetEase(Ease.InQuart);


       color.active = true;
       abre.active = true;
    }


    public AudioClip breakClip, downClip, outClip;
    public void BreakDownStart()
    {
        volume.profile.TryGet(out Bloom color);
        volume.profile.TryGet(out ChromaticAberration abre);
        volume.profile.TryGet(out ColorAdjustments a);
        volume.profile.TryGet(out Vignette v);
        

        color.active = false;
        abre.active = false;
        a.active = false;
        v.active = false;
        
        breakDownCanvas.gameObject.SetActive(false);
        breakDownMode = true;
        
        
        volume.profile.TryGet(out LensDistortion dist);

        dist.intensity.overrideState = true;
        DOTween.To(
                () => dist.intensity.value,
                x => dist.intensity.value = x,
                0,
                (float)intervalTime
            )
            .SetEase(Ease.OutBack)
            .OnComplete(() => volume.enabled = false);
    }

    public void BreakDownEnd()
    {
        sfxAudioSource.PlayOneShot(outClip);
        breakDownMode = false;
    }

    private void ResetBreakDownMode()
    {
        breakDownMode = false;
        if (breakDownCanvas != null)
        {
            breakDownCanvas.gameObject.SetActive(false);
        }
        if (volume != null)
        {
            volume.enabled = false;
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
            if (isTutorial)
            {
                HandleTutorial(songTimeMs);
            }
            else
            {
                SpawnDueNotes(songTimeMs);
            }
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
        notePositions.Clear();

        // 오디오 정리
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 이벤트 구독 해제
        OnEveryBeat = null;
    }
    public SpriteRenderer judgeEffect;
    public Color[] colors;

    private Transform GetHitTarget(NoteType noteType)
    {
        if (hitTargets == null) return null;
        int index = (int)noteType;
        if (index < 0 || index >= hitTargets.Length) return null;
        return hitTargets[index];
    }

    public void CheckTiming(NoteType noteType)
    {
        if (judgeNoteQueue.Count == 0) return;

        GameObject judgeObject = judgeNoteQueue.Peek();
        if (judgeObject == null) return;

        JudgeNote judgeNote = judgeObject.GetComponent<JudgeNote>();
        if (judgeNote == null || judgeNote.noteVisual == null) return;

        Note targetNote = judgeNote.noteVisual.GetComponent<Note>();
        if (targetNote == null) return;

        if (timingBoxes == null || timingBoxes.Length == 0) return;
        float pos = judgeObject.transform.localPosition.x;
        Vector2 outer = timingBoxes[timingBoxes.Length - 1];
        if (pos < outer.x || pos > outer.y) return;

        if (targetNote.noteType != noteType)
        {
            CheckJudgeType(JudgeType.Miss,noteType);
            RemoveFrontNote(JudgeType.Miss);
            return;
        }

        Transform hitTarget = GetHitTarget(noteType);
        for (int x = 0; x < timingBoxes.Length; x++)
        {
            if (timingBoxes[x].x <= pos && timingBoxes[x].y >= pos)
            {
                
                CheckJudgeType((JudgeType)x, noteType);
                if (hitTarget != null)
                {
                    targetNote.PlayHit(hitTarget);
                    RemoveFrontNote((JudgeType)x, false);
                }
                else
                {
                    RemoveFrontNote((JudgeType)x);
                }
                break;
            }
        }
    }

    void GenerateNote(NoteType noteType, int inBeatOffset, int tutorialCycleId = -1)
    {
        if (notePositions.Count == 0) return;
        GameObject note = Instantiate(notePrefab, notePositions[0], Quaternion.identity);
        Note noteComponent = note.GetComponent<Note>();
        if (noteComponent != null)
        {
            noteComponent.noteType = noteType;
            noteComponent.tutorialCycleId = tutorialCycleId;
            noteComponent.InitMove(notePositions, intervalTime, maxNoteInScreen, inBeatOffset/((float)intervalTime*1000));
        }
        GameObject judgeNote = Instantiate(judgeNotePrefab, new Vector3(maxNoteInScreen,-8,0), Quaternion.identity);
        judgeNote.GetComponent<JudgeNote>().noteType = noteType;
        note.transform.SetParent(transform);
        
        judgeNote.GetComponent<JudgeNote>().noteVisual = note;
        
        notes.Add(note);
        judgeNoteQueue.Enqueue(judgeNote);
    }
    
    void GenerateNote(NoteType noteType) => GenerateNote(noteType, 0);

    public void RemoveFrontNote(JudgeType judgeType, bool destroyVisual = true)
    {
        if (judgeNoteQueue.Count == 0) return;

        GameObject judgeNote = judgeNoteQueue.Dequeue();
        if (judgeNote != null)
        {
            JudgeNote judgeNoteComponent = judgeNote.GetComponent<JudgeNote>();
            if (judgeNoteComponent != null)
            {
                judgeNoteComponent.MarkResolved();
            }
            judgeNote.SetActive(false);
            GameObject visualNote = judgeNoteComponent != null ? judgeNoteComponent.noteVisual : null;
            if (visualNote != null)
            {
                notes.Remove(visualNote);
                Note note = visualNote.GetComponent<Note>();
                if (note != null)
                {
                    HandleTutorialJudge(note, judgeType);
                    if (judgeType == JudgeType.Miss)
                    {
                        note.PlayMiss();
                        destroyVisual = false;
                    }
                    else if (!destroyVisual)
                    {
                        // Visual note handles its own cleanup (ex: hit animation).
                    }
                    else
                    {
                        note.KillTween();
                    }
                }
                if (destroyVisual)
                {
                    Destroy(visualNote);
                }
            }
            Destroy(judgeNote);
        }
    }
    public void CheckJudgeType(JudgeType judgeType, NoteType noteType)
    {
        if (!isTutorial) GameManager.Instance.HpCheck(judgeType);
        if (judgeType <= JudgeType.Good && sfxAudioSource != null && goodOrBetterSfx != null)
        {
            AudioClip clip = breakDownMode && goodOrBetterBreakDownSfx != null
                ? goodOrBetterBreakDownSfx
                : goodOrBetterSfx;
            sfxAudioSource.PlayOneShot(clip);
        }
        
        UIManager.Instance.ChangeJudgeIconAndJudgeText(judgeType, noteType);
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

    private void ResolveSongById()
    {
        if (string.IsNullOrEmpty(songId)) return;
        for (int i = 0; i < songEntries.Count; i++)
        {
            SongEntry entry = songEntries[i];
            if (entry == null || string.IsNullOrEmpty(entry.songId)) continue;
            if (entry.songId != songId) continue;

            if (!string.IsNullOrEmpty(entry.chartResourceName))
            {
                chartResourceName = entry.chartResourceName;
            }
            if (audioSource != null && entry.audioClip != null)
            {
                audioSource.clip = entry.audioClip;
            }

            chart = null;
            return;
        }
    }

    private void SpawnDueNotes(double currentMs)
    {
        if (chart == null || chart.events == null || chart.events.Count == 0) return;

        while (pendingStartLogs.Count > 0)
        {
            if (currentMs < pendingStartLogs.Peek()) break;
            BreakDownStart();
            Debug.Log("[Event] Start");
            pendingStartLogs.Dequeue();
        }

        while (nextSpecialEventIndex < chart.events.Count)
        {
            NoteEvent evt = chart.events[nextSpecialEventIndex];
            if (evt.action < 5)
            {
                nextSpecialEventIndex++;
                continue;
            }
            if (currentMs < evt.timeMs) break;

            switch (evt.action)
            {
                case 5:
                    Debug.Log("[Event] Break!");
                    BreakEffect();
                    break;
                case 6:
                    Debug.Log("[Event] Down");
                    DownEffect();
                    pendingStartLogs.Enqueue(evt.timeMs + (intervalTime * 1000.0));
                    break;
                case 7:
                    Debug.Log("[Event] Out");
                    break;
                default:
                    Debug.Log($"[Event] action={evt.action}");
                    break;
            }
            nextSpecialEventIndex++;
        }

        if (debugFourBeat)
        {
            while (nextFourBeatIndex < chart.events.Count)
            {
                NoteEvent evt = chart.events[nextFourBeatIndex];
                if (evt.action >= 4)
                {
                    nextFourBeatIndex++;
                    continue;
                }
                double logTimeMs = evt.timeMs - (fourBeatMs + debugFourBeatOffsetMs);
                if (currentMs < logTimeMs) break;

                if (sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(devilWarning[evt.action]);
                }
                Debug.Log($"[4Beat][Spawn] Incoming {((NoteType)evt.action)}");
                SetBossAnimationIndex((NoteType)evt.action);
                nextFourBeatIndex++;
            }

            LogChartFourBeat(currentMs);
        }

        while (nextEventIndex < chart.events.Count)
        {
            NoteEvent evt = chart.events[nextEventIndex];
            if (evt.action >= 4)
            {
                nextEventIndex++;
                continue;
            }
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
            if (bossIdleLockRemaining > 0)
            {
                bossIdleLockRemaining--;
            }
            if (debugEveryFourBeats && beatIndex % 4 == 0 && beatIndex != lastEveryFourBeatIndex)
            {
                Debug.Log($"[Every4Beat] beatIndex={beatIndex}");
                lastEveryFourBeatIndex = beatIndex;
            }
            currentTime -= intervalTime;
        }
    }

    private void HandleTutorial(double songTimeMs)
    {
        if (tutorialMode < 0) return;
        if (tutorialRestBeats < 0 || tutorialSpawnBeats <= 0) return;

        int snapOffsetMs = chart != null ? chart.snapOffsetMs : 0;
        double beatTimeMs = songTimeMs - snapOffsetMs;
        if (beatTimeMs < 0) return;

        double beatIntervalMs = intervalTime * 1000.0;
        int globalBeatIndex = (int)Math.Floor(beatTimeMs / beatIntervalMs);
        if (globalBeatIndex < tutorialStartBeat) return;

        int beatIndex = globalBeatIndex - tutorialStartBeat;
        if (beatIndex == lastTutorialBeatIndex) return;

        int cycle = tutorialRestBeats + tutorialSpawnBeats;
        if (cycle <= 0) return;

        int cycleId = beatIndex / cycle;
        if (cycleId != currentTutorialCycleId)
        {
            CloseTutorialCycle(currentTutorialCycleId);
            if (tutorialMode < 0) return;
            currentTutorialCycleId = cycleId;
        }

        lastTutorialBeatIndex = beatIndex;
        if (debugFourBeat)
        {
            LogTutorialFourBeat(beatTimeMs, beatIntervalMs);
        }

        if (!IsTutorialSpawnBeat(beatIndex)) return;

        NoteType type = GetTutorialNoteType(true);
        GenerateNote(type, 0, cycleId);
        RegisterTutorialSpawn(cycleId);
    }

    private bool IsTutorialSpawnBeat(int beatIndex)
    {
        int cycle = tutorialRestBeats + tutorialSpawnBeats;
        if (cycle <= 0) return false;

        int cyclePos = ((beatIndex % cycle) + cycle) % cycle;
        if (tutorialMode <= 0)
        {
            return cyclePos == tutorialRestBeats;
        }

        return cyclePos >= tutorialRestBeats;
    }

    private NoteType GetTutorialNoteType(bool advancePattern)
    {
        if (tutorialPattern == null || tutorialPattern.Length == 0) return NoteType.Up;

        if (tutorialMode == 1)
        {
            return tutorialPattern[0];
        }

        NoteType type = tutorialPattern[tutorialPatternIndex % tutorialPattern.Length];
        if (advancePattern) tutorialPatternIndex++;
        return type;
    }

    private bool TryGetTutorialNoteTypeAtBeat(int beatIndex, out NoteType type)
    {
        type = NoteType.Up;
        int cycle = tutorialRestBeats + tutorialSpawnBeats;
        if (cycle <= 0) return false;

        int cyclePos = ((beatIndex % cycle) + cycle) % cycle;
        if (!IsTutorialSpawnBeat(beatIndex)) return false;

        if (tutorialPattern == null || tutorialPattern.Length == 0)
        {
            type = NoteType.Up;
            return true;
        }

        if (tutorialMode == 1)
        {
            type = tutorialPattern[0];
            return true;
        }

        int cyclesBefore = (beatIndex - cyclePos) / cycle;
        int spawnIndex;

        if (tutorialMode <= 0)
        {
            spawnIndex = cyclesBefore;
        }
        else
        {
            int spawnIndexInCycle = cyclePos - tutorialRestBeats;
            spawnIndex = (cyclesBefore * tutorialSpawnBeats) + spawnIndexInCycle;
        }

        int len = tutorialPattern.Length;
        int index = spawnIndex % len;
        if (index < 0) index += len;
        type = tutorialPattern[index];
        return true;
    }

    private void LogTutorialFourBeat(double beatTimeMs, double beatIntervalMs)
    {
        if (tutorialMode < 0) return;
        if (tutorialRestBeats < 0 || tutorialSpawnBeats <= 0) return;

        SetBossAnimationIndex(null);

        if (nextTutorialWarnBeatIndex < 0)
        {
            nextTutorialWarnBeatIndex = GetFirstTutorialSpawnBeatIndex();
            if (nextTutorialWarnBeatIndex < 0) return;
        }

        double tutorialTimeMs = beatTimeMs - (tutorialStartBeat * beatIntervalMs);
        if (tutorialTimeMs < 0) return;

        while (nextTutorialWarnBeatIndex >= 0)
        {
            double warnTimeMs = (nextTutorialWarnBeatIndex * beatIntervalMs) + (leadTimeMs - fourBeatMs) - debugFourBeatOffsetMs;
            if (tutorialTimeMs < warnTimeMs) break;

            if (TryGetTutorialNoteTypeAtBeat(nextTutorialWarnBeatIndex, out NoteType upcoming))
            {
                Debug.Log($"[4Beat][Tutorial] Incoming {upcoming}");
                if (sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(devilWarning[(int)upcoming]);
                }
                SetBossAnimationIndex(upcoming);
            }

            nextTutorialWarnBeatIndex = GetNextTutorialSpawnBeatIndex(nextTutorialWarnBeatIndex);
        }
    }

    private int GetFirstTutorialSpawnBeatIndex()
    {
        int cycle = tutorialRestBeats + tutorialSpawnBeats;
        if (cycle <= 0) return -1;
        return tutorialRestBeats;
    }

    private int GetNextTutorialSpawnBeatIndex(int currentSpawnBeatIndex)
    {
        int cycle = tutorialRestBeats + tutorialSpawnBeats;
        if (cycle <= 0) return -1;

        if (tutorialMode <= 0)
        {
            return currentSpawnBeatIndex + cycle;
        }

        int cyclePos = ((currentSpawnBeatIndex % cycle) + cycle) % cycle;
        int lastSpawnInCycle = tutorialRestBeats + tutorialSpawnBeats - 1;
        if (cyclePos < lastSpawnInCycle)
        {
            return currentSpawnBeatIndex + 1;
        }

        return currentSpawnBeatIndex + (cycle - cyclePos) + tutorialRestBeats;
    }

    private void RegisterTutorialSpawn(int cycleId)
    {
        if (cycleId < 0) return;

        if (!tutorialCycles.TryGetValue(cycleId, out TutorialCycleState state))
        {
            state = new TutorialCycleState();
            tutorialCycles[cycleId] = state;
        }

        state.expected++;
    }

    private void HandleTutorialJudge(Note note, JudgeType judgeType)
    {
        if (!isTutorial || tutorialMode < 0 || note == null) return;
        int cycleId = note.tutorialCycleId;
        if (cycleId < 0) return;

        if (!tutorialCycles.TryGetValue(cycleId, out TutorialCycleState state)) return;

        if (judgeType == JudgeType.Miss) state.failed = true;
        state.judged++;

        TryFinalizeTutorialCycle(cycleId, state);
    }

    private void CloseTutorialCycle(int cycleId)
    {
        if (cycleId < 0) return;
        if (!tutorialCycles.TryGetValue(cycleId, out TutorialCycleState state)) return;

        state.closed = true;
        TryFinalizeTutorialCycle(cycleId, state);
    }

    private void TryFinalizeTutorialCycle(int cycleId, TutorialCycleState state)
    {
        if (!state.closed) return;
        if (state.judged < state.expected) return;

        if (state.failed)
        {
            tutorialSuccessCount = 0;
        }
        else
        {
            tutorialSuccessCount++;
            if (tutorialSuccessCount >= tutorialSuccessesToAdvance)
            {
                AdvanceTutorialMode();

            }
        }

        tutorialCycles.Remove(cycleId);
    }

    private void AdvanceTutorialMode()
    {
        tutorialSuccessCount = 0;
        tutorialPatternIndex = 0;
        currentTutorialCycleId = -1;
        tutorialCycles.Clear();
        nextTutorialWarnBeatIndex = -1;

        nextTutorialMode = tutorialMode + 1;
        tutorialMode = -1;
        chatAvailable = true;
        ProceedTutorial();
        if (tutorialMode > tutorialMaxMode)
        {
            tutorialMode = -1;
            Debug.Log("튜토리얼 성공");
        }
    }

    private void ScheduleTutorialStart(int beatsDelay)
    {
        int delay = Mathf.Max(0, beatsDelay);
        tutorialStartBeat = GetCurrentGlobalBeat() + delay;
        lastTutorialBeatIndex = -1;
        lastTutorialFourBeatIndex = -1;
        currentTutorialCycleId = -1;
        tutorialCycles.Clear();
        nextTutorialWarnBeatIndex = -1;
    }

    private int GetCurrentGlobalBeat()
    {
        if (intervalTime <= 0) return 0;
        double songTimeMs = GetSongTimeMs();
        int snapOffsetMs = chart != null ? chart.snapOffsetMs : 0;
        double beatTimeMs = songTimeMs - snapOffsetMs;
        if (beatTimeMs < 0) return 0;
        double beatIntervalMs = intervalTime * 1000.0;
        return (int)Math.Floor(beatTimeMs / beatIntervalMs);
    }

    private void LogChartFourBeat(double songTimeMs)
    {
        if (chart == null || chart.events == null || chart.events.Count == 0) return;

        double beatTimeMs = songTimeMs - chart.snapOffsetMs;
        if (beatTimeMs < 0) return;

        double beatIntervalMs = intervalTime * 1000.0;
        int beatIndex = (int)Math.Floor(beatTimeMs / beatIntervalMs);
        if (beatIndex == lastChartFourBeatIndex) return;

        SetBossAnimationIndex(null);

        int warnBeat = beatIndex + 4 - leadBeats;
        double warnTimeMs = (warnBeat * beatIntervalMs) - debugFourBeatOffsetMs;
        double targetEventTimeMs = warnTimeMs + chart.snapOffsetMs;

        int idx = nextEventIndex;
        while (idx < chart.events.Count && chart.events[idx].timeMs < targetEventTimeMs) idx++;
        while (idx < chart.events.Count && chart.events[idx].timeMs == targetEventTimeMs && chart.events[idx].action >= 4) idx++;
        if (idx < chart.events.Count && chart.events[idx].action < 4 && Math.Abs(chart.events[idx].timeMs - targetEventTimeMs) <= 1.0)
        {
            Debug.Log($"[4Beat] Incoming {((NoteType)chart.events[idx].action)}");
            if (sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(devilWarning[chart.events[idx].action]);
            }
            SetBossAnimationIndex((NoteType)chart.events[idx].action);
        }

        lastChartFourBeatIndex = beatIndex;
    }

    private void SetBossAnimationIndex(NoteType? noteType)
    {
        if (bossUnitAnimator == null) return;

        int index = 0;
        if (noteType.HasValue)
        {
            index = ((int)noteType.Value) + 1;
            bossIdleLockRemaining = Mathf.Max(0, bossIdleLockBeats);
        }
        else
        {
            if (bossIdleLockRemaining > 0) return;
        }
        if (debugBossAnim)
        {
            Debug.Log($"[BossAnim] noteType={(noteType.HasValue ? noteType.Value.ToString() : "None")} index={index}");
        }
        bossUnitAnimator.SetAnimationIndex(index);
        if (noteType.HasValue)
        {
            bossUnitAnimator.PlayAnimation();
        }
    }

    public void TriggerPlayerAnimation(NoteType noteType)
    {
        if (GameManager.Instance.gameState != GameState.Playing) return;
        if (playerUnitAnimator == null) return;

        int index = ((int)noteType) + 1;
        if (debugPlayerAnim)
        {
            Debug.Log($"[PlayerAnim] noteType={noteType} index={index}");
        }
        playerUnitAnimator.SetAnimationIndex(index);
        playerUnitAnimator.PlayAnimation();
    }

    public void PlayInputSfx(NoteType noteType)
    {
        if (sfxAudioSource == null) return;
        int index = (int)noteType;
        if (inputSfxClips == null || index < 0 || index >= inputSfxClips.Length) return;
        AudioClip clip = inputSfxClips[index];
        if (clip == null) return;
        sfxAudioSource.PlayOneShot(clip);
    }

    private class TutorialCycleState
    {
        public int expected;
        public int judged;
        public bool failed;
        public bool closed;
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

[Serializable]
public class SongEntry
{
    public string songId;
    public string chartResourceName;
    public AudioClip audioClip;
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
