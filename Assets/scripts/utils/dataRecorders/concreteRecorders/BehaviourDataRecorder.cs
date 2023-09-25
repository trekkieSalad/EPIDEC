using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BehaviourDataRecorder : DataRecorder
{
    public BehaviourDataRecorder(string dataFileName, List<Citizen> Citizens) 
        : base(dataFileName, Citizens)
    {
        csvContent = "Day;Accept;Reject;TAccept;TReject;STAccept;STReject\n";
    }

    public override void AddCsvLine(int currentIteration)
    {
        int day = currentIteration;
        List<Citizen> acceptCitizens = 
            Citizens.Where(x => x.behavior == Behavior.Accept).ToList();
        List<Citizen> rejectCitizens = 
            Citizens.Where(x => x.behavior == Behavior.Reject).ToList();
        int accept = acceptCitizens.Count();
        int reject = rejectCitizens.Count();
        int tAccept = acceptCitizens.Where(x => x.IsTwitterUser).Count();
        int tReject = rejectCitizens.Where(x => x.IsTwitterUser).Count();
        int stAccept = accept - tAccept;
        int stReject = reject - tReject;

        csvContent += day + "; " + accept + "; " + reject + "; " + tAccept + 
            "; " + tReject + "; " + stAccept + "; " + stReject + "\n";
    }
}
