
using System.Collections.Generic;
using System.IO;

public abstract class DataRecorder
{
    public string dataFileName;
    public string csvContent = "";
    public List<Citizen> Citizens;
    public int initialPopulation;

    public DataRecorder(string dataFileName, List<Citizen> Citizens)
    {
        this.dataFileName = dataFileName;
        this.Citizens = Citizens;
        initialPopulation = Citizens.Count;
    }

    public abstract void AddCsvLine(int currentIteration);

    public void ExportData()
    {
        File.WriteAllText(dataFileName, csvContent);
    }
}