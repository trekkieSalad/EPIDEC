using System.Reflection;
using System;


public class DescriptionAttribute : Attribute
{
    public string Description;

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}

public static class EnumAttributes
{
    public static string GetDescription(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        FieldInfo enumField = enumType.GetField(enumValue.ToString());
        DescriptionAttribute[] enumInfo =
            (DescriptionAttribute[])enumField
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        string description = enumInfo.Length > 0 ?
            enumInfo[0].Description :
            enumValue.ToString();
        return description;
    }

    public static int GetCapacity(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        FieldInfo enumField = enumType.GetField(enumValue.ToString());
        CapacityAttribute[] enumInfo =
            (CapacityAttribute[])enumField
            .GetCustomAttributes(typeof(CapacityAttribute), false);
        int capacity = enumInfo.Length > 0 ?
            enumInfo[0].Capacity :
            0;
        return capacity;
    }

    public static int GetQuantity(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        FieldInfo enumField = enumType.GetField(enumValue.ToString());
        QuantityAttribute[] enumInfo =
            (QuantityAttribute[])enumField
            .GetCustomAttributes(typeof(QuantityAttribute), false);
        int quantity = enumInfo.Length > 0 ?
            enumInfo[0].Quantity :
            0;
        return quantity;
    }
}