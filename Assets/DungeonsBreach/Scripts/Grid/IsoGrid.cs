using UnityEngine;
using Unity.Mathematics;


public enum IsoGridDirection
{
    NE,
    SE,
    SW,
    NW,
}

[System.Serializable]
public class IsoGrid
{
    [SerializeField] private int2 m_dimension;
    [SerializeField] private float m_cellSize;
    [SerializeField] private float2 m_originOffset;


    public int2 Dimension {  get { return m_dimension; } }
    public float CellSize {  get { return m_cellSize; } }
    public float2 Offset {  get { return m_originOffset; } }


    public IsoGrid(int2 dimension, float cell_size, float2 offset)
    {
        m_dimension = dimension;
        m_cellSize = cell_size;
        m_originOffset = offset;
    }
}

[System.Serializable]
public struct IsoGridCoord
{
    public int x;
    public int y;


    public IsoGridCoord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return x + "," + y;
    }

}


public static class IsoGridMetrics
{
    public const float sinAngle = 0.60876143f;
    public const float cosAngle = 0.79335334f;

    public static float3 ToWorldPosition(this IsoGridCoord coord, IsoGrid grid)
    {
        float suby = grid.CellSize * (coord.y - (grid.Dimension.y / 2));
        float subx = grid.CellSize * (coord.x - (grid.Dimension.x / 2));

        float x = (-subx * cosAngle) + suby * cosAngle;
        float y = (subx * sinAngle) + suby * sinAngle;
        return new float3(x + grid.Offset.x, y + grid.Offset.y, 0);
    }
}
