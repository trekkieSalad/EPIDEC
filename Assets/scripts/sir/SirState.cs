using System.Collections;
using System.Collections.Generic;

using Unity.IO.LowLevel.Unsafe;

using UnityEngine;

public enum StateType
{
    Susceptible,
    Exposed,
    Infected,
    Hospitalized,
    ICU,
    Dead,
    Recovered,
}

[System.Serializable]
public abstract class SirState
{
    protected Citizen _citizen;
    public StateType Type;
    protected StateType _nextState;
    protected WorldParameters _worldParameters;
    protected int _timeToStateUpdate;

    public SirState(Citizen citizen)
    {
        this._citizen = citizen;
        this._worldParameters = WorldParameters.GetInstance();
        CalculateNextState();
    }

    protected abstract void CalculateNextState();
    protected abstract void ChangeState();

    public virtual void UpdateState()
    {
        if (_timeToStateUpdate == _citizen.CurrentTick)
        {
            ChangeState();
        }
    }

    public virtual void Expose()
    {
        return;
    }

    protected void SetTimeToStateUpdate(int timeToStateUpdate)
    {
        this._timeToStateUpdate = _citizen.CurrentTick + timeToStateUpdate;
    }


}
