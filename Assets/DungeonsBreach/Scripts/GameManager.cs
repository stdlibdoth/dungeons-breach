using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private IsoGrid m_grid;
    [SerializeField] private IsoTileBase m_tilePrefab; 


    private void Start()
    {
        GenenrateGrid();
    }



    private void GenenrateGrid()
    {

        for (int y = 0; y < m_grid.Dimension.y; y++)
        {
            for (int x = 0; x < m_grid.Dimension.x; x++)
            {
                IsoGridCoord coord = new IsoGridCoord(x, y);
                var tile = Instantiate(m_tilePrefab, coord.ToWorldPosition(m_grid), Quaternion.identity);
                tile.SetCoord(coord);
            }
        }
    }
}
