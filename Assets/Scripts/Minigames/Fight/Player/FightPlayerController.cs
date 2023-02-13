using System;
using System.Collections.Generic;
using UnityEngine;

public class FightPlayerController : StateMachine<FighterState>
{
    public GameObject animationPrefabObject;
    public GameObject picturePrefabObject;
    public FightOpponentController directOpponent;

    public int frameTriggerIntent = 3;

    public void Setup(FightPlayerInfo playerInfo, string wonScript)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(playerInfo.Position);

        base.AddState(FighterState.Idle, new FightPlayerIdleState(this, playerInfo));
        base.AddState(FighterState.Dodging, new FightPlayerDodgingState(this, playerInfo));
        base.AddState(FighterState.Attacking, new FightPlayerAttackingState(this, playerInfo));
        base.AddState(FighterState.TakingDamage, new FightPlayerTakingDamageState(this, playerInfo));
        base.AddState(FighterState.KnockedOut, new FightPlayerKnockedOutState(this, playerInfo, wonScript));
    }

    protected override FighterState GetInitialState()
    {
        return FighterState.Idle;
    }

    private void OnJabPressed()
    {
        GiveData(FightPunchType.Jab);
    }

    private void OnPunchPressed()
    {
        GiveData(FightPunchType.Punch);
    }

    private void OnStrongPunchPressed()
    {
        GiveData(FightPunchType.StrongPunch);
    }

    private void OnDodgePressed()
    {
        GiveData(FightPunchType.Dodging);
    }

    private void OnContinueRequested()
    {
        FightEndIntent endIntent = new FightEndIntent();

        directOpponent.GiveDataFrom(this, endIntent);
        GiveData(endIntent);
    }
}
