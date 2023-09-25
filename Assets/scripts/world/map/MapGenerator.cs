using System;
using System.Collections.Generic;
using TriangleNet.Topology;
using TriangleNet.Geometry;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

public abstract class MapGenerator : MonoBehaviour
{
    //text data
    //public TextAsset censusData;
    public TextAsset placesData;
    public TextAsset sectionsData;
    
    public List<Place> places = new List<Place>();


    //geometry
    [SerializeField] protected ICollection<Triangle>[] polygons;
    [SerializeField] protected int[][] repeatedTrianglePolygons;
    [SerializeField] protected MyDictionary<String, int> secRef = new MyDictionary<string, int>();

    public float xCoordMin;
    public float xCoordMax;
    public float zCoordMin;
    public float zCoordMax;

    //normalize
    public float newXCoordMin;
    public float newXCoordMax;
    public float newZCoordMin;
    public float newZCoordMax;
    public float scaleFactorX;
    public float scaleFactorZ;

    //gameobjects
    public GameObject delimiter;
    public GameObject placePrefab;

    //visualization
    public bool showVertex;

    #region Polygons

    public Vector3 normalizePoint(float x, float y)
    {
        return new Vector3(
            (float)Utils.maxMinNormalize(
                x, xCoordMin, xCoordMax, newXCoordMin, newXCoordMax
                ) * scaleFactorX,
            0, 
            (float)Utils.maxMinNormalize(
                y, zCoordMin, zCoordMax, newZCoordMin, newZCoordMax
                ) * scaleFactorZ);
    }

    /// <summary>
    /// Generates te city polygons and triangulate then using the Delaunay triangulation
    /// </summary>
    /// <param name="sectionsJson">A censal sections data object</param>
    /// <returns></returns>
    public ICollection<Triangle>[] GeneratePolygons(JArray sectionsJson)
    {
        polygons = new ICollection<Triangle>[sectionsJson.Count];
        repeatedTrianglePolygons = new int[sectionsJson.Count][];
        int j = 0;
        Vertex[] vertices;
        GameObject sectionContainer = GameObject.Find("Sections");
        foreach (JObject section in sectionsJson)
        {
            string sectionName = ((JObject)section["properties"]).GetValue("secRef").ToString();
            GameObject sectionObject = new GameObject();
            sectionObject.name = sectionName;
            sectionObject.transform.parent = sectionContainer.transform;

            Geometry geometry = ((JObject)section["geometry"]).ToObject<Geometry>();
            vertices = new Vertex[geometry.coordinates.Length];
            int i = 0;

            foreach (float[] points in geometry.coordinates)
            {
                vertices[i] = new Vertex(Utils.maxMinNormalize(points[0], xCoordMin, xCoordMax, newXCoordMin, newXCoordMax) * scaleFactorX, Utils.maxMinNormalize(points[1], zCoordMin, zCoordMax, newZCoordMin, newZCoordMax) * scaleFactorZ);
                //Display flag, only for debug. 
                if (showVertex)
                {
                    GameObject agent = Instantiate(delimiter);
                    agent.transform.parent = sectionObject.transform;
                    agent.name = "Delimiter " + i;
                    agent.transform.position = normalizePoint(points[0], points[1]);
                    i++;
                }
            }
            Polygon polygon = new Polygon();
            polygon.Add(new Contour(vertices));
            var mesh = polygon.Triangulate();

            double[] areas = new double[mesh.Triangles.Count];
            Triangle[] triangleArray = new Triangle[mesh.Triangles.Count];
            mesh.Triangles.CopyTo(triangleArray, 0);
            List<int> repeatedTriangles = new List<int>();

            for (int n = 0; n < mesh.Triangles.Count; n++)
                areas[n] = CalculateArea(triangleArray[n]);
            double minArea = areas.Min();

            for (int n = 0; n < mesh.Triangles.Count; n++)
                for (int m = 0; m < (int)(areas[n] / minArea); m++)
                    repeatedTriangles.Add(n);

            polygons[j] = mesh.Triangles;
            repeatedTrianglePolygons[j] = repeatedTriangles.ToArray();
            secRef.Add(((JObject)section["properties"]).GetValue("secRef").ToString(), j);
            j++;
        }
        return polygons;
    }

    /// <summary>
    /// Calculates a point in the Unity virtual environment that lies within the given triangle.
    /// </summary>
    /// <param name="triangle">Triangle object representing a piece of a census section.</param>
    /// <returns>Vector 3 with the coordinates of a point inside the given triangle</returns>
    public Vector3 GenerateRandomPointWithinTriangle(Triangle triangle)
    {
        float r1 = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f));
        float r2 = UnityEngine.Random.Range(0f, 1f);
        float w1 = 1 - r1;
        float w2 = r1 * (1 - r2);
        float w3 = r2 * r1;

        Vector2 v1 = new Vector2((float)triangle.GetVertex(0).X, (float)triangle.GetVertex(0).Y);
        Vector2 v2 = new Vector2((float)triangle.GetVertex(1).X, (float)triangle.GetVertex(1).Y);
        Vector2 v3 = new Vector2((float)triangle.GetVertex(2).X, (float)triangle.GetVertex(2).Y);
        Vector2 point = w1 * v1 + w2 * v2 + w3 * v3;
        return new Vector3(point.x, 0, point.y);

    }

    static double CalculateArea(Triangle triangle)
    {
        var vertex0 = triangle.GetVertex(0);
        var vertex1 = triangle.GetVertex(1);
        var vertex2 = triangle.GetVertex(2);

        // Calcula el área utilizando la fórmula del determinante.
        double area = 0.5 * Math.Abs(
            vertex0.X * (vertex1.Y - vertex2.Y) +
            vertex1.X * (vertex2.Y - vertex0.Y) +
            vertex2.X * (vertex0.Y - vertex1.Y)
        );

        return area;
    }

    public Triangle ChooseRandomTriangle(Triangle[] triangles, int[] indexes)
    {
        return triangles[indexes[UnityEngine.Random.Range(0, indexes.Length)]];
    }

    #endregion

    #region Sections

    /// <summary>
    /// Reads the JSON file of the census tracts and generates the array of polygons.
    /// </summary>
    public void CreateCitySections()
    {
        JObject sections = JObject.Parse(sectionsData.text);
        JArray sectionsArray = (JArray)sections["sectionsData"];
        polygons = GeneratePolygons(sectionsArray);
    }

    #endregion

    #region Places

    /// <summary>
    /// Read JSON file of relevant locations and generate them in the virtual environment
    /// </summary>
    abstract public List<Place> CreateRelevantPlaces();

    #endregion

    #region Citizens

    public void LocateCitizens(List<Citizen> citizens)
    {
        foreach (Citizen citizen in citizens)
        {
            LocateCitizen(citizen);
        }
    }

    private void LocateCitizen(Citizen citizen)
    {
        ICollection<Triangle> triangles = polygons[secRef[citizen.section]];
        int[] posibleIndex = repeatedTrianglePolygons[secRef[citizen.section]];
        Triangle[] triangleArray = new Triangle[triangles.Count];
        triangles.CopyTo(triangleArray, 0);
        Triangle triangle = ChooseRandomTriangle(triangleArray, posibleIndex);
        Vector3 position = GenerateRandomPointWithinTriangle(triangle);
        citizen.transform.position = position;
    }

    #endregion
}
