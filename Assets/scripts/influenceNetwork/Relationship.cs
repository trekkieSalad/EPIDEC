using UnityEngine;

public class Relationship
{
    // Ciudadanos que establecen la amistad
    public Citizen owner;
    public Citizen receptor;

    // Nos dice si los ciudadanos tienen el mismo comportamiento
    public bool sameBehavior
    {
        get { return owner.behavior.Equals(receptor.behavior); }
    }

    // Capacidad de influencia de la relacion
    public double trust;

    // gullibility and persuasion
    public double persuasion;
    public double gullibility;
    public bool signaled;
    public bool inquired;

    public Relationship(Citizen owner, Citizen receptor)
    {
        this.owner = owner;
        this.receptor = receptor;
        trust = Random.value;
        persuasion = 0;
        gullibility = 0;
        signaled = false;
        inquired = false;
    }

}

/*public class Friendship : Relationship
{
    // Alias para el segundo extremo de la relacion de amistad
    public Citizen friend { get { return owner; } }

    // Nos dice si los ciudadanos tienen el mismo comportamiento
    public bool sameBehavior
    { 
        get { return owner.behavior.Equals(friend.behavior); } 
    }

    // Capacidad de influencia del amigo
    public double trust;

    // gullibility and persuasion
    public double persuasion;
    public double gullibility;
    public bool signaled;
    public bool inquired;

    public Friendship(Citizen owner, Citizen friend) : base(owner, friend)
    {
        trust = Random.value;
        persuasion = 0;
        gullibility = 0;
        signaled = false;
        inquired = false;
    }
}*/