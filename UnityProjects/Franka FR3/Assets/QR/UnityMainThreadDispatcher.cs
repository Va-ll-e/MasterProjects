using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private static readonly Queue<Action> queue = new Queue<Action>();

    public static void EnsureCreated()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
    }

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            EnsureCreated();
            return instance;
        }
    }

    void Update()
    {
        lock (queue)
        {
            while (queue.Count > 0)
                queue.Dequeue().Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        lock (queue)
            queue.Enqueue(action);
    }
}