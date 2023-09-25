using System.Collections.Generic;

public class JsonGeometry
{
    public string type { get; set; }
    public List<float> coordinates { get; set; }
}

public class JsonFeature
{
    public string type { get; set; }
    public JsonGeometry geometry { get; set; }
    public Dictionary<string, object> properties { get; set; }
}
