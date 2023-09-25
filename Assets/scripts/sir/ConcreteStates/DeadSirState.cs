using UnityEngine;

public class DeadSirState : SirState
{
    public DeadSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(_citizen, SirStateColor.Grey);
        Debug.Log("Dead " + _citizen.name);
        Type = StateType.Dead;
        _citizen.Dead();
    }

    protected override void CalculateNextState()
    {
        return;
    }

    protected override void ChangeState()
    {
        return;
    }
}