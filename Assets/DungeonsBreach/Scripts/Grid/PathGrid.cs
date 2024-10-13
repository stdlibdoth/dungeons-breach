using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class PathGrid : IsoGrid
{
    private PathFindingMask[] m_pathFindingMask;
    public PathGrid(int2 dimension, float cell_size, float2 offset) :base(dimension, cell_size, offset)
    {
        m_pathFindingMask = new PathFindingMask[m_dimension.x * m_dimension.y];
    }

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
    public PathFindingMask PathingMaskSingleTile(IsoGridCoord coord)
    {
        return m_pathFindingMask[coord.To2DArrayIndex(m_dimension)];
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

    public void ResetMaskBits(IsoGridCoord coord, PathFindingMask reset_mask)
    {
        var mask = m_pathFindingMask[coord.To2DArrayIndex(m_dimension)];
        m_pathFindingMask[coord.To2DArrayIndex(m_dimension)] = mask & (~reset_mask);
    }

    public void SetMaskBits(IsoGridCoord coord, PathFindingMask set_mask)
    {
        var mask = m_pathFindingMask[coord.To2DArrayIndex(m_dimension)];
        m_pathFindingMask[coord.To2DArrayIndex(m_dimension)] = mask | set_mask;
    }


    public int MaskLineCast(PathFindingMask mask,IsoGridCoord start ,IsoGridDirection dir, int max_dist, out IsoGridCoord coord)
    {
        coord = start;
        for (int i = 0; i < max_dist; i++)
        {
            var temp = start + i*IsoGridMetrics.GridDirectionToCoord[(int)dir];
            if (!CheckRange(temp))
            {
                coord = start + (i-1)*IsoGridMetrics.GridDirectionToCoord[(int)dir];;
                return i-1;
            }

            var tileMask = PathingMaskSingleTile(temp);
            coord = temp;
            if (tileMask.CheckMaskOverlap(mask))
            {
                return i;
            }
        }
        return int.MaxValue;
    }
}


[System.Serializable]
public struct PathFindingMask
{
    public byte value;

    public const byte landBlocking = 0b00001111;
    public const byte airBlocking = 0b00000111;
    
    public PathFindingMask(byte value)
    {
        this.value = value;
    }

    public static PathFindingMask operator | (PathFindingMask lhs, PathFindingMask rhs)
    {
        return new PathFindingMask((byte)(lhs.value | rhs.value));
    }

    public static PathFindingMask operator ^(PathFindingMask lhs, PathFindingMask rhs)
    {
        return new PathFindingMask((byte)(lhs.value ^ rhs.value));
    }

    public static PathFindingMask operator &(PathFindingMask lhs, PathFindingMask rhs)
    {
        return new PathFindingMask((byte)(lhs.value & rhs.value));
    }

    public static PathFindingMask operator ~(PathFindingMask mask)
    {
        return new PathFindingMask((byte)(~mask.value));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="other_mask"></param>
    /// <returns>returns true if two masks have overlaped bits</returns>
    public bool CheckMaskOverlap(PathFindingMask other_mask)
    {
        return (value & other_mask.value) != 0;
    }
}

[SerializeField]
public enum PathingMaskBit : byte
{
    None = 0,
    Obstacle = 0b1,
    Ally = 0b10,
    Enemy = 0b100,
    Hole = 0b1000,
    Water = 0b10000,
    Trap = 0b100000,
}