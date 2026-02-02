using System;
using System.Collections;
using UnityEngine;

public class VanishEffect : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Vanish());
    }

    IEnumerator Vanish()
    {
        yield return new WaitForSecondsRealtime(2f);
        Destroy(gameObject);
    }
}
