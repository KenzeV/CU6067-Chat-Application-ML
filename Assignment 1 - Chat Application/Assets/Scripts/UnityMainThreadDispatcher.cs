using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{

    private static readonly Queue<Action> _executionQueue = new();
    //Runs queued tasks on unitys main thread each frame
    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
    //Queues coroutine to start on unitys main thread
    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }
    //wraps an action as a coroutine and adds it to queue
    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }
    //Queues an action and returns a task that reports completion or error
    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        void WrappedAction()
        {
            try
            {
                action();
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }
        Enqueue(ActionWrapper(WrappedAction));
        return tcs.Task;
    }
    //Wraps an action in a coroutine for the queue to run
    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
    private static UnityMainThreadDispatcher _instance = null;
    //checks if dispatcher instance is registered
    public static bool Exists()
    {
        return _instance != null;
    }
    //Returns registered dispatcher or throws error if missing
    public static UnityMainThreadDispatcher Instance()
    {
        if (!Exists())
        {
            throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
        }
        return _instance;
    }
    //registers dispatcher if none exists and retains is game object when scene changes
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    //Clears shared dispatcher reference when this component is destroyed
    void OnDestroy()
    {
        _instance = null;
    }
}