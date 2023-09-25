using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceFactory
{
    protected Vector3 normalizePoint(Vector3 point, 
        Vector3 oldMinCoords, Vector3 newMinCoords,
        Vector3 oldMaxCoords, Vector3 newMaxCoords, Vector3 scaleFactor)
    {
        float x = (float)Utils.maxMinNormalize(
                        point.x, oldMinCoords.x, oldMaxCoords.x, newMinCoords.x, newMaxCoords.x
                        ) * scaleFactor.x;
        float y = (float)Utils.maxMinNormalize(
                        point.y, oldMinCoords.y, oldMaxCoords.y, newMinCoords.y, newMaxCoords.y
                        ) * scaleFactor.y;
        float z = (float)Utils.maxMinNormalize(
                        point.z, oldMinCoords.z, oldMaxCoords.z, newMinCoords.z, newMaxCoords.z
                        ) * scaleFactor.z;

        return new Vector3(x, y, z);
    }
    public abstract List<Place> CreatePlaces(GameObject placePrefab,
        Vector3 oldMinCoords, Vector3 newMinCoords,
        Vector3 oldMaxCoords, Vector3 newMaxCoords, Vector3 scaleFactor);    
}
