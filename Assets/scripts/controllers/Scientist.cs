using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class Scientist : MonoBehaviour 
{
    [Header("Simulation")]
    public WorldController World;
    public MapGenerator Map;

    [Header("Agents")]
    public bool CreateSimulatedCitizens;
    public bool CreateCriticalNodes;
    public PopulationDensity DensityOfAgents;
    public GameObject CitizenObject;
    public GameObject NodeObject;

    private string times = "";
    DateTime start;

    [SerializeField] private List<Citizen> Citizens = new List<Citizen>();
    [SerializeField] private List<CriticalNode> CriticalNodes = new List<CriticalNode>();

    void Start()
    {
        
        ProgramController pc = GameObject.Find("ProgramController").GetComponent<ProgramController>();
        Debug.Log("Scientist is creating the world");

        // Habilitamos el script del controlador de la simulacion
        World.enabled = true;
        Debug.Log("World controller enabled");

        // Creamos los ciudadanos
        start = DateTime.Now;
        DateTime starTask = start;
        createCitizens();
        times += "Create citizens: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        // Creamos los nodos criticos
        starTask = DateTime.Now;
        createCriticalNodes();
        times += "Create critical nodes: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        //Creamos el mapa
        starTask = DateTime.Now;
        Map.CreateCitySections();
        times += "Create city sections: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        // Distribuimos los ciudadanos en el mapa
        starTask = DateTime.Now;
        Map.LocateCitizens(Citizens);
        times += "Locate citizens: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        // Creamos los lugares
        starTask = DateTime.Now;
        World.SetPlaces(Map.CreateRelevantPlaces());
        times += "Create places: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        // Diferenciamos los agentes en ciudadanos y nodos criticos
        starTask = DateTime.Now;
        World.SetCitizensAndCriticalNodes();
        times += "Set citizens and critical nodes: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        World.CreateRecorders();

    }

    void Update()
    {
        DateTime starTask = DateTime.Now;
        World.createSocialNetworks();
        times += "Create social networks: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";
        starTask = DateTime.Now;
        World.createSocialCircle();
        times += "Create social circles: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";
        starTask = DateTime.Now;
        World.SetPlacesToMove();
        times += "Set places to move: " + (DateTime.Now - starTask).TotalSeconds + " seconds\n";

        times += "Total time: " + (DateTime.Now - start).TotalSeconds + " seconds\n";

        File.WriteAllText("times.txt", times);

        foreach (Citizen citizen in Citizens)
        {
            citizen.UpdateCitizen(true);
        }

        this.enabled = false;
        //Destroy(gameObject);

    }

    private void createCitizens()
    {

        // Creamos los ciudadanos reales
        CitizenFactory citizenFactory = new RealCitizenFactory(World);
        Citizens.AddRange(citizenFactory.createPopulation(CitizenObject));

        // Creamos los ciudadanos simulados
        if(CreateSimulatedCitizens){
            citizenFactory =  new SimulatedCitizenFactory(World, Citizens);
            Citizens.AddRange(citizenFactory.createPopulation(CitizenObject));
        }

        SetInitialState();
    }

    private void createCriticalNodes()
    {
        if (CreateCriticalNodes)
        {
            CriticalNodeFactory criticalNodeFactory = new TwitterFactory();
            CriticalNode criticalNode = criticalNodeFactory.CreateCriticalNode(Citizens);
            CriticalNodes.Add(criticalNode);
        }     
    }

    private void SetInitialState()
    {
        List<Citizen> susceptibleCitizens = new List<Citizen>(Citizens);
        int initialInfected = WorldParameters.GetInstance().initialInfected;

        for (int i = 0; i < initialInfected; i++)
        {
            int index = UnityEngine.Random.Range(0, susceptibleCitizens.Count);
            Citizen citizen = susceptibleCitizens[index];
            citizen.ActualState = new InfectedSirState(citizen);
            susceptibleCitizens.RemoveAt(index);
        }

        foreach (Citizen citizen in susceptibleCitizens)
        {
            citizen.ActualState = new SusceptibleSirState(citizen);
        }

    }

    
}
