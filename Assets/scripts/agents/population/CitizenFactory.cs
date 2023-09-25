using UnityEngine;
using System.Collections.Generic;

public interface CitizenFactory
{
    public List<Citizen> createPopulation(
        GameObject citizenObject, int numCitizens = -1);
}

