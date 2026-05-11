using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [SerializeField]
    private ChoooseHouseHelper[] buildingTypes;
    
    private Dictionary<Vector3Int, GameObject> structureDictionary = new Dictionary<Vector3Int, GameObject>();
    
    [SerializeField] 
    private bool randomPlacement = false;
    
    [SerializeField] 
    [Range(0, 1)]
    private float skipPlacementProbability = 0.5f;

    private int buildingCount = 0;

    public IEnumerator PlaceStructures(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Directions> buildablePlots = FindBuildablePlots(roadPositions);
        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

        foreach (var plot in buildablePlots)
        {
            if (occupiedPositions.Contains(plot.Key) || ShouldSkipPlacement())
            {
                continue;
            }

            Quaternion rotation = GetRotationForDirection(plot.Value);

            foreach (var buildingType in buildingTypes)
            {
                if (!CanPlaceBuilding(buildingType))
                {
                    continue;
                }

                if (TryPlaceBuilding(buildingType, plot, rotation, buildablePlots, occupiedPositions))
                {
                    break;
                }
            }

            yield return null;
        }
    }

    private bool TryPlaceBuilding(ChoooseHouseHelper buildingType, KeyValuePair<Vector3Int, Directions> plot, 
        Quaternion rotation, Dictionary<Vector3Int, Directions> buildablePlots, HashSet<Vector3Int> occupiedPositions)
    {
        List<Vector3Int> requiredPositions = new List<Vector3Int>();

        if (buildingType.spaceTheyareTakinng > 1)
        {
            if (!CheckBuildingFit(buildingType, plot, buildablePlots, occupiedPositions, requiredPositions))
            {
                return false;
            }
        }

        // Place the building
        GameObject building = SpawnBuilding(buildingType.GetRandomHouse(), plot.Key, rotation);
        structureDictionary[plot.Key] = building;

        // Occupy all required positions
        foreach (var pos in requiredPositions)
        {
            occupiedPositions.Add(pos);
            structureDictionary[pos] = building;
        }

        return true;
    }

    private bool CheckBuildingFit(ChoooseHouseHelper buildingType, KeyValuePair<Vector3Int, Directions> plot,
        Dictionary<Vector3Int, Directions> buildablePlots, HashSet<Vector3Int> occupiedPositions, List<Vector3Int> requiredPositions)
    {
        Vector3Int perpendicular = GetPerpendicularDirection(plot.Value);
        int halfSize = Mathf.FloorToInt(buildingType.spaceTheyareTakinng / 2f);

        for (int i = 1; i <= halfSize; i++)
        {
            Vector3Int pos1 = plot.Key + perpendicular * i;
            Vector3Int pos2 = plot.Key - perpendicular * i;

            if (!IsPositionAvailable(pos1, buildablePlots, occupiedPositions) ||
                !IsPositionAvailable(pos2, buildablePlots, occupiedPositions))
            {
                return false;
            }

            requiredPositions.Add(pos1);
            requiredPositions.Add(pos2);
        }

        return true;
    }

    private Vector3Int GetPerpendicularDirection(Directions direction)
    {
        return (direction == Directions.Back || direction == Directions.Forward) 
            ? Vector3Int.right 
            : new Vector3Int(0, 0, 1);
    }

    private bool IsPositionAvailable(Vector3Int position, Dictionary<Vector3Int, Directions> buildablePlots, 
        HashSet<Vector3Int> occupiedPositions)
    {
        return buildablePlots.ContainsKey(position) && !occupiedPositions.Contains(position);
    }

    private GameObject SpawnBuilding(GameObject prefab, Vector3Int position, Quaternion rotation)
    {
        buildingCount++;
        return Instantiate(prefab, position, rotation, transform);
    }

    private Dictionary<Vector3Int, Directions> FindBuildablePlots(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Directions> plots = new Dictionary<Vector3Int, Directions>();
        HashSet<Vector3Int> roadSet = new HashSet<Vector3Int>(roadPositions);

        foreach (var position in roadPositions)
        {
            var neighborDirections = Placement.GetNeighborDirections(position, roadSet);
            
            foreach (Directions direction in Enum.GetValues(typeof(Directions)))
            {
                if (!neighborDirections.Contains(direction))
                {
                    Vector3Int plotPosition = position + Placement.GetOffset(direction);
                    
                    if (!plots.ContainsKey(plotPosition))
                    {
                        plots[plotPosition] = Placement.GetOppositeDirection(direction);
                    }
                }
            }
        }

        return plots;
    }

    private Quaternion GetRotationForDirection(Directions direction)
    {
        switch (direction)
        {
            case Directions.Forward: return Quaternion.Euler(0, 90, 0);
            case Directions.Back: return Quaternion.Euler(0, -90, 0);
            case Directions.Right: return Quaternion.Euler(0, 180, 0);
            default: return Quaternion.identity;
        }
    }

    private bool CanPlaceBuilding(ChoooseHouseHelper buildingType)
    {
        return buildingType.howMuchCanBePlaced == -1 || buildingType.CanPlaceBuilding();
    }

    private bool ShouldSkipPlacement()
    {
        return randomPlacement && UnityEngine.Random.value < skipPlacementProbability;
    }

    public void Reset()
    {
        foreach (var building in structureDictionary.Values)
        {
            if (building != null) Destroy(building);
        }
        
        structureDictionary.Clear();
        buildingCount = 0;

        foreach (var buildingType in buildingTypes)
        {
            buildingType.Reset();
        }
    }

    public int GetBuildingCount() => buildingCount;
}
