using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpawnModule : ActionModule
{
    private ActionModuleParam m_actionParam;
    [SerializeField] private UnitBase m_spawnUnit;
    [SerializeField] private SpawnAnchor m_spawnAnchor;
    [SerializeField] private ActionTileProfile m_profile;
    [SerializeField] private Animator m_animator;
    [SerializeField] private int m_spawnFrameDelay;
    [SerializeField] private string m_animateTrigger;


    public override ActionPriority Priority { get; set; }
    public override ActionTileProfile ActionTileProfile { get { return m_profile; } }

    public override IAction Build<T>(T param)
    {
        m_actionParam = param as ActionModuleParam;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        var unit = m_actionParam.unit;
        Debug.Log(unit + " spawn " + m_spawnUnit.name);
        m_animator?.SetTrigger(m_animateTrigger);
        m_animator?.SetFloat("DirBlend", (int)unit.Agent.Direction);
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
}
