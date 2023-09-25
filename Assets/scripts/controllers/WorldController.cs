using UnityEngine;

using ABMU.Core;

using System.Linq;
using TMPro;
using System.Collections.Generic;
using System;
using System.IO;

public class WorldController : AbstractController
{
    private Day _day;
    public Day day => _day;

    [Header("Enviroment")]
    public List<Place> Places;
    public MapGenerator mapGenerator;

    [Header("Agents")]
    public List<Citizen> Citizens = new List<Citizen>();
    public List<CriticalNode> criticalNodes = new List<CriticalNode>();
    private List<DataRecorder> dataRecorders = new List<DataRecorder>();

    private DateTime start, end;
    private TextMeshProUGUI dayText;
    private bool startSimulation = true;

    public void InitTime()
    {
          start = DateTime.Now;
    }

    public void EndTime()
    {
        end = DateTime.Now;
        File.AppendAllText("times.txt", "Simulation time: " + (end - start).TotalSeconds + " seconds\n");
    }


    #region Life Cycle

    public override void Init()
    {
        Debug.Log("Initializing world");
        base.Init();
        dayText = GameObject.Find("DayText").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (startSimulation)
        {
            InitTime();
            foreach (DataRecorder dr in dataRecorders)
                dr.AddCsvLine(currentTick);
            startSimulation = false;
        }
        _day = (Day)(currentTick % 7);
        dayText.text = day.ToString() + "\nDay " + currentTick;

        foreach (Place place in Places) place.EmptyNode();
    }

    void LateUpdate()
    {
        foreach (Place place in Places) place.ReorderUsers();
        Step();

        foreach (DataRecorder dr in dataRecorders)
            dr.AddCsvLine(currentTick);

        if (currentTick == 150)
        {
            EndTime();
            foreach (DataRecorder dr in dataRecorders)
                dr.ExportData();
            Application.Quit();
        }
    }

    #endregion

    #region Social

    public void createSocialNetworks()
    {
        Debug.Log("Creating influence networks for the " + Citizens.Count + " citizens");
        List<Citizen> citizens = new List<Citizen>();
        citizens.AddRange(Citizens);
        foreach (Citizen citizen in Citizens)
        {
            // Creamos las amistades correctamente
            Debug.LogWarning("Creating social network for " + citizen.name + " with " + citizens.Count);
            RelationshipFactory friendshipFactory = new FriendshipFactory(this);
            friendshipFactory.createNetwork(citizen, citizens);
            if (citizen.Friendships.Count < 5) friendshipFactory.createNetwork(citizen, Citizens);
            if (citizen.Friendships.Count >= 5) citizens.Remove(citizen);
        }

    }

    public void createSocialCircle()
    {
        Debug.Log("Creating social circles");
        RelationshipFactory socialCircleFactory = new SocialCircleFactory();
        foreach (Citizen citizen in Citizens)
        {
            socialCircleFactory.createNetwork(citizen, Citizens);
        }
    }

    #endregion

    #region Places

    public void SetPlaces(List<Place> places)
    {
        Places = new List<Place>();
        Places.AddRange(places);
    }

    public void SetPlacesToMove()
    {
        Debug.Log("Setting places to move to the citizens");
        foreach (Citizen citizen in Citizens)
        {
            foreach (PlaceType type in System.Enum.GetValues(typeof(PlaceType)))
            {
                citizen.AddPlace(type, GetNearPlace(citizen, type));
            }
        }
    }

    private Place GetNearPlace(Citizen citizen, PlaceType type)
    {
        /*GameObject[] possibleNodes =
            GameObject.FindGameObjectsWithTag(type.ToString());

        GameObject nearObject = null;
        float distanciaMasCercana = Mathf.Infinity;

        foreach (GameObject objeto in possibleNodes)
        {
            float distancia = Vector3.Distance(objeto.transform.position, citizen.gameObject.transform.position);

            if (distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                nearObject = objeto;
            }
        }
        return nearObject.GetComponent<Place>();
        */
        List<Place> places = Places.Where(p => p.HasType(type)).ToList();
        GameObject[] possibleNodes = places.Select(p => p.gameObject).ToArray();
        if (possibleNodes.Length != places.Count)
            Debug.LogWarning("Places with type " + type + ": " + places.Count + " -> " + possibleNodes.Length);

        return possibleNodes[UnityEngine.Random.Range(0, possibleNodes.Length)].GetComponent<Place>();
    }

    public Place GetRandomPlace(PlaceType type)
    {
        List<Place> places = Places.Where(p => p.types.Contains(type)).ToList();
        return places[UnityEngine.Random.Range(0, places.Count)];
    }

    #endregion

    #region Agents

    public void SetCitizensAndCriticalNodes()
    {
        Debug.Log("Setting citizens and critical nodes");
        foreach (AbstractAgent agent in agents)
        {
            if (agent is Citizen) Citizens.Add((Citizen)agent);
            else if (agent is CriticalNode) criticalNodes.Add((CriticalNode)agent);
        }
    }

    public Citizen GetRandomCitizen()
    {
        return Citizens[UnityEngine.Random.Range(0, Citizens.Count)];
    }

    public void LocateCitizens()
    {
        mapGenerator.LocateCitizens(Citizens);
    }

    #endregion

    #region Record Data

    public void CreateRecorders()
    {
        dataRecorders.Add(new SIRDataRecorder("SIRData.csv", Citizens));
        dataRecorders.Add(new BehaviourDataRecorder("BehaviourData.csv", Citizens));
    }

    #endregion
}
