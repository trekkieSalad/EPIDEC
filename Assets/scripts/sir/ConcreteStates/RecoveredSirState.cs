using UnityEngine;

public class RecoveredSirState : SirState
{
    public RecoveredSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(citizen, SirStateColor.Blue);
        Type = StateType.Recovered;
    }

    protected override void CalculateNextState()
    {
        _nextState = StateType.Susceptible;
        SetTimeToStateUpdate(_worldParameters.recoveredDays);
    }

    protected override void ChangeState()
    {
        _citizen.ActualState = new SusceptibleSirState(_citizen);
    }
}