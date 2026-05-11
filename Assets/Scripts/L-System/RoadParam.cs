using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadParam : MonoBehaviour
{
    public Action OnRoadPlacementComplete;

    [SerializeField] private GameObject straightPrefab, cornerPrefab, threewayPrefab, fourwayPrefab, endPrefab;
    
    private Dictionary<Vector3Int, GameObject> roadDictionary = new Dictionary<Vector3Int, GameObject>();
    private HashSet<Vector3Int> positionsToUpdate = new HashSet<Vector3Int>();
    
    private int roadSegmentCount = 0;

    // Rotation lookup tables
    private static readonly Dictionary<Directions, Quaternion> EndRotations = new Dictionary<Directions, Quaternion>
    {
        { Directions.Right, Quaternion.identity },
        { Directions.Back, Quaternion.Euler(0, 90, 0) },
        { Directions.Left, Quaternion.Euler(0, 180, 0) },
        { Directions.Forward, Quaternion.Euler(0, -90, 0) }
    };

    public List<Vector3Int> GetRoadPositions() => roadDictionary.Keys.ToList();

    public IEnumerator PlaceRoadSegment(Vector3 startPosition, Vector3 direction, int length)
    {
        Vector3Int normalizedDirection = Vector3Int.RoundToInt(direction.normalized);
        Quaternion rotation = (normalizedDirection.x == 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

        for (int i = 0; i < length; i++)
        {
            Vector3Int position = Vector3Int.RoundToInt(startPosition) + normalizedDirection * i;

            if (roadDictionary.ContainsKey(position))
            {
                positionsToUpdate.Add(position);
                continue;
            }

            GameObject road = Instantiate(straightPrefab, position, rotation, transform);
            roadSegmentCount++;
            roadDictionary[position] = road;

            // Mark endpoints for updates
            if (i < 5 || i >= length - 5)
            {
                positionsToUpdate.Add(position);
            }

            yield return null;
        }

        OnRoadPlacementComplete?.Invoke();
    }

    public void UpdateRoadConnections()
    {
        foreach (var position in positionsToUpdate)
        {
            UpdateRoadAtPosition(position);
        }
    }

    private void UpdateRoadAtPosition(Vector3Int position)
    {
        List<Directions> neighbors = Placement.GetNeighborDirections(position, roadDictionary.Keys);
        
        if (roadDictionary[position] != null)
        {
            Destroy(roadDictionary[position]);
        }

        GameObject prefab = GetRoadPrefab(neighbors, out Quaternion rotation);
        roadDictionary[position] = Instantiate(prefab, position, rotation, transform);
        roadSegmentCount++;
    }

    private GameObject GetRoadPrefab(List<Directions> neighbors, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        int count = neighbors.Count;

        switch (count)
        {
            case 1:
                rotation = EndRotations[neighbors[0]];
                return endPrefab;

            case 2:
                if (IsOpposite(neighbors[0], neighbors[1]))
                {
                    rotation = (neighbors.Contains(Directions.Forward) || neighbors.Contains(Directions.Back)) 
                        ? Quaternion.Euler(0, 90, 0) 
                        : Quaternion.identity;
                    return straightPrefab;
                }
                rotation = GetCornerRotation(neighbors);
                return cornerPrefab;

            case 3:
                rotation = GetThreewayRotation(neighbors);
                return threewayPrefab;

            case 4:
                return fourwayPrefab;

            default:
                return straightPrefab;
        }
    }

    private bool IsOpposite(Directions dir1, Directions dir2)
    {
        return Placement.GetOppositeDirection(dir1) == dir2;
    }

    private Quaternion GetCornerRotation(List<Directions> neighbors)
    {
        if (neighbors.Contains(Directions.Forward) && neighbors.Contains(Directions.Right))
            return Quaternion.Euler(0, 90, 0);
        if (neighbors.Contains(Directions.Right) && neighbors.Contains(Directions.Back))
            return Quaternion.Euler(0, 180, 0);
        if (neighbors.Contains(Directions.Left) && neighbors.Contains(Directions.Back))
            return Quaternion.Euler(0, -90, 0);
        return Quaternion.identity;
    }

    private Quaternion GetThreewayRotation(List<Directions> neighbors)
    {
        if (neighbors.Contains(Directions.Right) && neighbors.Contains(Directions.Back) && neighbors.Contains(Directions.Left))
            return Quaternion.Euler(0, 90, 0);
        if (neighbors.Contains(Directions.Left) && neighbors.Contains(Directions.Back) && neighbors.Contains(Directions.Forward))
            return Quaternion.Euler(0, 180, 0);
        if (neighbors.Contains(Directions.Left) && neighbors.Contains(Directions.Forward) && neighbors.Contains(Directions.Right))
            return Quaternion.Euler(0, -90, 0);
        return Quaternion.identity;
    }

    public void Reset()
    {
        foreach (var road in roadDictionary.Values)
        {
            if (road != null) Destroy(road);
        }
        roadDictionary.Clear();
        positionsToUpdate.Clear();
        roadSegmentCount = 0;
    }

    public int GetRoadSegmentCount() => roadSegmentCount;
}
