using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private IsoGrid m_grid;


    [SerializeField] private IsoTileBase m_tilePrefab;
    [SerializeField] private IsoTileBase m_blockPrefab;
    [SerializeField] private IsoTileBase m_pathPrefab;


    public static PathGrid ActivePathGrid { get; set; }
    public static TileGrid ActiveTileGrid { get; set; }


    protected override void Awake()
    {
        base.Awake();
        ActivePathGrid = new PathGrid(m_grid.Dimension, m_grid.CellSize, float2.zero);
        ActiveTileGrid = new TileGrid(m_grid.Dimension, m_grid.CellSize, float2.zero);
        PopulatePathGridMask();
        GenenratePathGrid();
    }

    public void GenenratePathGrid()
    {
        var pathGrid = ActivePathGrid;
        for (int y = 0; y < pathGrid.Dimension.y; y++)
        {
            for (int x = 0; x < pathGrid.Dimension.x; x++)
            {
                IsoGridCoord coord = new IsoGridCoord(x, y);
                if (pathGrid.PathingMaskSingleTile(coord).value != (byte)PathingMaskBit.None)
                {
                    var tile = Instantiate(m_blockPrefab, coord.ToWorldPosition(pathGrid), Quaternion.identity);
                    tile.SetCoord(coord);
                }
            }
        }
    }


    private void PopulatePathGridMask()
    {
        var block = new PathFindingMask { value = PathFindingMask.landBlocking };
        var land = new PathFindingMask { value = (byte)PathingMaskBit.None };
        var pathGrid = ActivePathGrid;

        for (int y = 0; y < pathGrid.Dimension.y; y++)
        {
            for (int x = 0; x < pathGrid.Dimension.x; x++)
            {
                var coord = new IsoGridCoord(x, y);
                if (x == pathGrid.Dimension.x-1 || y == pathGrid.Dimension.y-1)
                {
                    pathGrid.UpdatePathFindingMask(coord, block);
                }
                else
                {
                    pathGrid.UpdatePathFindingMask(coord, land);
                }

            }
        }
    }
}
