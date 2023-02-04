using System.Collections;
using UnityEngine;

public class GameUtils
{
    private static Vector2 NegateYVector = new Vector2(1.0f, -1.0f);
    private static int MaxSortingObjects = 128;

    public static Vector2 ToUnityCoordinates(Vector2 position)
    {
        return (position / 100.0f) * NegateYVector;
    }
    
    public static int ToUnitySortingPosition(int gameSortingPostion)
    {
        return MaxSortingObjects - gameSortingPostion;
    }
}