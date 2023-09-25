using ABMU;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using static RealCitizenFactory;
using ABMU.Core;

public class RealCitizenFactory : CitizenFactory
{
    object lockCitizens = new object();
    object lockCounter = new object();
    List<Citizen> citizens = new List<Citizen>();
    int counter = 0;
    AbstractController controller;

    public RealCitizenFactory(AbstractController _controller)
    {
        controller = _controller;
    }

    public List<Citizen> createPopulation(
        GameObject _citizenObject, int numCitizens = -1)
    {

        bool citizenLimit = numCitizens != -1;

        TextAsset textAsset = Resources.Load<TextAsset>("RealPopulation");
        
        string[] lines = textAsset.text.Split('\n');
        lines = lines.Skip(1).ToArray();
        lines = lines.Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

        List<Citizen> population = new List<Citizen>();
        for (int i = 0; i < lines.Length; i++)
        {
            GameObject citizenObject = MonoBehaviour.Instantiate(_citizenObject);
            citizenObject.name = "Citizen " + i;
            citizenObject.tag = "Citizen";
            Citizen citizen = citizenObject.GetComponent<Citizen>();
            citizen.OwnInit(controller);
            population.Add(citizen);
        }


        DateTime start = DateTime.Now;

        Parallel.For(0,lines.Length,(j)=>
        {
            string line = lines[j];
            if (!(line.Equals("") || line.Equals("\r")))
            {
                Citizen citizen = population[j];
                string finalLine = line.Replace("\r", "");

                string[] data = Utils.CsvRowData(finalLine, ";");
                MakeRealPopulation(citizen, Utils.stringArrayToInt(data));
                lock (lockCitizens) citizens.Add(citizen);
                lock (lockCounter) counter++;
            }
        });

        Debug.Log($"Time to create {counter} citizens: {(DateTime.Now - start).TotalSeconds} seconds");

        return citizens;
    }



    private void MakeRealPopulation(Citizen citizen, int[] data)
    {
        citizen.Simulated = false;

        // información sociodemografica
        citizen.Gender = (CitizenGender)data[0];
        citizen.Age = data[1];
        citizen.Family = data[2];
        citizen.RuralHouse = data[3].Equals(1);
        citizen.economicActivity = (EconomicActivity)data[4];
        citizen.EssentialJob = data[5].Equals(1);
        citizen.NetIncome = data[6];

        // necesidades
        citizen.needAImportance = Utils.maxMinNormalize(
            (double)(data[7] + data[41] + data[42]) / 3, 1.0, 5.0, 0.0, 1.0);
        citizen.needBImportance = Utils.maxMinNormalize(
            (double)(data[37] + data[39]) / 2, 1.0, 5.0, 0.0, 1.0);

        citizen.membershipImportance = Utils.randomNormal(0.5, 0.1);

        // satisfacción de necesidades
        // need A
        double needA = (double)(data[8] + data[9] + data[10] + data[11] +
            data[12] + data[13] + data[14] + data[15] + data[16] + data[17] +
            data[18] + data[19] + data[20] + data[21] + data[22] + data[23] +
            data[24] + data[25] + data[26] + data[27] + data[28] + data[29] +
            data[30] + data[31] + data[32] + data[33] + data[34] + data[35]
            ) / 28;
        citizen.needASatisfactionA = Utils.maxMinNormalize(
            needA, 1.0, 5.0, -1.0, 1.0); // modPosCurve??
        citizen.needASatisfactionB = 0 - citizen.needASatisfactionA;

        // need B
        double needB = (double)(data[36] + data[38] + data[40]) / 3;
        citizen.needBSatisfactionA = Utils.maxMinNormalize(
            needB, 1.0, 5.0, -1.0, 1.0); // modPosCurve??
        citizen.needBSatisfactionB = 0 - citizen.needBSatisfactionA;

        // evaluación de las necesidades
        citizen.needAEvaluationA =
            citizen.needASatisfactionA * citizen.needAImportance;
        citizen.needAEvaluationB =
            citizen.needASatisfactionB * citizen.needAImportance;
        citizen.needBEvaluationA =
            citizen.needBSatisfactionA * citizen.needBImportance;
        citizen.needBEvaluationB =
            citizen.needBSatisfactionB * citizen.needBImportance;

        // satisfaccion global
        citizen.satisfactionA =
            (citizen.needAEvaluationA + citizen.needBEvaluationA) / 2;
        citizen.satisfactionB =
            (citizen.needAEvaluationB + citizen.needBEvaluationB) / 2;
        citizen.satisfaction =
            Math.Max(citizen.satisfactionA, citizen.satisfactionB);
        citizen.behavior = citizen.satisfaction.Equals(citizen.satisfactionA) ?
            Behavior.Accept : Behavior.Reject;

        // disonancia
        citizen.dissonanceTolerance =
            Utils.randomNormalTruncated(0.5, 0.14, 0.0, 1.0);

        // confianza
        System.Random random = new System.Random();
        citizen.CityCouncilTrust = random.NextDouble();
        citizen.PoliticalOppositionTrust = random.NextDouble();
        citizen.LocalMediaTrust = random.NextDouble();
        citizen.LocalMediaOppositionTrust = random.NextDouble();
        citizen.TwitterTrust = random.NextDouble();
        citizen.IsTwitterUser = false;

        // SIR
        citizen.Asintomatic = false;
        citizen.Quarantine = false;

        // Seccion
        string[] sectionList = new string[] {"1503009003", "1503010001",
            "1503002007", "1503003007", "1503003017", "1503007031", "1503009002",
            "1503007027", "1503008003", "1503010005", "1503007041", "1503006023",
            "1503005020", "1503005011", "1503001012", "1503001009", "1503009001",
            "1503008004", "1503005003", "1503002021", "1503002023", "1503005012",
            "1503008005", "1503002011", "1503003020", "1503005006", "1503008006",
            "1503002014", "1503003023", "1503007015", "1503004018", "1503002020",
            "1503007006", "1503005004", "1503001002", "1503007009", "1503007036",
            "1503005010", "1503003014", "1503009004", "1503005005", "1503005014",
            "1503004002", "1503002003", "1503007023", "1503007019", "1503005008",
            "1503002017", "1503006017", "1503001004", "1503005002", "1503004006",
            "1503006002", "1503001005", "1503002022", "1503007042", "1503009005",
            "1503002009", "1503004004", "1503004007", "1503005018", "1503008001",
            "1503005019", "1503007007", "1503001001", "1503005022", "1503007033",
            "1503001007", "1503007016", "1503007029", "1503006014", "1503007030",
            "1503004023", "1503007039", "1503007013", "1503007028", "1503007012",
            "1503003002", "1503002016", "1503007043", "1503001003", "1503007018",
            "1503008002", "1503010003", "1503003006", "1503007014", "1503005017",
            "1503002002", "1503007017", "1503004020", "1503002008", "1503004013"};
        int randomIndex = random.Next(sectionList.Length);
        citizen.section = sectionList[randomIndex];

    }
}