
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


public class WorldParameters
{

    private static WorldParameters instance;

    // Simulation parameters
    [Header("Simulation Variables")]
    [SerializeField] private double _populationDensity;
    [SerializeField] private int _initialInfected;

    [Header("SIR probabilities")]
    [SerializeField] private double _pSeAA;
    [SerializeField] private double _pSeAB;
    [SerializeField] private double _pSeBB;
    [SerializeField] private double _pId;
    [SerializeField] private double _pIh;
    [SerializeField] private double _pHd;
    [SerializeField] private double _pHicu;
    [SerializeField] private double _pIcud;

    [Header("SIR days between states")]
    [SerializeField] private int _recoveredDays;
    [SerializeField] private int _infectiousDaysToDead;
    [SerializeField] private int _infectiousDaysToHospitalized;
    [SerializeField] private int _infectiousDaysToRecovered;
    [SerializeField] private int _hospitalizedDaysToDead;
    [SerializeField] private int _hospitalizedDaysToIcu;
    [SerializeField] private int _hospitalizedDaysToRecovered;
    [SerializeField] private int _icuDaysToDead;
    [SerializeField] private int _icuDaysToRecovered;

    [Header("Influence Network")]
    [SerializeField] private int _numFriends;
    [SerializeField] private double _randomFriendProb;

    [Header("CriticalNodes")]
    [SerializeField] private double _twitterPercentage;
    [SerializeField] private double _twitterFactor;

    public double populationDensity { get => _populationDensity; }
    public double pSeAA { get => _pSeAA; }
    public double pSeAB { get => _pSeAB; }
    public double pSeBB { get => _pSeBB; }
    public double pId { get => _pId; }
    public double pIh { get => _pIh; }
    public double pHd { get => _pHd; }
    public double pHicu { get => _pHicu; }
    public double pIcud { get => _pIcud; }
    public int recoveredDays { get => _recoveredDays; }
    public int infectiousDaysToDead { get => _infectiousDaysToDead; }
    public int infectiousDaysToHospitalized { get => _infectiousDaysToHospitalized; }
    public int infectiousDaysToRecovered { get => _infectiousDaysToRecovered; }
    public int hospitalizedDaysToDead { get => _hospitalizedDaysToDead; }
    public int hospitalizedDaysToIcu { get => _hospitalizedDaysToIcu; }
    public int hospitalizedDaysToRecovered { get => _hospitalizedDaysToRecovered; }
    public int icuDaysToDead { get => _icuDaysToDead; }
    public int icuDaysToRecovered { get => _icuDaysToRecovered; }
    public int numFriends { get => _numFriends; }
    public double randomFriendProb { get => _randomFriendProb; }
    public int initialInfected { get => _initialInfected; }
    public double twitterPercentage { get => _twitterPercentage; }
    public double twitterFactor { get => _twitterFactor; }


    private WorldParameters(){
        initParameters();
        // find object scientist
        Scientist scientist = GameObject.Find("Scientist").GetComponent<Scientist>();
        _populationDensity = (double)scientist.DensityOfAgents/100;
    }

    public static WorldParameters GetInstance() 
    {
        if (instance == null)
            instance = new WorldParameters();
        return instance;
    }
    
    private void initParameters(){
        Dictionary<string, object> parameters = new Dictionary<string, object>();


        TextAsset textAsset = Resources.Load<TextAsset>("WorldParams");

        string[] lines = textAsset.text.Split('\n');
        lines = lines.Skip(1).ToArray();

        string[] data;
        foreach (string line in lines)
        {
            if (line.Equals("") || line.Equals("\r")) continue;
            string finalLine = line.Replace("\r", "");
            data = Utils.CsvRowData(finalLine, ";");
            parameters.Add(data[0], data[1]);
        }

        _pSeAA = double.Parse(
            parameters["p-se-AA"].ToString(), CultureInfo.InvariantCulture);
        _pSeAB = 
            double.Parse(parameters["p-se-AB"].ToString(), 
            CultureInfo.InvariantCulture);
        _pSeBB = 
            double.Parse(parameters["p-se-BB"].ToString(),
            CultureInfo.InvariantCulture);
        _pId = 
            double.Parse(parameters["p-id"].ToString(),
            CultureInfo.InvariantCulture);
        _pIh = 
            double.Parse(parameters["p-ih"].ToString(),
            CultureInfo.InvariantCulture);
        _pHd = 
            double.Parse(parameters["p-hd"].ToString(),
            CultureInfo.InvariantCulture);
        _pHicu = 
            double.Parse(parameters["p-hicu"].ToString(),
                        CultureInfo.InvariantCulture);
        _pIcud = 
            double.Parse(parameters["p-icud"].ToString(),
                        CultureInfo.InvariantCulture);

        _recoveredDays = int.Parse(parameters["recovered-days"].ToString());
        _infectiousDaysToDead = 
            int.Parse(parameters["infectious-days-to-dead"].ToString());
        _infectiousDaysToHospitalized = 
            int.Parse(parameters["infectious-days-to-hospitalized"].ToString());
        _infectiousDaysToRecovered =
            int.Parse(parameters["infectious-days-to-recovered"].ToString());
        _hospitalizedDaysToDead = 
            int.Parse(parameters["hospitalized-days-to-dead"].ToString());
        _hospitalizedDaysToIcu =
            int.Parse(parameters["hospitalized-days-to-icu"].ToString());
        _hospitalizedDaysToRecovered =
            int.Parse(parameters["hospitalized-days-to-recovered"].ToString());
        _icuDaysToDead = int.Parse(parameters["ICU-days-to-dead"].ToString());
        _icuDaysToRecovered =
            int.Parse(parameters["ICU-days-to-recovered"].ToString());

        _numFriends = int.Parse(parameters["num-friends"].ToString());  

        _randomFriendProb = 
            double.Parse(parameters["random-friend-prob"].ToString(),
                        CultureInfo.InvariantCulture);
            
        _initialInfected = int.Parse(parameters["initial-infected"].ToString());
        _twitterPercentage =
            double.Parse(parameters["twitter-percentage"].ToString(),
                       CultureInfo.InvariantCulture);
        _twitterFactor =
            double.Parse(parameters["twitter-factor"].ToString(),
                       CultureInfo.InvariantCulture);
    }

}