using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : IAction,IPreviewable<MoveActionParam>
{
    private static Dictionary<PreviewKey, MoveAction> m_previews = new Dictionary<PreviewKey, MoveAction>();
    private MoveActionParam m_param;

    public static void StopPreview(PreviewKey preview_key)
    {
        if (m_previews.ContainsKey(preview_key))
        {
            var moveAction = m_previews[preview_key];
            moveAction.m_param.agent.StopMovePreview();
            BattleUIController.ActionPreviewer.ClearPreview(preview_key);
            //m_previews[preview_key].StopPreview();
        }
        m_previews.Remove(preview_key);
    }


    public ActionPriority Priority { get; set; }
    public PreviewKey PreviewKey { get; set; }



    public IAction Build<T>(T p) where T : IActionParam
    {
        m_param = p as MoveActionParam;
        return this;
    }

    public IEnumerator ExcuteAction()
    {
        if (m_param.ignorePathing)
        {
            var agent = m_param.agent;
            yield return agent.MoveStraight(m_param.locamotion, m_param.target);

            var grid = GridManager.ActivePathGrid;
            var tileMask = grid.PathingMaskSingleTile(agent.Coordinate);

            //check falling
            if (agent.FallMask.CheckMaskOverlap(tileMask))
            {
                yield return new WaitForSeconds(0.3f);
                Vector3 target = agent.transform.position + Vector3.down * grid.CellSize * 0.5f;
                yield return agent.AnimateAgent(LocamotionType.Shift, target);

                LevelManager.TryGetUnits(agent.Coordinate, out List<UnitBase> units);
                foreach (var unit in units)
                {
                    if(unit.PathAgent == agent)
                    {
                        ActionTileInfo actionTileInfo = ActionTileInfo.Self;
                        actionTileInfo.value = int.MaxValue;
                        var action = unit.Damage(actionTileInfo).ToSelfDamageAction();
                        yield return action.ExcuteAction();
                    }
                }
            }
        }
        else
            yield return m_param.agent.MoveAgent(m_param.locamotion, m_param.target);
    }

    public IPreviewable<MoveActionParam> GeneratePreview(MoveActionParam data)
    {
        m_param = data;
        PreviewKey = new PreviewKey(data.agent);
        return this;
    }

    public ActionTileInfo[] StartPreview()
    {
        var grid = GridManager.ActivePathGrid;
        var agent = m_param.agent;
        var tileMask = grid.PathingMaskSingleTile(m_param.target);
        m_previews[PreviewKey] = this;

        var previewInfo = new List<ActionTileInfo>();
        agent.StartMovePreview(m_param.target, true);

        //check falling
        if (agent.FallMask.CheckMaskOverlap(tileMask))
        {
            LevelManager.TryGetUnits(agent.Coordinate, out List<UnitBase> units);
            foreach (var unit in units)
            {
                if (unit.PathAgent == agent)
                {
                    ActionTileInfo actionTileInfo = ActionTileInfo.Self;
                    actionTileInfo.value = int.MaxValue;
                    var action = unit.Damage(actionTileInfo).ToSelfDamageAction();
                    action.PreviewKey = PreviewKey;
                    var info = action.StartPreview();
                    previewInfo.AddRange(info);
                    var deathPreviewData = new ActionPreviewerData("DeathMark", agent.Direction, agent.Coordinate);
                    BattleUIController.ActionPreviewer.RegistorPreview(deathPreviewData, PreviewKey);
                    break;
                }
            }
        }
        return previewInfo.ToArray();
    }

    public void StopPreview()
    {
        StopPreview(PreviewKey);
    }

}


public class MoveActionParam:IActionParam
{
    public LocamotionType locamotion;
    public IsoGridCoord target;
    public PathFindingAgent agent;
    public bool ignorePathing;
}
