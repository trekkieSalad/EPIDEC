
using UnityEngine;

public class SusceptibleSirState : SirState
{
    public SusceptibleSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(_citizen, SirStateColor.Green);
        Type = StateType.Susceptible;
    }

    protected override void CalculateNextState()
    {
        _nextState = StateType.Exposed;
        SetTimeToStateUpdate(0);
    }

    protected override void ChangeState()
    {
        _citizen.ActualState = new ExposedSirState(_citizen);
    }

    public override void Expose()
    {
        ChangeState();
    }
}
