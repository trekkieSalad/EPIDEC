
using System;

public enum PlaceType
{
    [Capacity(25)]   // limite de gente que puede haber a la vez (numero a explorar al contagiar)
    [Quantity(5)]
    EducationalCenter,
    [Capacity(20)]
    [Quantity(30)]
    WorkCenter,
    [Capacity(30)]
    [Quantity(30)]
    LeisureZone,
    [Capacity(30)]
    [Quantity(30)]
    MarketPlace,
    [Capacity(25)]
    [Quantity(20)]
    PublicInfrastructure
}

public class CapacityAttribute : Attribute
{
    public int Capacity;

    public CapacityAttribute(int capacity)
    {
        Capacity = capacity;
    }
}

public class QuantityAttribute : Attribute
{
    public int Quantity;

    public QuantityAttribute(int quantity)
    {
        Quantity = quantity;
    }
}