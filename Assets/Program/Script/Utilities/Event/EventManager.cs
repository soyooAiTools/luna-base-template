using System;
using System.Collections;
using System.Collections.Generic;
using GameManager.Event;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private EventPool _eventPool;

    private bool IsInit = false;
    

    public void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        _eventPool = new EventPool();
        IsInit = true;
    }

    private void Update()
    {
        if (IsInit)
        {
            _eventPool.Update(Time.time,Time.unscaledTime);
        }
    }

    public void Subscribe(int Id, EventHandler eventHandler)
    {
        _eventPool.Subscribe(Id,eventHandler);
    }

    public void UnSubscribe(int Id,EventHandler eventHandler)
    {
        _eventPool.UnSubscribe(Id,eventHandler);
    }

    public void Fire(object sender,GameEventArgs ea)
    {
        _eventPool.Fire(sender,ea);
    }

    public void FireNow(object sender,GameEventArgs ea)
    {
        _eventPool.FireNow(sender,ea);
    }
    
}
