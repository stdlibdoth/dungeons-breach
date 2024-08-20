using UnityEngine;
using Unity.Mathematics;
using System;
using System.Runtime.CompilerServices;



[System.Serializable]
public class IsoGrid
{
    [SerializeField] private int2 m_dimension;
    [SerializeField] private float m_cellSize;
    [SerializeField] private float2 m_originOffset;

    private PathFindingMask[] m_pathFindingMask;

    public int2 Dimension {  get { return m_dimension; } }
    public float CellSize {  get { return m_cellSize; } }
    public float2 Offset {  get { return m_originOffset; } }


    public PathFindingMask[] PathFindingMask
    {
        get
        {
            int l = m_pathFindingMask.Length;
            PathFindingMask[] m = new PathFindingMask[l];
            Array.Copy(m_pathFindingMask, m, l);
            return m;
        }
    }

    public PathFindingMask PathFindingTileMask(IsoGridCoord coord)
    {
        return m_pathFindingMask[coord.To2DArrayIndex(m_dimension)];
    }


    public IsoGrid(int2 dimension, float cell_size, float2 offset)
    {
        m_dimension = dimension;
        m_cellSize = cell_size;
        m_originOffset = offset;
        m_pathFindingMask = new PathFindingMask[m_dimension.x * m_dimension.y];
    }

    public void PopulatePathFindingMask(PathFindingMask[] mask)
    {
        if (mask.Length != m_pathFindingMask.Length)
            throw new ArgumentException("mask length unmatch!");
        m_pathFindingMask = mask;
    }

    public void UpdatePathFindingMask(IsoGridCoord coord, PathFindingMask new_value)
    {
        m_pathFindingMask[coord.To2DArrayIndex(m_dimension)] = new_value;
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


    public static bool operator ==(IsoGridCoord lhs, IsoGridCoord rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }

    public static bool operator !=(IsoGridCoord lhs, IsoGridCoord rhs)
    {
        return !(lhs.x == rhs.x && lhs.y == rhs.y);
    }

    public static IsoGridCoord operator+(IsoGridCoord lhs, IsoGridCoord rhs)
    {
        return new IsoGridCoord(lhs.x + rhs.x, lhs.y + rhs.y);
    }

    public static IsoGridCoord operator -(IsoGridCoord lhs, IsoGridCoord rhs)
    {
        return new IsoGridCoord(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public bool Euqals(IsoGridCoord obj)
    {
        return this == obj;
    }

    public override bool Equals(object obj)
    {
        return obj is IsoGridCoord other && this.Euqals(other);

    }

    public override int GetHashCode()
    {
        return (x,y).GetHashCode();
    }

}


public static class IsoGridMetrics
{
    public const float sinAngle = 0.60876143f;
    public const float cosAngle = 0.79335334f;
    public const int directionCount = 4;

    public static float3 ToWorldPosition(this IsoGridCoord coord, IsoGrid grid)
    {
        float suby = grid.CellSize * (coord.y - (grid.Dimension.y / 2));
        float subx = grid.CellSize * (coord.x - (grid.Dimension.x / 2));

        float x = (-subx * cosAngle) + suby * cosAngle;
        float y = (subx * sinAngle) + suby * sinAngle;
        return new float3(x + grid.Offset.x, y + grid.Offset.y, 0);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int To2DArrayIndex(this IsoGridCoord coord, int2 dimension)
    {
        return coord.y * dimension.x + coord.x;
    }

}
