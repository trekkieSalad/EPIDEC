using System.Collections.Generic;
using UnityEngine;

public class TwitterFactory : CriticalNodeFactory
{
    public CriticalNode CreateCriticalNode(List<Citizen> citizens)
    {
        WorldParameters worldParameters = WorldParameters.GetInstance();
        double pTwitter = worldParameters.twitterPercentage;
        // creamos el nodo con un objeto vacio y ke asignamos el componente twitter
        GameObject twitterGameObject = new GameObject("Twitter");
        Twitter twitter = twitterGameObject.AddComponent<Twitter>();
        // cogemos pTwitter ciudadanos al azar
        int nCitizens = citizens.Count;
        int nTwitterCitizens = (int)(nCitizens * pTwitter);
        for (int i = 0; i < nTwitterCitizens; i++)
        {
            int j = Random.Range(i, nCitizens);
            Citizen temp = citizens[i];
            citizens[i] = citizens[j];
            citizens[j] = temp;
        }
        // asignamos los ciudadanos al nodo
        twitter.Users = citizens.GetRange(0, nTwitterCitizens);
        foreach (Citizen citizen in twitter.Users)
        {
            citizen.IsTwitterUser = true;
        }
        return twitter;
    }
}