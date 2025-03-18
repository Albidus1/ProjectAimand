using System;
using System.Collections.Generic;
using UnityEngine;



namespace MyCustoms.Tools
{
    public struct GameEvent
    {
        private static GameEvent e;
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
        }

        public static void TriggerEvent<Event>(Event _newEvent) where Event : struct
        {

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


    public interface IEventListenerBase { }
    public interface IEventListener<T> : IEventListenerBase
    {
        void OnEvent(T _eventType);
    }
}