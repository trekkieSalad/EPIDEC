using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Place : MonoBehaviour
{
    public List<Citizen> users = new List<Citizen>();
    public List<PlaceType> types;
    public int capacity;

    public void Start()
    {
        capacity = 0;
        foreach (PlaceType pt in types)
        {
            capacity = Mathf.Max(capacity, pt.GetCapacity());
        }
    }

    #region PlaceType

    public void AddType(PlaceType type)
    {
        if ( !types.Contains(type) )
        {
            types.Add(type);
            capacity = Mathf.Max(capacity, type.GetCapacity());
        }
    }

    public bool HasType(PlaceType type)
    {
        return types.Contains(type);
    }

    #endregion

    #region User Management

    public void RegisterCitizen(Citizen citizen)
    {
        users.Add(citizen);
    }

    public void UnregisterCitizen(Citizen citizen)
    {
        users.Remove(citizen);
    }

    public void EmptyNode()
    {
        users.Clear();
    }

    public void ReorderUsers()
    {
        int n = users.Count;
        for (int i = 0; i < n; i++)
        {
            int j = Random.Range(i, n);
            Citizen temp = users[i];
            users[i] = users[j];
            users[j] = temp;
        }
    }

    public List<Citizen> GetSimultaneousUsers(Citizen user)
    {
        int index = users.IndexOf(user);
        int midRange = capacity / 2;
        int initValue = Mathf.Max(0, index - midRange);
        int range = capacity + Mathf.Min(0, index - midRange);
        range = Mathf.Min(range, users.Count - initValue);
        return users.GetRange(initValue, range);
    }

    #endregion
}
