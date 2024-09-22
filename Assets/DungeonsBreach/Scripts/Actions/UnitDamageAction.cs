using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDamageAction : IAction ,IPreviewable<UnitDamagePreviewData>
{
    public ActionPriority Priority { get; set; }

    protected DamageActionParam m_damageActionParam;
    protected UnitDamagePreviewData m_previewData;

    private static List<UnitDamageAction> m_damagePreviewCache = new List<UnitDamageAction>();


    #region IAction

    public virtual IEnumerator ExcuteAction()
    {
        var animationData = m_damageActionParam.animationStateData;
        animationData.PlayAnimation();

        var unit = m_damageActionParam.unit;
        var attackInfo = m_damageActionParam.attackInfo;

        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -attackInfo.value;
        unit.UpdateStatus(deltaStatus);


        //for testing player status only, remove later
        if(unit.CompareTag("PlayerUnit"))
        {
            GameManager.UpdatePlayerStatus(new PlayerStatus{
                maxHP = 0,
                hp = -attackInfo.value,
                defence = 0,
            });
        }

        if(attackInfo.pushDist > 0)
        {
            var targetTile = unit.Agent.Coordinate + attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)attackInfo.pushDir];
            if (LevelManager.TryGetUnits(targetTile, out var hits))
            {
                var temp = ActionTileInfo.Default;
                float stopDist = GridManager.ActivePathGrid.CellSize / 2;
                Vector3 pos = unit.Agent.Coordinate.ToWorldPosition(GridManager.ActivePathGrid);
                yield return unit.StartCoroutine(unit.Agent.AnimateAgent(LocamotionType.Shift, targetTile, stopDist));
                yield return unit.StartCoroutine(unit.Agent.AnimateAgent(LocamotionType.Shift, pos, 3));
                deltaStatus.hp = -1;
                unit.UpdateStatus(deltaStatus);
                foreach (var hit in hits)
                {
                    BattleManager.RegistorAction(hit.Damage(temp),PlayBackMode.Instant);
                }
            }
            else if(GridManager.ActiveTileGrid.CheckRange(targetTile))
            {
                var action = unit.Move(attackInfo.pushType, targetTile, false);
                BattleManager.RegistorAction(action,PlayBackMode.Instant);
            }
        }
        yield return null;
    }

    public virtual IAction Build<T>(T p) where T : IActionParam
    {
        m_damageActionParam = p as DamageActionParam;
        return this;
    }
    #endregion


    #region IPreviewable

    public IPreviewable<UnitDamagePreviewData> GeneratePreview(UnitDamagePreviewData data)
    {
        m_previewData = data;
        return this;
    }

    public IEnumerator StartPreview()
    {
        var unit = m_damageActionParam.unit;
        var attackInfo = m_damageActionParam.attackInfo;
                
        //Preview animation
        foreach (var animationData in m_previewData.animationData)
        {
            animationData.PlayAnimation(true);
        }

        m_previewData.unitHealthBar.SetPreview(0,-attackInfo.value);
        if(attackInfo.pushDist > 0)
        {
            var targetTile = unit.Agent.Coordinate + attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)attackInfo.pushDir];
            if (LevelManager.TryGetUnits(targetTile, out var hits))
            {
                var temp = ActionTileInfo.Default;
                m_previewData.unitHealthBar.SetPreview(0,-1);
                //UIPreview
                var hitPreviewData0 = new ActionPreviewerData("HitPreview",
                attackInfo.pushDir.Opposite(),unit.Agent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(hitPreviewData0);

                var shiftPreviewData = new ActionPreviewerData("ShiftUnavailablePreview",
                attackInfo.pushDir,unit.Agent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(shiftPreviewData);
                foreach (var hit in hits)
                {
                    IPreviewable<UnitDamagePreviewData> preview = hit.Damage(temp);
                    var hitPreviewData1 = new ActionPreviewerData("HitPreview",
                    attackInfo.pushDir,targetTile);
                    BattleUIController.ActionPreviewer.RegistorPreview(hitPreviewData1);
                    yield return preview.StartPreview();
                }
            }
            else if(GridManager.ActiveTileGrid.CheckRange(targetTile))
            {
                var shiftPreviewData = new ActionPreviewerData("ShiftAvailablePreview",
                attackInfo.pushDir,unit.Agent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(shiftPreviewData);
                
                //preview unit shift position


            }
        }
        m_damagePreviewCache.Add(this);
        yield return null;
    }

    public void StopPreview()
    {
        foreach (var item in m_damagePreviewCache)
        {
            item.m_previewData.unitHealthBar.ResetPreview();
            foreach (var animationData in item.m_previewData.animationData)
            {
                animationData.StopAnimation();
            }        
        }
        m_damagePreviewCache.Clear();
    }

    #endregion
}



public class DamageActionParam:IActionParam
{
    public ActionTileInfo attackInfo;
    public AnimationStateData animationStateData;
    public UnitBase unit;
}



public class SelfDamageAction:UnitDamageAction
{
    public override IEnumerator ExcuteAction()
    {
        m_damageActionParam.animationStateData.PlayAnimation();
        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -1;
        m_damageActionParam.unit.UpdateStatus(deltaStatus);
        yield return null;
    }
}
