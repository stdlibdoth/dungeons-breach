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


    private void Start()
    {
    }


    public void GenenratePathGrid()
    {
        var pathGrid = ActivePathGrid;
        for (int y = 0; y < pathGrid.Dimension.y; y++)
        {
            for (int x = 0; x < pathGrid.Dimension.x; x++)
            {
                IsoGridCoord coord = new IsoGridCoord(x, y);
                if (pathGrid.PathFindingTileMask(coord).value != (byte)PathFindingTile.Land)
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
        var land = new PathFindingMask { value = (byte)PathFindingTile.Land };
        var pathGrid = ActivePathGrid;

        for (int y = 0; y < pathGrid.Dimension.y; y++)
        {
            for (int x = 0; x < pathGrid.Dimension.x; x++)
            {
                var coord = new IsoGridCoord(x, y);
                if (y == 5 && x <= 6 && x >= 2)
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
