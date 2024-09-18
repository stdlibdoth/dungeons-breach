using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpawnModule : ActionModule
{
    protected ActionModuleParam m_actionParam;
    [Space]
    [SerializeField] protected UnitBase m_spawnUnit;
    [SerializeField] protected SpawnAnchor m_spawnAnchor;

    [SerializeField] protected int m_spawnFrameDelay;

    public override ActionPriority Priority { get; set; }


    #region IAction
    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        UnitStatus deltaStatus = UnitStatus.Empty;
        deltaStatus.moves = -m_actionParam.unit.MovesAvalaible;
        m_actionParam.unit.UpdateStatus(deltaStatus);
        Actived = false;
        IsAvailable = false;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + " spawn " + m_spawnUnit.name);
        PlayAnimation(unit);
        Vector3 relativePos = m_spawnAnchor.GetAnchor(unit.Agent.Direction).localPosition;
        var grid = GridManager.ActivePathGrid;
        yield return new WaitForSeconds(Time.fixedDeltaTime * m_spawnFrameDelay);
        foreach (var coord in m_actionParam.confirmedCoord)
        {
            var pos = (Vector3)coord.ToWorldPosition(grid) + relativePos;
            var dir = unit.Agent.Coordinate.DirectionTo(coord, grid);
            var spawn = Instantiate(m_spawnUnit, pos, Quaternion.identity);
            spawn.SetDirection(dir);
        }
        yield return null;
    }
    #endregion


    #region IPreviewable

    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator StartPreview()
    {
        throw new System.NotImplementedException();
    }

    public override void StopPreview()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    protected void PlayAnimation(UnitBase unit)
    {
        if(m_animationDataOverride)
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
