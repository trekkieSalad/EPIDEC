using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using UnityEngine;

public class CoruñaGenerator : MapGenerator
{

    public PlaceFactory placeFactory;

    public override List<Place> CreateRelevantPlaces()
    {
        placeFactory = new CoruñaPlaceFactory(this.placesData);
        places.AddRange(placeFactory.CreatePlaces( placePrefab, 
            new Vector3(xCoordMin, 0, zCoordMin), 
            new Vector3(newXCoordMin, 0, newZCoordMin),
            new Vector3(xCoordMax, 0, zCoordMax),
            new Vector3(newXCoordMax, 0, newZCoordMax),
            new Vector3(scaleFactorX, 0, scaleFactorZ)));
        return places;
    }
}
