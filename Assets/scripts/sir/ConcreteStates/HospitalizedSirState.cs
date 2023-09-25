using UnityEngine;

public class HospitalizedSirState : SirState
{
    public HospitalizedSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(citizen, SirStateColor.Violet);
        Type = StateType.Hospitalized;
    }

    protected override void CalculateNextState()
    {
        double probability = Random.value;

        if (probability < _worldParameters.pHd)
        {
            _nextState = StateType.Dead;
            SetTimeToStateUpdate(_worldParameters.hospitalizedDaysToDead);
        }
        else if (probability < _worldParameters.pHicu)
        {
            _nextState = StateType.ICU;
            SetTimeToStateUpdate(_worldParameters.hospitalizedDaysToIcu);
        }
        else
        {
            _nextState = StateType.Recovered;
            SetTimeToStateUpdate(_worldParameters.hospitalizedDaysToRecovered);
        }
    }

    protected override void ChangeState()
    {
        if (_nextState.Equals(StateType.Dead))
            _citizen.ActualState = new DeadSirState(_citizen);
        else if (_nextState.Equals(StateType.ICU))
            _citizen.ActualState = new ICUSirState(_citizen);
        else
            _citizen.ActualState = new RecoveredSirState(_citizen);
    }
}