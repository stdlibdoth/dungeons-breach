using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectileSpawnModule : ActionModule
{
    private ActionModuleParam m_actionParam;
    [Space]
    [SerializeField] private TrajectileUnit m_spawnUnit;
    [SerializeField] private SpawnAnchor m_spawnAnchor;
    [SerializeField] private ActionTileProfile m_profile;
    [SerializeField] private int m_spawnFrameDelay;


    public override ActionPriority Priority { get; set; }
    public override ActionTileProfile ActionTileProfile { get { return m_profile; } }

    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
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

        var pos = (Vector3)unit.Agent.Coordinate.ToWorldPosition(grid) + relativePos;
        var dir = unit.Agent.Coordinate.DirectionTo(m_actionParam.confirmedCoord[0], grid);
        var spawn = Instantiate(m_spawnUnit, pos, Quaternion.identity);
        spawn.SetTargets(m_actionParam.confirmedCoord);
        spawn.SetDirection(dir);

        yield return null;
    }


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
