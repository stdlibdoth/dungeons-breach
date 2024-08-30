using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionComparer : Comparer<IAction>
{
    public override int Compare(IAction x, IAction y)
    {
        return (x.Priority - y.Priority).value;
    }
}
public class GameManager : Singleton<GameManager>
{

    [SerializeField] private IsoGridCoord m_start;
    [SerializeField] private IsoGridCoord m_end;



    private void Start()
    {
        //PopulateGridMask();
        //GenenrateGrid();
        //FindPath();
    }




    //private void FindPath()
    //{
    //    var mask = new PathFindingMask { value = PathFindingMask.landBlocking };
    //    if (IsoGridPathFinding.FindPathAstar(m_start, m_end, m_grid, mask, out var path))
    //    {
    //        for(int i = 0;i<path.Count;i++)
    //        {
    //            var coord = path[i];
    //            var tile = Instantiate(m_pathPrefab, coord.ToWorldPosition(m_grid), Quaternion.identity);
    //            tile.SetCoord(coord);
    //        }
    //    }
    //}

    public static Coroutine DispachCoroutine(IEnumerator coroutine)
    {
        return GetSingleton().StartCoroutine(coroutine);
    }
}
