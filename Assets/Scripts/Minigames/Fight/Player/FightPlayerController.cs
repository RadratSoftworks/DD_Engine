using UnityEngine;
using UnityEngine.InputSystem;

public class FightPlayerController : StateMachine<FighterState>
{
    public GameObject animationPrefabObject;
    public GameObject picturePrefabObject;
    public FightOpponentController directOpponent;

    private PlayerInput controlInput;
    public int frameTriggerIntent = 3;

    private void Awake()
    {
        controlInput = GetComponent<PlayerInput>();
    }

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
        // If either one of us is knocked out, end the fight and disable input
        if ((directOpponent.CurrentState == FighterState.KnockedOut) ||
            (CurrentState == FighterState.KnockedOut))
        {
            FightEndIntent endIntent = new FightEndIntent();

            directOpponent.GiveDataFrom(this, endIntent);
            GiveData(endIntent);

            controlInput.enabled = false;
        }
    }
}
