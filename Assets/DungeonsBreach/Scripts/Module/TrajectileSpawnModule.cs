using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectileSpawnModule : ActionModule
{
    private ActionModuleParam m_actionParam;
    [Space]
    [SerializeField] private TrajectileUnit m_spawnUnit;
    [SerializeField] private SpawnAnchor m_spawnAnchor;
    [SerializeField] private int m_spawnFrameDelay;

    [Header("UI")]
    [SerializeField] private TrajectilePreviewer m_previewer;

    protected List<UnitDamageAction> m_tempDamagePreview = new List<UnitDamageAction>();

    #region IAction

    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        UnitStatus deltaStatus = UnitStatus.Empty;
        deltaStatus.moves = -m_actionParam.unit.MovesAvalaible;
        m_actionParam.unit.UpdateStatus(deltaStatus);
        Actived=  false;
        IsAvailable = false;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + " trajectile " + m_spawnUnit.name);
        PlayAnimation(unit);
        Vector3 relativePos = m_spawnAnchor.GetAnchor(unit.Agent.Direction).localPosition;
        var grid = GridManager.ActivePathGrid;
        yield return new WaitForSeconds(Time.fixedDeltaTime * m_spawnFrameDelay);

        var pos = (Vector3)unit.Agent.Coordinate.ToWorldPosition(grid) + relativePos;
        var dir = unit.Agent.Coordinate.DirectionTo(m_actionParam.confirmedCoord[0], grid);
        var spawn = Instantiate(m_spawnUnit, pos, Quaternion.identity);
        spawn.SetTargets(m_actionParam.confirmedCoord);
        spawn.SetDirection(dir);

        yield return null;
    }


    #endregion

    #region IPreviewable
    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        PreviewKey = UnitBase.NextPreviewKey;
        m_actionParam = data;
        return this;
    }

    public override IEnumerator StartPreview()
    {
        var tUnit = m_spawnUnit;
        var dir = m_actionParam.unit.Agent.Direction;
        IsoGridCoord startCoord = m_actionParam.unit.Agent.Coordinate;
        var grid = GridManager.ActivePathGrid;

        foreach (var confirmedCoord in m_actionParam.confirmedCoord)
        {    
            IsoGridCoord end = confirmedCoord;
            Vector3 endPos = end.ToWorldPosition(grid);
            Vector3 startPos = m_spawnAnchor.GetAnchor(dir).localPosition + (Vector3)startCoord.ToWorldPosition(grid);
            bool isAlly = m_actionParam.unit.CompareTag("PlayerUnit");
            m_previewer.SetPreviewer(startPos,endPos,isAlly);
            foreach (var actionTileInfo in tUnit.ActionModule.Profile.data)
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
                        Debug.Log(PreviewKey.GetHashCode());
                        m_tempDamagePreview.Add(action);
                        yield return action.StartPreview();
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
        m_previewer.ClearPreviewer();
    }
    #endregion


    private void PlayAnimation(UnitBase unit)
    {
        if (m_animationDataOverride)
        {
            m_animationData?.PlayAnimation();
        }
        else
        {
            if (unit.GenerateActionAnimationData("Attack", out var data))
            {
                data?.PlayAnimation();
                data?.animator?.SetFloat("DirBlend", (int)unit.Agent.Direction);
            }
        }
    }
}
