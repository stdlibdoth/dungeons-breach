using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;




[System.Serializable]
public class IsoGrid
{
    [SerializeField] protected int2 m_dimension;
    [SerializeField] protected float m_cellSize;
    [SerializeField] protected float2 m_originOffset;


    public int2 Dimension {  get { return m_dimension; } }
    public float CellSize {  get { return m_cellSize; } }
    public float2 Offset {  get { return m_originOffset; } }

    public int2 Center
    {
        get
        {
            return m_dimension / 2;
        }
    }

    public IsoGrid(int2 dimension, float cell_size, float2 offset)
    {
        m_dimension = dimension;
        m_cellSize = cell_size;
        m_originOffset = offset;
    }

    public bool CheckRange(IsoGridCoord coord)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < Dimension.x && coord.y < Dimension.y;
    }


    public IsoGridCoord[] SurroundingCoordsWithDummy(IsoGridCoord center)
    {
        var adjacent = new IsoGridCoord[IsoGridMetrics.directionCount];
        for (int i = 0; i < adjacent.Length; i++)
        {
            var coord = center + IsoGridMetrics.GridDirectionToCoord[i];
            adjacent[i] = CheckRange(coord) ? coord : new IsoGridCoord(-1, -1);
        }
        return adjacent;
    }

    public IsoGridCoord[] SurroundingCoords(IsoGridCoord center)
    {
        List<IsoGridCoord> adjacent = new List<IsoGridCoord>();
        for (int i = 0; i < IsoGridMetrics.directionCount; i++)
        {
            var coord = center + IsoGridMetrics.GridDirectionToCoord[i];
            if (CheckRange(coord))
                adjacent.Add(coord);
        }
        return adjacent.ToArray();
    }
}

[System.Serializable]
public struct IsoGridCoord
{
    public int x;
    public int y;

    public static IsoGridCoord Zero
    {
        get { return new IsoGridCoord(0, 0); }
    }

    public static int Distance(IsoGridCoord coord1, IsoGridCoord coord2)
    {
        return math.abs(coord1.x - coord2.x) + math.abs(coord1.y - coord2.y);
    }


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

    public static IsoGridCoord operator *(int lhs, IsoGridCoord rhs)
    {
        return new IsoGridCoord(lhs*rhs.x, lhs*rhs.y);
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

[System.Serializable]
public enum IsoGridDirection
{
    SE,
    SW,
    NW,
    NE,
}