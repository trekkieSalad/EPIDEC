using System;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class CoruñaPlaceFactory : PlaceFactory
{
    private TextAsset placesJson;

    public CoruñaPlaceFactory(TextAsset placesJson)
    {
        this.placesJson = placesJson;
    }

    public override List<Place> CreatePlaces(GameObject placePrefab, 
        Vector3 oldMinCoords, Vector3 newMinCoords,
        Vector3 oldMaxCoords, Vector3 newMaxCoords, Vector3 scaleFactor)
    {
        List<Place> places = new List<Place>();
        int id = 0;

        // cogemos el objeto Section hijo de MapGenerator
        GameObject placeContainer = GameObject.Find("Places");


        JsonFeatureCollection featureCollection = JsonConvert.DeserializeObject<JsonFeatureCollection>(placesJson.text);
        foreach (JsonFeature feature in featureCollection.features)
        {
            GameObject placeObject = MonoBehaviour.Instantiate(placePrefab);
            placeObject.name = feature.properties["@id"].ToString() + " - " + id;
            placeObject.tag = "Place";

            Vector3 position = new Vector3(
                feature.geometry.coordinates[0], 
                0, 
                feature.geometry.coordinates[1]);

            placeObject.transform.position = normalizePoint(
                position, oldMinCoords, newMinCoords,
                oldMaxCoords, newMaxCoords, scaleFactor );
            placeObject.transform.parent = placeContainer.transform;
            Place place = placeObject.GetComponent<Place>();

            place = clasifyPlace(place, feature);

            places.Add(place);

            id++;
        }

        return places;
    }

    private Place clasifyPlace(Place place, JsonFeature feature)
    {
        // Clasificamos el tipo de lugar entre:
        // - centro educativo
        // - infraestructura publica
        // - comercio
        // - trabajo
        // - ocio
        if( feature.properties.ContainsKey("leisure") ||
            feature.properties.ContainsKey("tourism") ||
            feature.properties.ContainsKey("religion") )

            place.AddType(PlaceType.LeisureZone);

        if( feature.properties.ContainsKey("amenity"))
            if (feature.properties["amenity"].ToString() == "marketplace")
            {
                place.AddType(PlaceType.MarketPlace);
            }
            else if (new List<string> {"restaurant", "pub", "cafe", "ice_cream", "theatre", "cinema" }.
                Contains(feature.properties["amenity"].ToString())) 
            {
                place.AddType(PlaceType.LeisureZone);
                place.AddType(PlaceType.WorkCenter);
            }
            else if (new List<string> { "courthouse", "fire_station", "police", "townhall" }.
                Contains(feature.properties["amenity"].ToString()))
            {
                place.AddType(PlaceType.PublicInfrastructure);
            }
            else if (new List<string> { "hospital", "clinic", "veterinary", "school" }.
                Contains(feature.properties["amenity"].ToString()))
            {
                if (feature.properties.ContainsKey("operator:type") &&
                    feature.properties["operator:type"].ToString() != "private" ||
                    !feature.properties.ContainsKey("operator:type"))
                    place.AddType(PlaceType.PublicInfrastructure);
                else
                    place.AddType(PlaceType.WorkCenter);
                
            }
            else if (new List<string> { "college", "university" }.
                Contains(feature.properties["amenity"].ToString()))
            {
                place.AddType(PlaceType.EducationalCenter);
                if (feature.properties.ContainsKey("operator:type") &&
                    feature.properties["operator:type"].ToString() != "private" ||
                    !feature.properties.ContainsKey("operator:type"))
                    place.AddType(PlaceType.PublicInfrastructure);
                else
                    place.AddType(PlaceType.WorkCenter);

            }

        if (feature.properties.ContainsKey("shop"))
        {
            place.AddType(PlaceType.WorkCenter);
            if (new List<string> { 
                    "supermarket",  "bakery",   "butcher",  "kiosk",    "dairy", 
                    "convenience",  "seafood",  "deli",     "frozen_food", 
                    "greengrocer",  "cheese",   "newsagent"}.
                Contains(feature.properties["shop"].ToString()))
            {
                place.AddType(PlaceType.MarketPlace);
            }
            else
            {
                place.AddType(PlaceType.LeisureZone);
            }
        }

        if (feature.properties.ContainsKey("office"))
            if (feature.properties["office"].ToString() == "government")
                place.AddType(PlaceType.PublicInfrastructure);
            else 
                place.AddType(PlaceType.WorkCenter);

        if (feature.properties.ContainsKey("landuse"))
            if (feature.properties["landuse"].ToString() == "industrial")
                place.AddType(PlaceType.WorkCenter);
            else if (feature.properties["landuse"].ToString() == "military")
                place.AddType(PlaceType.PublicInfrastructure);

        


        return place;
    }
}
