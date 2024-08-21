using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private IsoGrid m_grid;
    [SerializeField] private IsoTileBase m_tilePrefab;
    [SerializeField] private IsoTileBase m_blockPrefab;
    [SerializeField] private IsoTileBase m_pathPrefab;



    [SerializeField] private IsoGridCoord m_start;
    [SerializeField] private IsoGridCoord m_end;


    [SerializeField] private PathFindingAgent m_agent;

    protected override void Awake()
    {
        base.Awake();
        m_grid = new IsoGrid(m_grid.Dimension, m_grid.CellSize, Unity.Mathematics.float2.zero);
        GridManager.ActiveGrid = m_grid;
    }


    private void Start()
    {
        PopulateGridMask();
        GenenrateGrid();
        FindPath();
        StartCoroutine(m_agent.MoveAgent(m_end));
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(pos.ToIsoCoordinate(m_grid));
        }

    }



    private void GenenrateGrid()
    {
        for (int y = 0; y < m_grid.Dimension.y; y++)
        {
            for (int x = 0; x < m_grid.Dimension.x; x++)
            {
                IsoGridCoord coord = new IsoGridCoord(x, y);
                var tileMask = m_grid.PathFindingTileMask(coord);
                IsoTileBase prefab = tileMask.value != (byte)PathFindingTile.Land?m_blockPrefab:m_tilePrefab;
                var tile = Instantiate(prefab, coord.ToWorldPosition(m_grid), Quaternion.identity);
                tile.SetCoord(coord);
            }
        }
    }


    private void PopulateGridMask()
    {
        var block = new PathFindingMask { value = PathFindingMask.landBlocking };
        var land = new PathFindingMask { value = (byte)PathFindingTile.Land };

        for (int y = 0; y < m_grid.Dimension.y; y++)
        {
            for (int x = 0; x < m_grid.Dimension.x; x++)
            {
                var coord = new IsoGridCoord(x, y);
                if(y == 5 && x<=7 && x>=2)
                {
                    m_grid.UpdatePathFindingMask(coord, block);
                }
                else
                {
                    m_grid.UpdatePathFindingMask(coord, land);
                }

            }
        }

    }


    private void FindPath()
    {
        var mask = new PathFindingMask { value = PathFindingMask.landBlocking };
        if (IsoGridPathFinding.FindPathAstar(m_start, m_end, m_grid, mask, out var path))
        {
            for(int i = 0;i<path.Count;i++)
            {
                var coord = path[i];
                var tile = Instantiate(m_pathPrefab, coord.ToWorldPosition(m_grid), Quaternion.identity);
                tile.SetCoord(coord);
            }
        }
    }
}
