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
