using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawnModule : BasicSpawnModule
{

    [Header("UI")]
    [SerializeField]private ProjectilePreviewer m_projectilePreviewer;



    protected List<IsoGridCoord> m_actionTargets = new List<IsoGridCoord>();
    protected List<UnitDamageAction> m_tempDamagePreview = new List<UnitDamageAction>();

    protected override IsoGridCoord[] ActionRangeInternal(IsoGridCoord center, IsoGridDirection dir)
    {
        m_actionTargets.Clear();
        List<IsoGridCoord> range = new List<IsoGridCoord>();
        var pUnit = m_spawnUnit as ProjectileUnit;
        if(pUnit == null)
            range.ToArray();
        
        int dist = pUnit.TravelRange;
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
                m_actionTargets.Add(stopCoord);
            }
        }
        return range.ToArray();
    }


    public override IsoGridCoord[] ConfirmActionTargets()
    {
        ActionRangeInternal(m_actionParam.unit.Agent.Coordinate, m_actionParam.unit.Agent.Direction);
        m_confirmedActionRange = m_actionTargets.ToArray();
        return m_actionTargets.ToArray();
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;

        Debug.Log(unit + " projectile " + m_spawnUnit.name);

        List<IsoGridCoord> spawnTile = new List<IsoGridCoord>();
        foreach (var tileInfo in m_profile.data)
        {
            var c = tileInfo.relativeCoord.OnRelativeTo(unit.Agent.Coordinate, unit.Agent.Direction);
            foreach (var confirmed in m_confirmedActionRange)
            {
                if(confirmed.x == c.x || confirmed.y == c.y)
                    spawnTile.Add(c);
            }
        }
        if(spawnTile.Count==0)
            yield break;

        PlayAnimation(unit);
        Debug.Log(unit.Agent.Direction);
        Vector3 relativePos = m_spawnAnchor.GetAnchor(unit.Agent.Direction).localPosition;
        var grid = GridManager.ActivePathGrid;
        yield return new WaitForSeconds(Time.fixedDeltaTime * m_spawnFrameDelay);
        foreach (var coord in spawnTile)
        {
            var pos = (Vector3)coord.ToWorldPosition(grid) + relativePos;
            var dir = unit.Agent.Coordinate.DirectionTo(coord, grid);
            var spawn = Instantiate(m_spawnUnit, pos, Quaternion.identity);
            var pUnit = spawn as ProjectileUnit;
            pUnit.PreviewKey = PreviewKey;
            spawn.SetDirection(dir);
            yield return pUnit.StartProjectile();
        }
    }



    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        PreviewKey = new PreviewKey(this);
        //Debug.Log("generate key: " + PreviewKey.GetHashCode() + "  " + gameObject.name);
        m_actionParam = data;
        return this;
    }

    public override IEnumerator StartPreview()
    {
        var pUnit = m_spawnUnit as ProjectileUnit;
        var center = m_actionParam.unit.Agent.Coordinate;
        var dir = m_actionParam.unit.Agent.Direction;
        var grid = GridManager.ActivePathGrid;

        foreach (var tileInfo in m_profile.data)
        {
            IsoGridCoord startCoord = tileInfo.relativeCoord.OnRelativeTo(center, dir);
            IsoGridCoord end = startCoord;
            if(grid.CheckRange(startCoord))
            {
                GridManager.ActivePathGrid.MaskLineCast(pUnit.Agent.BlockingMask, startCoord, dir, pUnit.TravelRange, out end);
                Vector3 endPos = end.ToWorldPosition(grid);
                Vector3 startPos = m_spawnAnchor.GetAnchor(dir).localPosition + (Vector3)startCoord.ToWorldPosition(grid);
                bool isPlayer = m_actionParam.unit.CompareTag("PlayerUnit");
                m_projectilePreviewer.SetPreviewer(startPos,endPos,isPlayer,PreviewKey);
                foreach (var actionTileInfo in pUnit.ActionModule.Profile.data)
                {
                    IsoGridCoord coord = actionTileInfo.relativeCoord.OnRelativeTo(end, dir);
                    var info = actionTileInfo;
                    info.pushDir = info.pushDir.RotateRelativeTo(m_actionParam.unit.Agent.Direction);
                    if(grid.CheckRange(coord))
                    {
                        LevelManager.TryGetUnits(end, out var hits);
                        foreach (var hit in hits)
                        {
                            var action = hit.Damage(info);
                            action.PreviewKey = PreviewKey;
                            m_tempDamagePreview.Add(action);
                            yield return action.StartPreview();
                        }
                    }
                }
            }
        }
        yield return null;
    }

    public override void StopPreview()
    {
        foreach (var item in m_tempDamagePreview)
        {
            item.StopPreview();
        }
        m_tempDamagePreview.Clear();
    }
}
