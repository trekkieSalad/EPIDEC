using UnityEngine;

public class InfectedSirState : SirState
{

    private int daysToQuarantine;

    public InfectedSirState(Citizen citizen) : base(citizen)
    {
        Utils.ChangeColor(_citizen, SirStateColor.Red);
        Type = StateType.Infected;
        daysToQuarantine = citizen.CurrentTick + 3;
    }

    protected override void CalculateNextState()
    {
        double probability = Random.value;

        if (probability < _worldParameters.pId)
        {
            _nextState = StateType.Dead;
            SetTimeToStateUpdate(_worldParameters.infectiousDaysToDead);
        }
        else if (probability < _worldParameters.pIh)
        {
            _nextState = StateType.Hospitalized;
            SetTimeToStateUpdate(_worldParameters.infectiousDaysToHospitalized);
        }
        else
        {
            _nextState = StateType.Recovered;
            SetTimeToStateUpdate(_worldParameters.infectiousDaysToRecovered);
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (!_citizen.Asintomatic && _citizen.CurrentTick == daysToQuarantine)
        {
            _citizen.Quarantine = true;
        }
    }

    protected override void ChangeState()
    {
        if (_nextState.Equals(StateType.Dead))
            _citizen.ActualState = new DeadSirState(_citizen);
        else if (_nextState.Equals(StateType.Hospitalized))
            _citizen.ActualState = new HospitalizedSirState(_citizen);
        else
            _citizen.ActualState = new RecoveredSirState(_citizen);
    }
}