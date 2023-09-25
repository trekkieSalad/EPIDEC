using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class SIRDataRecorder : DataRecorder
{
    public SIRDataRecorder(string dataFileName, List<Citizen> Citizens) : base(dataFileName, Citizens)
    {
        csvContent = "Day; Susceptible; Exposed; Infected; Hospitalized; ICU; Dead; Recovered\n";
    }

    public override void AddCsvLine(int currentIteration)
    {
        int day = currentIteration;
        int susceptible = Citizens.Where(x => x.ActualState.Type == StateType.Susceptible).Count();
        int exposed = Citizens.Where(x => x.ActualState.Type == StateType.Exposed).Count();
        int infected = Citizens.Where(x => x.ActualState.Type == StateType.Infected).Count();
        int hospitalized = Citizens.Where(x => x.ActualState.Type == StateType.Hospitalized).Count();
        int icu = Citizens.Where(x => x.ActualState.Type == StateType.ICU).Count();
        int dead = initialPopulation - Citizens.Count;
        int recovered = Citizens.Where(x => x.ActualState.Type == StateType.Recovered).Count();

        csvContent += day + "; " + susceptible + "; " + exposed + "; " + infected + "; " + hospitalized + "; " + icu + "; " + dead + "; " + recovered + "\n";
    }
}
