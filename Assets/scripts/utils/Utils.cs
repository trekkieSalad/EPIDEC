using System;

using UnityEngine;

public static class Utils
{
    public static string[] CsvRowData(string line, string delimiter)
    {
        return line.Split(
            new string[] { delimiter }, System.StringSplitOptions.None);
    }

    public static double maxMinNormalize(double value, double actualMin, 
        double actualMax, double newMin, double newMax)
    {
        double normValue = (value - actualMin) / 
            (actualMax - actualMin) * (newMax - newMin) + newMin;

        if (double.IsNaN(normValue)) return newMin;

        return Math.Min(Math.Max(normValue, newMin), newMax);        
    }

    public static int[] stringArrayToInt(string[] data)
    {
        int[] result = new int[data.Length];

        for (int i = 0; i < data.Length; i++) result[i] = int.Parse(data[i]);

        return result;
    }

    public static double randomNormal(double mean, double stdDev)
    {
        System.Random rand = new System.Random();
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = 
            Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        double randNormal = mean + stdDev * randStdNormal;
        randNormal = Math.Min(Math.Max(randNormal, 0), 1);
        return randNormal;
    }

    public static double randomNormalTruncated(
        double mean, double stdDev, double min, double max)
    {
        double randNormal = randomNormal(mean, stdDev);
        
        if (randNormal < min || randNormal > max)
            randNormal = randomNormalTruncated(mean, stdDev, min, max);
            
        return randNormal;
    }

    public static int logNormal()
    {
        double normal = randomNormal(1.621, 0.418); 
        return (int)Math.Round(Math.Exp(normal));
    }

    public static void ChangeColor(Citizen citizen, SirStateColor color)
    {
        float r = (float)((int)color / 1000000) / 255;
        float g = (float)(((int)color / 1000) % 1000) / 255;
        float b = (float)((int)color % 1000) / 255;
        Renderer renderer = citizen.GetComponent<Renderer>();
        renderer.material.color = new Color(r,g,b);

    }

    public static bool IsLaboralDay(Day day)
    {
        return day != Day.Saturday && day != Day.Sunday;
    }

}
