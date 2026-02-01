using System;
using System.Collections;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject beatEffect;
    private Coroutine beatCoroutine;
    protected override void Awake()
    {
        base.Awake();
        beatEffect.SetActive(false);
    }
    

    public void CallBeatEffect()
    {
        if (beatCoroutine != null)
        {
            StopCoroutine(beatCoroutine); // 기존 코루틴 중단
        }
        beatCoroutine = StartCoroutine(BeatEffect()); // 새로운 코루틴 시작
    }

    private IEnumerator BeatEffect()
    {
        beatEffect.SetActive(true);
        float elapsedTime = 0f;
        float fadedTime = (float)NoteManager.Instance.intervalTime*0.6f;
        var renderer = beatEffect.GetComponent<CanvasRenderer>();

        while (elapsedTime <= fadedTime)
        {
            renderer.SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadedTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderer.SetAlpha(0f);
        beatEffect.SetActive(false);
    }
}
