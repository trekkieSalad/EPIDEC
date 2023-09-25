using ABMU.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class FriendshipFactory : RelationshipFactory
{
    private AbstractController controller;
    private WorldParameters world;
    public FriendshipFactory(AbstractController _controller)
    {
        controller = _controller;
        world = WorldParameters.GetInstance();
    }
    public void createNetwork(Citizen citizen, List<Citizen> citizens)
    {
        setSocialNetworks(citizen, citizens);
    }

    private void setSocialNetworks(Citizen citizen, List<Citizen> citizens)
    {
        List<Citizen> allCandidates = citizens;
        allCandidates = allCandidates.Except(citizen.GetFriends()).ToList();
        allCandidates.Remove(citizen);

        List<Citizen> inAgeCandidates =
            allCandidates.Where(c => Math.Abs(c.Age - citizen.Age) < 5).ToList();


        while (citizen.Friendships.Count < world.numFriends &&
            inAgeCandidates.Count > 0)
        {
            Citizen candidate =
                inAgeCandidates[UnityEngine.Random.Range(0, inAgeCandidates.Count)];
            inAgeCandidates.Remove(candidate);
            allCandidates.Remove(candidate);

            if (candidate.Friendships.Count < world.numFriends + 1)
            {
                citizen.AddFriendship(candidate);
                candidate.AddFriendship(citizen);
            }
        }

        if (UnityEngine.Random.value < world.randomFriendProb && 
            citizen.Friendships.Count < world.numFriends + 1 &&
            allCandidates.Count > 0)
        {
            Citizen candidate =
                allCandidates[UnityEngine.Random.Range(0, allCandidates.Count)];

            if (candidate.Friendships.Count < world.numFriends + 1)
            {
                citizen.AddFriendship(candidate);
                candidate.AddFriendship(citizen);
            }
        }

    }
}