using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;
using ABMU.Core;
using Random = UnityEngine.Random;

#region Citizen Enums
public enum Behavior
{
    Accept,
    Reject,
}

public enum CitizenGender
{
    Male,
    Female,
}
#endregion

public enum EconomicActivity {
    Employed = 1,
    Unemployed = 2,
    Freelance = 3,
    CivilServant = 4,
    Executive = 5,
    Student = 6,
    Inactive = 7,
}



[Serializable]
public class Citizen : AbstractAgent {

    // ATTRIBUTES
    #region ATTRIBUTES

    #region Simulation Attributes
    [Header("Simulation")]
    [SerializeField] private WorldParameters _worldParameters;
    //public AbstractController controller;
    public bool Simulated;

    #endregion

    #region Sociodemographic Attributes
    [Header("Sociodemographic")]
    public CitizenGender Gender;
    public int Age;
    public int Family;
    public bool RuralHouse; //preguntar si debería ser booleano
    public EconomicActivity economicActivity;
    public bool EssentialJob; //preguntar si debería ser booleano
    public int NetIncome;

    #endregion

    #region Behavior Attributes
    [Header("Behavior")]
    public double needAImportance;
    public double needBImportance;
    public double membershipImportance;

    public double needASatisfactionA;
    public double needBSatisfactionA;
    public double needBSatisfactionB;
    public double needASatisfactionB;
    public double membershipSatisfactionA;
    public double membershipSatisfactionB;

    public double needAEvaluationA;
    public double needBEvaluationA;
    public double needBEvaluationB;
    public double needAEvaluationB;
    public double membershipEvaluationA;
    public double membershipEvaluationB;

    public double satisfactionA;
    public double satisfactionB;
    public double satisfaction;

    public bool needADilemma;
    public bool needBDilemma;
    public bool membershipDilemma;

    public Behavior behavior;
    public Behavior initBehavior;
    public double dissonanceTolerance;
    public double dissonanceA;
    public double dissonanceB;
    public double dissonanceStrength;
    public string section;

    #endregion

    #region SEIRD Attributes
    [Header("SEIRD")]
    public SirState ActualState;
    public bool Asintomatic;
    public bool Quarantine;

    #endregion

    #region Critical Nodes Attributes
    [Header("Critical Nodes")]
    public double CityCouncilTrust;
    public double PoliticalOppositionTrust;
    public double LocalMediaTrust;
    public double LocalMediaOppositionTrust;
    public double TwitterTrust;
    public bool IsTwitterUser;

    #endregion

    #region Interaction Attributes
    [Header("Interaction")]
    public bool Inquiring;
    public bool Signaling;
    public bool RandomConversation;
    public int friends;
    public int neighbors;
    public List<Relationship> Friendships = new List<Relationship>();
    public List<Relationship> Neighborhood = new List<Relationship>();
    public List<Relationship> Relationships
    {
        get
        {
            List<Relationship> relationships = new List<Relationship>();
            relationships.AddRange(Neighborhood);
            relationships.AddRange(Friendships);
            return relationships;
        }
    }

    #endregion

    #region Place Attributes
    [Header("Place")]
    public MyDictionary<PlaceType, Place> Places = new MyDictionary<PlaceType, Place>();
    public Place ActualPlace;
    #endregion

    #endregion

    #region Life Cycle Methods

    public override void Awake()
    {
        /*base.Awake();
        base.Init();
        initBehavior = behavior;
        CreateStepper(BehaviourQueue);
        _worldParameters = WorldParameters.GetInstance();*/
    }

    public void OwnInit(AbstractController controller)
    {
        this.controller = controller;
        controller.RegisterAgent(this);
        base.Init();
        initBehavior = behavior;
        CreateStepper(BehaviourQueue);
        _worldParameters = WorldParameters.GetInstance();
    }

    void Update()
    {
        MoveBehaviour();
    }

    public void BehaviourQueue()
    {
        Inquiring = false;
        Signaling = false;
        RandomConversation = false;

        InquireBehaviour();
        SignalBehaviour();
        RandomComm();

        ResetComms();

        ActualState.UpdateState();
        AloneBehavior();

        LateUpdatePlace();

        if (ActualPlace != null && ActualState.Type.Equals(StateType.Infected))
            SpreadVirus();

        friends = Friendships.Count;
        neighbors = Neighborhood.Count;
    }

    #region On Move
    private void Move()
    {
        WorldController controller = this.controller as WorldController;
        bool isLaboralDay = Utils.IsLaboralDay(controller.day);
        ActualPlace = null;

        if (!isLaboralDay)
        {
            if (Random.value > 0.75)
                ActualPlace = controller.GetRandomPlace(PlaceType.LeisureZone);
            else
                ActualPlace = Places[PlaceType.LeisureZone];
        }
        else
        {
            switch (economicActivity)
            {
                case EconomicActivity.Employed:
                case EconomicActivity.Executive:
                    ActualPlace = Places[PlaceType.WorkCenter];
                    break;
                case EconomicActivity.Freelance:
                    if (Random.value > 0.5)
                        ActualPlace = Places[PlaceType.WorkCenter];
                    break;
                case EconomicActivity.Unemployed:
                case EconomicActivity.Inactive:
                    if (Random.value > 0.9)
                        ActualPlace = Places[PlaceType.PublicInfrastructure];
                    else if (Random.value > 0.5)
                        ActualPlace = Places[PlaceType.MarketPlace];
                    else
                        ActualPlace = Places[PlaceType.LeisureZone];
                    break;
                case EconomicActivity.CivilServant:
                    ActualPlace = Places[PlaceType.PublicInfrastructure];
                    break;
                case EconomicActivity.Student:
                    ActualPlace = Places[PlaceType.EducationalCenter];
                    break;
                default:
                    Debug.LogError("Economic Activity not found");
                    break;
            }

        }

        if (ActualPlace != null)
        {
            ActualPlace.RegisterCitizen(this);
        }
    }

    #endregion

    #region On Dead

    public void Dead()
    {
        foreach (Relationship relationship in Relationships)
        {
            relationship.receptor.RemoveRelation(this);
        }
        controller.DeregisterAgent(this);
        Destroy(GetComponent<Citizen>());
    }

    public void RemoveRelation(Citizen receptor)
    {
        Relationships.RemoveAll(r => r.receptor == receptor);
    }

    #endregion

    #region Behaviours

    private void InquireBehaviour()
    {
        if (dissonanceStrength > 0 && (needADilemma || needBDilemma))
            InquireComm();
    }

    private void SignalBehaviour()
    {
        if (dissonanceStrength > 0 && membershipDilemma)
            SignalComm();
    }

    private void AloneBehavior()
    {
        if (FriendsNumber == 0)
            MakeFriends();
    }

    private void MoveBehaviour()
    {
        List<StateType> criticalStates =
            new List<StateType>
            { StateType.Dead, StateType.ICU, StateType.Hospitalized };
        ActualPlace = null;

        if (criticalStates.Contains(ActualState.Type) ||
            (ActualState.Type.Equals(StateType.Infected) &&
            behavior.Equals(Behavior.Accept) && !Quarantine)
            )
            return;
        else Move();

    }

    #endregion

    #endregion

    #region Communication Methods

    private void InquireComm()
    {
        List<Relationship> sortRelationships = Relationships.
            OrderBy(f => f.inquired).
            ThenByDescending(f => f.sameBehavior).
            ThenByDescending(f => f.persuasion).ToList();

        Citizen inquiredFriend = sortRelationships[0].receptor;
        Inquiring = true;

        // Todo esto se puede actualizar al usar diccionarios

        double similarityNeedAImportanceA = NeedSimilarity(
            needAEvaluationA, inquiredFriend.needAEvaluationA,
            needAImportance, inquiredFriend.needAImportance);

        double similarityNeedBImportanceA = NeedSimilarity(
            needBEvaluationA, inquiredFriend.needBEvaluationA,
            needBImportance, inquiredFriend.needBImportance);

        double similarityNeedAImportanceB = NeedSimilarity(
            needAEvaluationB, inquiredFriend.needAEvaluationB,
            needAImportance, inquiredFriend.needAImportance);

        double similarityNeedBImportanceB = NeedSimilarity(
            needBEvaluationB, inquiredFriend.needBEvaluationB,
            needBImportance, inquiredFriend.needBImportance);

        double persuasionNeedAA =
            sortRelationships[0].trust * similarityNeedAImportanceA;
        double persuasionNeedBA =
            sortRelationships[0].trust * similarityNeedBImportanceA;
        double persuasionNeedAB =
            sortRelationships[0].trust * similarityNeedAImportanceB;
        double persuasionNeedBB =
            sortRelationships[0].trust * similarityNeedBImportanceB;

        needASatisfactionA = NewNeedSatisfaction(needASatisfactionA,
            persuasionNeedAA, inquiredFriend.needASatisfactionA);
        needBSatisfactionA = NewNeedSatisfaction(needBSatisfactionA,
            persuasionNeedBA, inquiredFriend.needBSatisfactionA);
        needASatisfactionB = NewNeedSatisfaction(needASatisfactionB,
            persuasionNeedAB, inquiredFriend.needASatisfactionB);
        needBSatisfactionB = NewNeedSatisfaction(needBSatisfactionB,
            persuasionNeedBB, inquiredFriend.needBSatisfactionB);

        UpdateCitizen();

        sortRelationships[0].receptor.UpdateCitizen();
        sortRelationships[0].persuasion = persuasionNeedAA + persuasionNeedBA +
            persuasionNeedAB + persuasionNeedBB;
        sortRelationships[0].inquired = true;
    }

    private void SignalComm()
    {
        List<Relationship> sortRelationships = Relationships.
            OrderBy(f => f.signaled).
            ThenByDescending(f => f.sameBehavior).
            ThenByDescending(f => f.gullibility).ToList();

        Relationship relationship = sortRelationships[0];
        Signaling = true;

        UpdateRelationshipReceptor(relationship, true);

        UpdateCitizen();

    }

    private void RandomComm()
    {
        double prob = 0.05;

        if (prob >= Random.value)
        {
            Relationship relationship =
                Relationships[Random.Range(0, Relationships.Count)];
            RandomConversation = true;

            UpdateRelationshipReceptor(relationship);

            UpdateCitizen();

        }
    }

    private void UpdateRelationshipReceptor(Relationship relationship, bool signalComm = false)
    {

        Citizen receptor = relationship.receptor;

        double similarityNeedAImportanceA = NeedSimilarity(
            needAEvaluationA, receptor.needAEvaluationA,
            needAImportance, receptor.needAImportance);

        double similarityNeedBImportanceA = NeedSimilarity(
            needBEvaluationA, receptor.needBEvaluationA,
            needBImportance, receptor.needBImportance);

        double similarityNeedAImportanceB = NeedSimilarity(
            needAEvaluationB, receptor.needAEvaluationB,
            needAImportance, receptor.needAImportance);

        double similarityNeedBImportanceB = NeedSimilarity(
            needBEvaluationB, receptor.needBEvaluationB,
            needBImportance, receptor.needBImportance);



        Relationship reverseLink = null;
        foreach (Relationship friendship in receptor.Relationships)
        {
            if (friendship.receptor == this)
            {
                reverseLink = friendship;
                break;
            }
        }

        double sigTrust = reverseLink.trust;
        double persuasionNeedAA = sigTrust * similarityNeedAImportanceA;
        double persuasionNeedBA = sigTrust * similarityNeedBImportanceA;
        double persuasionNeedAB = sigTrust * similarityNeedAImportanceB;
        double persuasionNeedBB = sigTrust * similarityNeedBImportanceB;

        receptor.needASatisfactionA = NewNeedSatisfaction(
            receptor.needASatisfactionA, persuasionNeedAA,
            needASatisfactionA);
        receptor.needBSatisfactionA = NewNeedSatisfaction(
            receptor.needBSatisfactionA, persuasionNeedBA,
            needBSatisfactionA);
        receptor.needASatisfactionB = NewNeedSatisfaction(
            receptor.needASatisfactionB, persuasionNeedAB,
            needASatisfactionB);
        receptor.needBSatisfactionB = NewNeedSatisfaction(
            receptor.needBSatisfactionB, persuasionNeedBB,
            needBSatisfactionB);

        receptor.UpdateCitizen();

        if (signalComm)
        {
            relationship.gullibility = persuasionNeedAA + persuasionNeedBB;
            relationship.signaled = true;
        }

    }

    #endregion

    #region SEIRD Methods

    private void SpreadVirus()
    {
        List<Citizen> contacts = ActualPlace.GetSimultaneousUsers(this);
        contacts.Remove(this);
        bool s = ActualState.Type.Equals(StateType.Infected) &&
            behavior.Equals(Behavior.Accept) && !Asintomatic;
        
        double pSE;
        foreach (Citizen contact in contacts)
        {
            if (contact.ActualState.Type == StateType.Susceptible)
            {
                if (contact.behavior.Equals(Behavior.Accept) &&
                    behavior.Equals(Behavior.Accept))
                    pSE = _worldParameters.pSeAA;
                else if (contact.behavior.Equals(Behavior.Reject) &&
                    behavior.Equals(Behavior.Reject))
                    pSE = _worldParameters.pSeBB;
                else
                    pSE = _worldParameters.pSeAB;

                if (pSE >= Random.value)
                {
                    contact.ActualState.Expose();
                }
            }
        }

    }

    #endregion

    #region Attributes Update Methods

    public void CreateSocialNetwork()
    {
        WorldController worldController = (WorldController)controller;
        // Creamos las redes sociales
        RelationshipFactory friendshipFactory = new FriendshipFactory(controller);
        friendshipFactory.createNetwork(this, worldController.Citizens);
    }

    public void UpdateCitizen(bool initialization = false)
    {
        UpdateEvaluations(initialization);
        UpdateDissonances();
        CalculateBehavior();
        UpdateEvaluations(initialization);
        UpdateDissonances();
    }

    private void UpdateEvaluations(bool initialization = false)
    {
        needAEvaluationA = needAImportance * needASatisfactionA;
        needBEvaluationA = needBImportance * needBSatisfactionA;
        needAEvaluationB = needAImportance * needASatisfactionB;
        needBEvaluationB = needBImportance * needBSatisfactionB;

        double similar = 0;
        double different = 0;
        foreach (Relationship friendship in Friendships)
        {
            if (friendship.sameBehavior)
                similar++;
            else
                different++;
        }
        similar = similar / FriendsNumber;
        different = different / FriendsNumber;

        membershipSatisfactionA = 0;
        membershipSatisfactionB = 0;

        if (behavior.Equals(Behavior.Accept))
        {
            membershipSatisfactionA =
                Utils.maxMinNormalize(similar, 0, 1, -1, 1);
            membershipSatisfactionB =
                Utils.maxMinNormalize(different, 0, 1, -1, 1);
        }
        else if (initialization)
        {
            membershipSatisfactionA =
                Utils.maxMinNormalize(different, 0, 1, -1, 1);
            membershipSatisfactionB =
                Utils.maxMinNormalize(similar, 0, 1, -1, 1);
        }
        else
        {
            membershipSatisfactionA = different;
            membershipSatisfactionB = similar;
        }

        membershipEvaluationA = membershipImportance * membershipSatisfactionA;
        membershipEvaluationB = membershipImportance * membershipSatisfactionB;

        satisfactionA =
            (needAEvaluationA + membershipEvaluationA + needBEvaluationA) / 3;
        satisfactionB =
            (needAEvaluationB + membershipEvaluationB + needBEvaluationB) / 3;
    }

    private void UpdateDissonances()
    {
        needADilemma = false;
        needBDilemma = false;
        membershipDilemma = false;

        List<double> evaluationsList = new List<double>()
            {needAEvaluationA, needBEvaluationA, membershipEvaluationA};

        dissonanceA = DissonanceStatus(evaluationsList);
        double dissonanceStrengthA =
            (dissonanceA - dissonanceTolerance) / (1 - dissonanceTolerance);

        evaluationsList = new List<double>()
            {needAEvaluationB, needBEvaluationB, membershipEvaluationB};

        dissonanceB = DissonanceStatus(evaluationsList);
        double dissonanceStrengthB =
            (dissonanceB - dissonanceTolerance) / (1 - dissonanceTolerance);

        double evaluation1;
        double evaluation2;
        double membershipEvaluation;

        if (behavior.Equals(Behavior.Accept))
        {
            dissonanceStrength = Math.Max(0, dissonanceStrengthA);

            evaluation1 = needAEvaluationA;
            evaluation2 = needBEvaluationA;
            membershipEvaluation = membershipEvaluationA;

        }
        else
        {
            dissonanceStrength = Math.Max(0, dissonanceStrengthB);

            evaluation1 = needAEvaluationB;
            evaluation2 = needBEvaluationB;
            membershipEvaluation = membershipEvaluationB;
        }

        if ((evaluation1 > 0 && membershipEvaluation < 0 && evaluation2 < 0) ||
            (evaluation1 < 0 && membershipEvaluation > 0 && evaluation2 > 0))
            needADilemma = true;

        if ((evaluation1 < 0 && membershipEvaluation > 0 && evaluation2 < 0) ||
            (evaluation1 > 0 && membershipEvaluation < 0 && evaluation2 > 0))
            membershipDilemma = true;

        if ((evaluation1 < 0 && membershipEvaluation < 0 && evaluation2 > 0) ||
            (evaluation1 > 0 && membershipEvaluation > 0 && evaluation2 < 0))
            needBDilemma = true;
    }

    private void CalculateBehavior()
    {
        bool random = Random.value < 0.5;
        bool fSatisfaction =
            FurtherComparisonNeeded(satisfactionA, satisfactionB, 2);
        bool fDissonance =
            FurtherComparisonNeeded(dissonanceA, dissonanceB, 1);
        bool fEvaluation =
            FurtherComparisonNeeded(needAEvaluationA, needAEvaluationB, 2);
        bool satisfaction = satisfactionA > satisfactionB;
        bool dissonance = dissonanceA < dissonanceB;
        bool evaluation = needAEvaluationA > needAEvaluationB;

        if (fSatisfaction && fDissonance && fEvaluation && random
            || fSatisfaction && fDissonance && !fEvaluation && evaluation
            || fSatisfaction && !fDissonance && dissonance
            || !fSatisfaction && satisfaction)
        {
            behavior = Behavior.Accept;
            this.satisfaction = satisfactionA;
        }
        else
        {
            behavior = Behavior.Reject;
            this.satisfaction = satisfactionB;
        }
    }

    

    #endregion

    #region Agent Utils Methods

    private double DissonanceStatus(List<double> evaluationsList)
    {
        double satisfying = evaluationsList.Where(i => i > 0).ToList().Sum();
        double dissatisfying = evaluationsList.Where(i => i < 0).ToList().Sum();
        double dissonant = Math.Min(Math.Abs(satisfying), Math.Abs(dissatisfying));
        double consonant = Math.Max(Math.Abs(satisfying), Math.Abs(dissatisfying));
        double dissonance = ((dissonant + consonant) != 0) ?
            (2 * dissonant) / (dissonant + consonant) : 0;

        return dissonance;
    }

    private bool FurtherComparisonNeeded(
            double dimensionA, double dimensionB, double theoricalRange)
    {
        return (dimensionA > dimensionB - 0.1 * theoricalRange &&
                dimensionA < dimensionB + 0.1 * theoricalRange);
    }

    private double NeedSimilarity(
        double needEvaluation, double friendNeedEvaluation,
        double needImportance, double friendNeedImportance)
    {
        if (needEvaluation * friendNeedEvaluation > 0)
            return 0.4 * (1 - Math.Abs(needImportance - friendNeedImportance));
        else
            return 0;
    }

    public double NewNeedSatisfaction(double needSatisfaction,
        double needPersuasion, double friendNeedSatisfaction)
    {
        return (1 - needPersuasion) * needSatisfaction +
            needPersuasion * friendNeedSatisfaction;
    }

    private void ResetComms()
    {
        if (Friendships.All(f => f.inquired))
        {
            foreach (Relationship friendship in Friendships)
                friendship.inquired = false;
        }


        if (Friendships.All(f => f.signaled))
        {
            foreach (Relationship friendship in Friendships)
                friendship.signaled = false;
        }
    }
    #endregion

    #region Friendship methods

    public int FriendsNumber
    {
        get { return Friendships.Count; }
    }

    public void AddFriendship(Citizen friend)
    {
        Friendships.Add(new Relationship(this, friend));
    }

    public List<Citizen> GetFriends()
    {
        List<Citizen> friends = new List<Citizen>();
        foreach (Relationship friendship in Friendships)
        {
            friends.Add(friendship.receptor);
        }
        return friends;
    }

    public bool IsFriend(Citizen friend)
    {
        return GetFriends().Contains(friend);
    }

    private void MakeFriends()
    {
        WorldController worldController = controller as WorldController;
        AddFriendship(worldController.GetRandomCitizen());
    }
    #endregion

    #region Social Circle methods

    public int NeighborsNumber
    {
        get { return Neighborhood.Count; }
    }

    public void AddNeighbor(Citizen neighbor)
    {
        Neighborhood.Add(new Relationship(this, neighbor));
    }

    public List<Citizen> GetNeighbors()
    {
        List<Citizen> neighbors = new List<Citizen>();
        foreach (Relationship neighbor in this.Neighborhood)
        {
            neighbors.Add(neighbor.receptor);
        }
        return neighbors;
    }
    #endregion

    #region Place methods

    public void AddPlace(PlaceType type, Place place)
    {
        Places.Add(type, place);
    }

    private void LateUpdatePlace()
    {
        bool s = ActualState.Type.Equals(StateType.Infected) &&
            behavior.Equals(Behavior.Accept) && !Quarantine;
        if (s)
        {
            if (ActualPlace != null)
                ActualPlace.UnregisterCitizen(this);
            ActualPlace = null;
        }
    }

    #endregion

    #region Citizen Utils
    public int CurrentTick
    {
        get { return controller.currentTick; }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
    #endregion
}