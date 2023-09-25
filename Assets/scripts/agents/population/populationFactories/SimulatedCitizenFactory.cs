using ABMU;
using UnityEngine;

using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ABMU.Core;

public class SimulatedCitizenFactory : CitizenFactory
{
    static private List<Citizen> realCitizens;
    AbstractController controller;

    public SimulatedCitizenFactory(AbstractController _controller, List<Citizen> _realCitizens)
    {
        realCitizens = _realCitizens;
        controller = _controller;
    }

    public List<Citizen> createPopulation(
        GameObject _citizenObject, int numCitizens = -1)
    {
        Debug.Log("Creating simulated citizens");
        List<Citizen> citizens = new List<Citizen>();
        object lockCitizens = new object();
        bool citizenLimit = numCitizens != -1;
        int counter = realCitizens.Count;

        TextAsset textAsset = Resources.Load<TextAsset>("CensusCoruña");

        string[] lines = textAsset.text.Split('\n');
        lines = lines.Skip(1).ToArray();

        string[] data;
        string t = "";
        DateTime start = DateTime.Now;
        double timew = 0, timep = 0;
        foreach (string line in lines)
        {
            if (line.Equals("") || line.Equals("\r")) continue;
            double reductionFactor = WorldParameters.GetInstance().populationDensity;
            string finalLine = line.Replace("\r", "");
            data = Utils.CsvRowData(finalLine, ";");

            string section = data[0];
            for (int n = 1; n < 5; n++)
            {
                int citizensNumber = (int)(int.Parse(data[n]) * reductionFactor);
                CitizenGender gender = (n < 3) ?
                    CitizenGender.Male : CitizenGender.Female;
                bool old = (n % 2 == 0);
                List<Citizen> population = new List<Citizen>();

                DateTime t1 = DateTime.Now;
                for (int i = 0; i < citizensNumber; i++)
                {
                    GameObject citizenObject =
                        MonoBehaviour.Instantiate(_citizenObject);
                    citizenObject.name = "Citizen " + counter;
                    citizenObject.tag = "Citizen";
                    Citizen citizen = citizenObject.GetComponent<Citizen>();
                    citizen.OwnInit(controller);
                    population.Add(citizen);
                }

                DateTime t2 = DateTime.Now;
                Parallel.For(0, citizensNumber, (i) =>
                {
                    Citizen citizen = population[i];
                    MakeSimulatedPopulation(citizen, section, old, gender);
                    lock(lockCitizens) citizens.Add(citizen);
                    counter++;
                });
                DateTime t3 = DateTime.Now;
                timew += (t2 - t1).TotalSeconds;
                timep += (t3 - t2).TotalSeconds;

            }
        }
        DateTime end = DateTime.Now;
        t += $"Time to create {counter} simulated citizens: {(end - start).TotalSeconds} seconds\n";
        t += $"Time to instantiate {counter} simulated citizens: {timew} seconds\n";
        t += $"Time to populate {counter} simulated citizens: {timep} seconds\n";
        File.WriteAllText("timing.txt", t);
        Debug.Log($"Time to create {counter} simulated citizens: {(DateTime.Now - start).TotalSeconds} seconds");
        return citizens;
    }


    static private void MakeSimulatedPopulation(
        Citizen citizen, string section, bool old, CitizenGender gender)
    {
        System.Random random = new System.Random();
        Dictionary<CitizenGender, double[]> famProb =
            new Dictionary<CitizenGender, double[]>(){
                {CitizenGender.Male, new double[]{0.388, 0.033}},
                {CitizenGender.Female, new double[]{0.354, 0.086}}
            };


        Dictionary<CitizenGender, double[]> netProb =
            new Dictionary<CitizenGender, double[]>(){
                {CitizenGender.Male, new double[]{0.708, 0.9}},
                {CitizenGender.Female, new double[]{0.636, 0.62}}
            };


        Dictionary<CitizenGender, double[]> rurProb =
            new Dictionary<CitizenGender, double[]>(){
                {CitizenGender.Male, new double[]{0.219, 0.167}},
                {CitizenGender.Female, new double[]{0.315, 0.114}}
            };


        Dictionary<CitizenGender, double[]> essentProb =
            new Dictionary<CitizenGender, double[]>(){
                {CitizenGender.Male, new double[]{0.169, 0.1}},
                {CitizenGender.Female, new double[]{0.197, 0.086}}
            };

        Dictionary<CitizenGender, double[]> empProb =
            new Dictionary<CitizenGender, double[]>(){
                {CitizenGender.Male, new double[]{0.284, 0.033}},
                {CitizenGender.Female, new double[]{0.238, 0.0}}
            };

        // Establecemos los atributos del ciudadano en función de su edad y sexo
        citizen.Simulated = true;
        citizen.section = section;
        citizen.Age = old ?
            random.Next(65, 101) :
            random.Next(18, 65);
        citizen.Gender = gender;

        citizen.Family =
            (random.NextDouble() <= famProb[gender][old ? 1 : 0]) ? 4 : 1;
        citizen.NetIncome =
            (random.NextDouble() <= netProb[gender][old ? 1 : 0]) ? 3 : 1;
        citizen.RuralHouse =
            (random.NextDouble() <= rurProb[gender][old ? 1 : 0]) ?
            true : false;
        citizen.EssentialJob =
            (random.NextDouble() <= netProb[gender][old ? 1 : 0]) ?
            true : false;
        citizen.economicActivity =
            (random.NextDouble() <= empProb[gender][old ? 1 : 0]) ?
            EconomicActivity.Employed :
            (EconomicActivity)random.Next(2,8);
        citizen.dissonanceTolerance =
            Utils.randomNormalTruncated(0.5, 0.14, 0, 1);

        // Copiamos las necesidades de un ciudadano real que cumpla las mismas
        // características que el ciudadano simulado
        Citizen toCopy = getCitizenToCopy(citizen);
        citizen.needAImportance = toCopy.needAImportance;
        citizen.needBImportance = toCopy.needBImportance;
        citizen.membershipImportance = toCopy.membershipImportance;

        citizen.needASatisfactionA = toCopy.needASatisfactionA;
        citizen.needBSatisfactionA = toCopy.needBSatisfactionA;
        citizen.needASatisfactionB = toCopy.needASatisfactionB;
        citizen.needBSatisfactionB = toCopy.needBSatisfactionB;

        citizen.needAEvaluationA = toCopy.needAEvaluationA;
        citizen.needBEvaluationA = toCopy.needBEvaluationA;
        citizen.needAEvaluationB = toCopy.needAEvaluationB;
        citizen.needBEvaluationB = toCopy.needBEvaluationB;

        citizen.satisfactionA = toCopy.satisfactionA;
        citizen.satisfactionB = toCopy.satisfactionB;
        citizen.satisfaction =
            Math.Max(citizen.satisfactionA, citizen.satisfactionB);

        citizen.behavior = (citizen.satisfaction == citizen.satisfactionA) ?
            Behavior.Accept : Behavior.Reject;

        // Establecemos el nivel de confianza del ciudadano simulado
        citizen.CityCouncilTrust = random.NextDouble();
        citizen.PoliticalOppositionTrust = random.NextDouble();
        citizen.LocalMediaTrust = random.NextDouble();
        citizen.LocalMediaOppositionTrust = random.NextDouble();
        citizen.TwitterTrust = random.NextDouble();
        citizen.IsTwitterUser = false;

        // Establecemos la situacion epidemiologica del ciudadano simulado
        citizen.Asintomatic = false;
        citizen.Quarantine = false;

    }

    static private Citizen getCitizenToCopy(Citizen citizen)
    {
        List<Citizen> filterRealCitizens = realCitizens;
        if (citizen.Age >= 45)
        {
            filterRealCitizens = 
                filterRealCitizens.Where(a => a.Age >= 45).ToList();
        }
        else if (citizen.Age >= 25)
        {
            if (citizen.RuralHouse)
                filterRealCitizens = filterRealCitizens.Where(a =>
                        a.RuralHouse &&
                        25 <= a.Age &&
                        a.Age < 45
                    ).ToList();
            else if (citizen.EssentialJob)
                filterRealCitizens = filterRealCitizens.Where(a =>
                        !a.RuralHouse &&
                        a.EssentialJob &&
                        25 <= a.Age &&
                        a.Age < 45
                    ).ToList();
            else if (citizen.economicActivity.Equals(EconomicActivity.Employed))
                filterRealCitizens = filterRealCitizens.Where(a =>
                        !a.RuralHouse &&
                        !a.EssentialJob &&
                        25 <= a.Age &&
                        a.Age < 45 &&
                        a.economicActivity.Equals(EconomicActivity.Employed)
                    ).ToList();
            else
                filterRealCitizens = filterRealCitizens.Where(a =>
                        !a.RuralHouse &&
                        !a.EssentialJob &&
                        25 <= a.Age &&
                        a.Age < 45 &&
                        !a.economicActivity.Equals(EconomicActivity.Employed)
                    ).ToList();
        }
        else
        {
            if (citizen.Family < 3)
                filterRealCitizens = filterRealCitizens.Where(a =>
                        a.Age < 25 &&
                        a.Family < 3
                    ).ToList();
            else if (citizen.NetIncome > 2)
                filterRealCitizens = filterRealCitizens.Where(a =>
                        a.NetIncome > 2 &&
                        a.Age < 25 &&
                        a.Family > 2
                    ).ToList();
            else
                filterRealCitizens = filterRealCitizens.Where(a =>
                        a.NetIncome < 3 &&
                        a.Age < 25 &&
                        a.Family > 2
                    ).ToList();
        }
        System.Random random = new System.Random();
        return filterRealCitizens[random.Next(filterRealCitizens.Count)];
    }
}
