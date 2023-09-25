using System.Collections;
using System.Collections.Generic;
using ABMU.Core;

using UnityEngine;

public abstract class CriticalNode : AbstractAgent
{
    [Header("Users info")]
    public List<Citizen> Users = new List<Citizen>();
    public WorldParameters WorldParameters;

    [Header("Factors")]
    public double PersuasionFactor;

    public int CurrentTick
    {
        get { return controller.currentTick; }
    }

    public override void Awake()
    {
        WorldParameters = WorldParameters.GetInstance();
        AbstractController controller =
            FindObjectOfType<AbstractController>();

        // Añadimos el agente al mundo y obtenemos su cola de comportamientos
        if (controller == null) Debug.LogError("No controller found");
        else
        {
            base.Awake();
            base.Init();
            CreateStepper(PolarizeStrategy);
            LoadData();
        }
    }


    public abstract void PolarizeStrategy();
    public abstract void LoadData();

    public void PolarizeCitizen(Citizen citizen, double ASatisfaction, double BSatisfaction)
    {
        citizen.needASatisfactionA = citizen.NewNeedSatisfaction(
            citizen.needASatisfactionA, citizen.TwitterTrust * PersuasionFactor, ASatisfaction);
        citizen.needASatisfactionB = citizen.NewNeedSatisfaction(
            citizen.needASatisfactionB, citizen.TwitterTrust * PersuasionFactor, BSatisfaction);
        citizen.needBSatisfactionA = citizen.NewNeedSatisfaction(
            citizen.needBSatisfactionA, citizen.TwitterTrust * PersuasionFactor, ASatisfaction);
        citizen.needBSatisfactionB = citizen.NewNeedSatisfaction(
            citizen.needBSatisfactionB, citizen.TwitterTrust * PersuasionFactor, BSatisfaction);

        citizen.UpdateCitizen();
    }
}