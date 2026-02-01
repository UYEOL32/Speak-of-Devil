using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindFirstObjectByType(typeof(T));
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 이미 인스턴스가 존재하는데 내가 아니라면, 나는 중복 생성된 것!
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name}의 중복 인스턴스를 파괴합니다.");
            Destroy(gameObject); 
            return;
        }
        _instance = this as T;
    }
    public static bool HasInstance => _instance != null;
}


public class SingletonPersistence<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        // 1. 중복 체크: 이미 할당된 인스턴스가 내가 아니라면 자신을 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 인스턴스 할당 및 유지 설정
        // (주의: Instance 호출 시 이미 할당되므로 여기서는 DontDestroyOnLoad만 처리해도 무방)
        if (transform.parent != null)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

