using System;
using System.Collections.Generic;

public interface IState
{
    public void Enter();
    public void Update();
    public void Leave();
    public void ReceiveData(IStateMachine sender, object data);
};