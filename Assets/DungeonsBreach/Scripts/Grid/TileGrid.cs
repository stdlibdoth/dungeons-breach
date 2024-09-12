using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class TileGrid : IsoGrid
{
    private IsoTileBase[] m_tiles;

    public TileGrid(int2 dimension, float cell_size, float2 offset) : base(dimension, cell_size, offset)
    {
        m_tiles = new IsoTileBase[m_dimension.x * m_dimension.y];
    }



    public IsoTileBase Tile(IsoGridCoord coord)
    {
        var index = coord.To2DArrayIndex(m_dimension);
        return m_tiles[index];
    }


    public void InitTiles(IsoTileBase[] tiles)
    {
        Debug.Assert(tiles.Length == m_dimension.x * m_dimension.y);
        m_tiles = tiles;
    }
}


[System.Serializable]
public struct TileMask
{
    public uint value;


    public TileMask(uint value)
    {
        this.value = value;
    }

    public static TileMask operator | (TileMask lhs, TileMask rhs)
    {
        return new TileMask((byte)(lhs.value | rhs.value));
    }

    public static TileMask operator & (TileMask lhs, TileMask rhs)
    {
        return new TileMask((byte)(lhs.value & rhs.value));
    }

    public static TileMask operator ^(TileMask lhs, TileMask rhs)
    {
        return new TileMask((byte)(lhs.value ^ rhs.value));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="other_mask"></param>
    /// <returns>returns true if two masks have overlaped bits</returns>
    public bool CheckMaskOverlap(TileMask other_mask)
    {
        return (value & other_mask.value) != 0;
    }

}