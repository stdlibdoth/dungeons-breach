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
    public PathFindingMask PathFindingTileMask(IsoGridCoord coord)
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
}
