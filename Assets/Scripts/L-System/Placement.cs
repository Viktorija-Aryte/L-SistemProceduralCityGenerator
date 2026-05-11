using System.Collections.Generic;
using UnityEngine;

public static class Placement 
{
    // Cache direction mappings for better performance
    private static readonly Dictionary<Directions, Vector3Int> DirectionOffsets = new Dictionary<Directions, Vector3Int>
    {
        { Directions.Right, Vector3Int.right },
        { Directions.Left, Vector3Int.left },
        { Directions.Forward, Vector3Int.forward },
        { Directions.Back, Vector3Int.back }
    };

    private static readonly Dictionary<Directions, Directions> OppositeDirections = new Dictionary<Directions, Directions>
    {
        { Directions.Forward, Directions.Back },
        { Directions.Back, Directions.Forward },
        { Directions.Left, Directions.Right },
        { Directions.Right, Directions.Left }
    };

    public static List<Directions> GetNeighborDirections(Vector3Int origin, ICollection<Vector3Int> tiles)
    {
        List<Directions> neighbors = new List<Directions>(4);
        
        foreach (var direction in DirectionOffsets)
        {
            if (tiles.Contains(origin + direction.Value))
            {
                neighbors.Add(direction.Key);
            }
        }
        
        return neighbors;
    }

    public static Vector3Int GetOffset(Directions direction)
    {
        if (DirectionOffsets.TryGetValue(direction, out Vector3Int offset))
        {
            return offset;
        }
        throw new System.ArgumentException($"Invalid direction: {direction}");
    }

    public static Directions GetOppositeDirection(Directions direction)
    {
        if (OppositeDirections.TryGetValue(direction, out Directions opposite))
        {
            return opposite;
        }
        throw new System.ArgumentException($"Invalid direction: {direction}");
    }
}