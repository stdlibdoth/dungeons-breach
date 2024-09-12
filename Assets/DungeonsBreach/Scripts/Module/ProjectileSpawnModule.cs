using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawnModule : BasicSpawnModule
{
    public override IsoGridCoord[] ActionRange(IsoGridCoord center, IsoGridDirection dir)
    {
        List<IsoGridCoord> range = new List<IsoGridCoord>();
        var pUnit = m_spawnUnit as ProjectileUnit;
        if(pUnit == null)
            range.ToArray();
        
        int dist = pUnit.TravelRange;
        Debug.Log(dist);
        foreach (var tileInfo in m_profile.data)
        {
            IsoGridCoord startCoord = tileInfo.relativeCoord.OnRelativeTo(center, dir);
            var grid = GridManager.ActivePathGrid;
            if(grid.CheckRange(startCoord))
            {
                int blockDist = GridManager.ActivePathGrid.MaskLineCast(pUnit.Agent.BlockingMask, startCoord, dir, dist, out var stopCoord);
                for (int i = 0; i <= blockDist; i++)
                {
                    range.Add(startCoord + i*IsoGridMetrics.GridDirectionToCoord[(int)dir]);
                }
            }
        }
        return range.ToArray();
    }
}
