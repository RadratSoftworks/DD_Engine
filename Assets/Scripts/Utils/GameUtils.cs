using System.Collections;
using UnityEngine;

public class GameUtils
{
    private static Vector2 NegateYVector = new Vector2(1.0f, -1.0f);

    public static Vector2 ToUnityCoordinates(Vector2 position)
    {
        return (position / 100.0f) * NegateYVector;
    }

    public static Vector2 ToUnitySize(Vector2 size)
    {
        return (size / 100.0f);
    }
    
    public static int ToUnitySortingPosition(float gameSortingPostion)
    {
        return (int)(-gameSortingPostion);
    }
}