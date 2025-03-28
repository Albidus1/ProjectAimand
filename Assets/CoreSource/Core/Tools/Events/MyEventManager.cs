using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;



public interface IEventListenerBase { }
public interface IEventListener<T> : IEventListenerBase
{
    void OnEvent(T _eventType);
}

public struct GameEvent
{
    static GameEvent e;
    public string eventName;

    public GameEvent(string _newName)
    {
        eventName = _newName;
    }

    public static void Trigger(string _newName)
    {
        e.eventName = _newName;
        EventManager.TriggerEvent(e);
    }
}

[ExecuteAlways]
public static class EventManager
{
    private static Dictionary<Type, List<IEventListenerBase>> subscribersList;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeStatics()
    {
        subscribersList = new Dictionary<Type, List<IEventListenerBase>>();
    }

    static EventManager()
    {
        subscribersList = new Dictionary<Type, List<IEventListenerBase>>();
    }

    public static void AddListener<Event>(IEventListener<Event> _listener) where Event : struct
    {
        Type eventType = typeof(Event);

        if (false == subscribersList.ContainsKey(eventType))
        {
            subscribersList[eventType] = new List<IEventListenerBase>();
        }

        if (false == SubscriptionExits(eventType, _listener))
        {
            subscribersList[eventType].Add(_listener);
        }
    }

    public static void RemoveListener<Event>(IEventListener<Event> _listener) where Event : struct
    {
        Type eventType = typeof(Event);

        if (false == subscribersList.ContainsKey(eventType))
        {
            return;
        }

        List<IEventListenerBase> subsList = subscribersList[eventType];

        for(int i = subsList.Count - 1; i >= 0; i--)
        {
            if (subsList[i] == _listener)
            {
                subsList.Remove(subsList[i]);

                if (subsList.Count == 0)
                {
                    subscribersList.Remove(eventType);
                }

                return;
            }
        }
    }

    public static void TriggerEvent<Event>(Event _newEvent) where Event : struct
    {
        List<IEventListenerBase> subsList;

        if (false == subscribersList.TryGetValue(typeof(Event), out subsList))
        {
            return;
        }

        for (int i = subsList.Count - 1; i >= 0; i--)
        {
            (subsList[i] as IEventListener<Event>).OnEvent(_newEvent);
        }
    }

    public static bool SubscriptionExits(Type _type, IEventListenerBase _receiver)
    {
        List<IEventListenerBase> receivers;

        if (false == subscribersList.TryGetValue(_type, out receivers))
        {
            return false;
        }

        bool exits = false;

        for (int i = receivers.Count - 1; i >= 0; i--)
        {
            if (receivers[i] == _receiver)
            {
                exits = true;
                break;
            }
        }
        
        return exits;
    }
}

public static class EventRegister
{
    public delegate void Delegate<T> (T eventType);

    public static void EventStartListening<EventType>(this IEventListener<EventType> _caller) where EventType : struct
    {
        EventManager.AddListener<EventType>(_caller);
    }

    public static void EventStopListening<EventType>(this IEventListener<EventType> _caller) where EventType : struct
    {
        EventManager.RemoveListener<EventType>(_caller);
    }
}