using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HPVisualController : MonoBehaviour
{
    [SerializeField] GameObject _hpVisualPrefab;
    List<GameObject> hpBars = new List<GameObject>();
    
    public void GenerateHpVisual()
    {
        for (int i = 0; i < GameManager.Instance.maxHp % 5; i++)
        {
            Vector3 delta = new Vector3(0,i*0.1f,0);
            GameObject hpBar = Instantiate(_hpVisualPrefab, transform.position + delta, Quaternion.identity);
            hpBar.transform.SetParent(transform);
            hpBars.Add(hpBar);
        }

        foreach (GameObject hpBar in hpBars)
        {
            // hpBar.DOLocalMoveX(originalPosition.x + 0.2f, 0.1f)
            //     .SetEase(Ease.InOutQuad)
            //     .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
}
