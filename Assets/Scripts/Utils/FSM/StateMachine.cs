using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> : MonoBehaviour, IStateMachine where T : IConvertible
{
    private Dictionary<T, IState> stateDictionary = new Dictionary<T, IState>();
    private IState currentState;
    private T currentStateIdentifier;

    public T CurrentState => currentStateIdentifier;

    private void Start()
    {
        currentStateIdentifier = GetInitialState();
        currentState = stateDictionary[currentStateIdentifier];
        if (currentState != null)
        {
            currentState.Enter();
        }
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    public void Transition(T stateValue, object sendValue = null, IStateMachine sendFrom = null)
    {
        if (!stateDictionary.TryGetValue(stateValue, out IState stateToTransitionTo)) {
            return;
        }

        if (currentState != null)
        {
            currentState.Leave();
        }

        currentState = stateToTransitionTo;
        currentStateIdentifier = stateValue;

        if (currentState != null)
        {
            currentState.Enter();

            if (sendValue != null)
            {
                currentState.ReceiveData(sendFrom ?? this, sendValue);
            }
        }
    }

    public void GiveData(object data)
    {
        if (currentState != null)
        {
            currentState.ReceiveData(this, data);
        }
    }

    public void GiveDataFrom(IStateMachine otherMachine, object data)
    {
        if (currentState != null)
        {
            currentState.ReceiveData(otherMachine, data);
        }
    }

    protected void AddState(T stateIdentifier, IState state)
    {
        stateDictionary.Add(stateIdentifier, state);
    }

    protected virtual T GetInitialState()
    {
        return default(T);
    }
}
