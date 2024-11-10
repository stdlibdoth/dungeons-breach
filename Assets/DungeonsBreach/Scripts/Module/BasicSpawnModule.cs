using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BasicSpawnModule : ActionModule
{
    [Space]
    [SerializeField] protected UnitBase m_spawnUnit;
    [SerializeField] protected SpawnAnchor m_spawnAnchor;

    [SerializeField] protected int m_spawnFrameDelay;


    #region IAction
    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        UnitStatus deltaStatus = UnitStatus.Empty;
        deltaStatus.moves = -m_actionParam.unit.MovesAvalaible;
        m_actionParam.unit.UpdateStatus(deltaStatus);
        return this;
    }

    public override async UniTask ExcuteAction()
    {
        if(m_confirmedActionRange.Length<=0)
            return;

        var unit = m_actionParam.unit;
        Debug.Log(unit + " spawn " + m_spawnUnit.name);
        PlayAnimation(unit);
        Vector3 relativePos = m_spawnAnchor.GetAnchor(unit.PathAgent.Direction).localPosition;
        var grid = GridManager.ActivePathGrid;
        await UniTask.WaitForSeconds(Time.fixedDeltaTime * m_spawnFrameDelay);
        foreach (var coord in m_confirmedActionRange)
        {
            var pos = (Vector3)coord.ToWorldPosition(grid) + relativePos;
            var dir = unit.PathAgent.Coordinate.DirectionTo(coord, grid);
            var spawn = Instantiate(m_spawnUnit, pos, Quaternion.identity);
            spawn.SetDirection(dir);
        }
        EventManager.GetTheme<ActionModuleTheme>("ActionModuleTheme").GetTopic("OnModuleExecute").Invoke(this);
        await UniTask.Yield();
    }
    #endregion


    #region IPreviewable

    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        throw new System.NotImplementedException();
    }

    public override ActionTileInfo[] StartPreview()
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
                data?.animator?.SetFloat("DirBlend", (int)unit.PathAgent.Direction);
            }
        }
    }
}
