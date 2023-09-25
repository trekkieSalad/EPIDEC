using UnityEngine;

public class ICUSirState : SirState
{
    public ICUSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(citizen, SirStateColor.Brown);
        Type = StateType.ICU;
    }

    protected override void CalculateNextState()
    {
        double probability = Random.value;

        if (probability < _worldParameters.pIcud)
        {
            _nextState = StateType.Dead;
            SetTimeToStateUpdate(_worldParameters.icuDaysToDead);
        }
        else
        {
            _nextState = StateType.Recovered;
            SetTimeToStateUpdate(_worldParameters.icuDaysToRecovered);
        }
    }

    protected override void ChangeState()
    {
        if (_nextState.Equals(StateType.Dead))
            _citizen.ActualState = new DeadSirState(_citizen);
        else
            _citizen.ActualState = new RecoveredSirState(_citizen);
    }
}