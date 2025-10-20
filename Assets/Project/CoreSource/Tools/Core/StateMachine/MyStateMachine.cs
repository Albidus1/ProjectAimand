using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IStateMachine
{
    bool TriggerEvents { get; set; }
}

public struct StateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
{
    public GameObject target;
    public MyStateMachine<T> targetStateMachine;
    public T newState;
    public T previousState;

    public StateChangeEvent(MyStateMachine<T> _stateMachine)
    {
        target = _stateMachine.target;
        targetStateMachine = _stateMachine;
        newState = _stateMachine.currentState;
        previousState = _stateMachine.previousState;
    }
}


public class MyStateMachine<T> : IStateMachine where T : struct, IComparable, IConvertible, IFormattable
{
    public virtual bool TriggerEvents { get; set; }
    public GameObject target;
    public virtual T currentState { get; protected set; }
    public virtual T previousState { get; protected set; }

    public delegate void OnStateChangeDelegate();
    public OnStateChangeDelegate OnStateChange;

    public MyStateMachine(GameObject _target, bool _triggerEvents)
    {
        target = _target;
        TriggerEvents = _triggerEvents;
    }

    public virtual void StateChange(T _newState)
    {
        // 새 상태가 현재 상태와 같을 경우 종료
        if (EqualityComparer<T>.Default.Equals(_newState, currentState))
        {
            return;
        }

        previousState = currentState;
        currentState = _newState;

        OnStateChange?.Invoke();

        if (true == TriggerEvents)
        {
            EventManager.TriggerEvent(new StateChangeEvent<T> (this));
        }
    }

    public virtual void RestorePreviousState()
    {
        // 이전 상태 복구
        currentState = previousState;

        OnStateChange?.Invoke();

        if (true == TriggerEvents)
        {
            EventManager.TriggerEvent(new StateChangeEvent<T>(this));
        }
    }
}
