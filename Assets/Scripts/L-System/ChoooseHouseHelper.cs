using System;
using UnityEngine;

[Serializable]
public class ChoooseHouseHelper 
{
    //The list of house types
    [SerializeField] private GameObject[] prefabs;
    public int spaceTheyareTakinng;
    public int howMuchCanBePlaced;
    public int howManyAlreadyPlaces;

    public GameObject GetRandomHouse()
    {
        howManyAlreadyPlaces++;
        //if there are more type of houses to choose from, do 
        if (prefabs.Length > 1)
        {
            var random = UnityEngine.Random.Range(0, prefabs.Length);
            return prefabs[random];
        }
        
        return prefabs[0];
    }

    public bool CanPlaceBuilding()
    {
        return howManyAlreadyPlaces < howMuchCanBePlaced;
    }

    public void Reset()
    {
        howManyAlreadyPlaces = 0;
    }
}
