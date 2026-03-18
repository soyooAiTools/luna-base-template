using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using GameManager.Event;
using UnityEngine;

internal partial class EventPool //where T : EventArgs  Luna不支持这种继承方式，这里就先注释掉了，自己注意一点就行
    {
        private Queue<GameManager.Event.EventPool.Event> m_Event; //执行队列
        private Dictionary<int, List<EventHandler>> m_Subscribe;

        public EventPool()
        {
            m_Event = new Queue<GameManager.Event.EventPool.Event>();
            m_Subscribe = new Dictionary<int, List<EventHandler>>();
        }
        
        public void Update(float elapseSeconds,float realElapseSeconds)
        {
            lock (m_Event)
            {
                while (m_Event.Count > 0)
                {
                    GameManager.Event.EventPool.Event eventmode =  m_Event.Dequeue();
                    HandlerEvent(eventmode.Sender, eventmode.EventArgs);
                }
            }
        }

        public void Subscribe(int Id,EventHandler handler)
        {
            if (m_Subscribe.ContainsKey(Id)) //如果当前委托字典中已经存在这个ID
            {
                if (m_Subscribe[Id].Contains(handler))
                {
                    Debug.LogWarning("当前已经有此事件处理器");
                }
                else
                {
                    m_Subscribe[Id].Add(handler);
                }
            }
            else
            {
                m_Subscribe.Add(Id,new List<EventHandler>());
                m_Subscribe[Id].Add(handler);
            }
            
        }

        public void UnSubscribe(int Id, EventHandler handler)
        {
            if (m_Subscribe.ContainsKey(Id)) //如果当前委托字典中已经存在这个ID
            {
                if (m_Subscribe[Id].Contains(handler))
                {
                    m_Subscribe[Id].Remove(handler);
                }
                else
                {
                    Debug.LogWarning("当前没有注册这个事件处理器");
                }
            }
            else
            {
                Debug.LogWarning("当前没有注册这个事件处理器");
            }
        }

        //下一帧触发，线程安全
        public void Fire(object sender,GameEventArgs args)
        {
            if (args == null)
            {
                Debug.LogWarning("发送的事件为空");
            }
            else
            {
                GameManager.Event.EventPool.Event eventNode = GameManager.Event.EventPool.Event.Create(sender,args);
                lock (m_Event)
                {
                    m_Event.Enqueue(eventNode);
                }
            }
        }

        //立即触发，线程不安全哦
        public void FireNow(object sender,GameEventArgs args)
        {
            if (args == null)
            {
                Debug.LogError("发送的事件为空");
            }
            else
            {
                HandlerEvent(sender,args);
            }
        }

        private void HandlerEvent(object sender, GameEventArgs baseEventArgs)
        {
            int EventId = baseEventArgs.Id;
            if (m_Subscribe.ContainsKey(EventId)) //如果当前ID存在事件处理器
            {
                var HandlerList = m_Subscribe[EventId];
                foreach (var handler in HandlerList)
                {
                    handler(sender,baseEventArgs);
                }
            }
            else //如果当前不存在处理器
            {
                Debug.LogError($"当前发送的事件不存在处理器 : {baseEventArgs.GetType().FullName}");
            }
        }
    }