using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitDamageAction : IAction ,IPreviewable<UnitDamagePreviewData>
{
    public ActionPriority Priority { get; set; }

    public PreviewKey PreviewKey{get; set;}
    protected DamageActionParam m_damageActionParam;
    protected UnitDamagePreviewData m_previewData;

    private static List<UnitDamageAction> m_damagePreviewCache = new List<UnitDamageAction>();

    private Sequence m_animationSeq;

    public UnitDamageAction()
    {
        PreviewKey = new PreviewKey(this);
    }


    public SelfDamageAction ToSelfDamageAction()
    {
        SelfDamageAction action = new SelfDamageAction();
        action.PreviewKey = this.PreviewKey;
        action.Build(m_damageActionParam);
        action.GeneratePreview(m_previewData);
        return action;
    }


    #region IAction

    public virtual IEnumerator ExcuteAction()
    {
        if(m_damageActionParam == null)
            yield break;

        if(m_damageActionParam.animationAction!= null)
            GameManager.DispachCoroutine(m_damageActionParam.animationAction.Invoke());


        var unit = m_damageActionParam.unit;
        var attackInfo = m_damageActionParam.attackInfo;

        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -attackInfo.value;
        unit.UpdateStatus(deltaStatus);

        if(attackInfo.pushDist > 0 && !unit.IsStationary)
        {
            var targetTile = unit.PathAgent.Coordinate + attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)attackInfo.pushDir];
            if (LevelManager.TryGetUnits(targetTile, out var hits))
            {
                var temp = ActionTileInfo.Default;
                temp.value = 1;
                float stopDist = GridManager.ActivePathGrid.CellSize / 1.5f;
                Vector3 pos = unit.PathAgent.Coordinate.ToWorldPosition(GridManager.ActivePathGrid);
                Debug.Log("block unit" + hits[0]);
                yield return unit.StartCoroutine(unit.PathAgent.AnimateAgent(LocamotionType.Shift, targetTile, stopDist));
                yield return unit.StartCoroutine(unit.PathAgent.AnimateAgent(LocamotionType.Shift, pos, 3));
                deltaStatus.hp = -1;
                unit.UpdateStatus(deltaStatus);
                foreach (var hit in hits)
                {
                    yield return hit.Damage(temp).ExcuteAction();
                }
            }
            else if(GridManager.ActiveTileGrid.CheckRange(targetTile))
            {

                var action = unit.Move(attackInfo.pushType, targetTile, true,false);
                yield return action.ExcuteAction();
            }
        }   
        yield return ActionTurn.ExcuteTempActions();
    }

    public virtual IAction Build<T>(T p) where T : IActionParam
    {
        Priority = new ActionPriority{value = 0};
        m_damageActionParam = p as DamageActionParam;
        return this;
    }
    #endregion


    #region IPreviewable


    public void ResetPreviewKey()
    {
        foreach (var item in m_damagePreviewCache)
        {
            item.PreviewKey.UpdateKey(item);
        }
    }

    public virtual IPreviewable<UnitDamagePreviewData> GeneratePreview(UnitDamagePreviewData data)
    {
        PreviewKey = new PreviewKey(this);
        m_previewData = data;
        return this;
    }

    public virtual ActionTileInfo[] StartPreview()
    {
        if (m_previewData == null)
            return new ActionTileInfo[] { };

        var unit = m_damageActionParam.unit;
        var attackInfo = m_damageActionParam.attackInfo;
        //Preview animation
        m_previewData.unitHealthBar.StartDamangeAnimation(-1);
        StartPreviewAnimation();

        //Preview health
        m_previewData.unitHealthBar.SetPreview(0,-attackInfo.value);
        m_damagePreviewCache.Add(this);


        List<ActionTileInfo> actionTileInfo = new List<ActionTileInfo> { attackInfo };
        //Preview recursive damage actions
        if(attackInfo.pushDist > 0)
        {
            var targetTile = unit.PathAgent.Coordinate + attackInfo.pushDist * IsoGridMetrics.GridDirectionToCoord[(int)attackInfo.pushDir];

            if(unit.IsStationary)
            {
                var shiftPreviewData = new ActionPreviewerData("ShiftUnavailablePreview",
                attackInfo.pushDir,unit.PathAgent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(shiftPreviewData,PreviewKey);
            }
            else if (LevelManager.TryGetUnits(targetTile, out var hits))
            {
                var temp = ActionTileInfo.Default;
                m_previewData.unitHealthBar.SetPreview(0,-1);
                //UIPreview
                var hitPreviewData0 = new ActionPreviewerData("HitPreview",
                attackInfo.pushDir.Opposite(),unit.PathAgent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(hitPreviewData0,PreviewKey);

                var shiftPreviewData = new ActionPreviewerData("ShiftUnavailablePreview",
                attackInfo.pushDir,unit.PathAgent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(shiftPreviewData,PreviewKey);
                foreach (var hit in hits)
                {
                    IPreviewable<UnitDamagePreviewData> preview = hit.Damage(temp);
                    var hitPreviewData1 = new ActionPreviewerData("HitPreview",
                    attackInfo.pushDir,targetTile);
                    BattleUIController.ActionPreviewer.RegistorPreview(hitPreviewData1,PreviewKey);
                    actionTileInfo.AddRange(preview.StartPreview());
                }
            }
            else if(GridManager.ActiveTileGrid.CheckRange(targetTile))
            {
                var shiftPreviewData = new ActionPreviewerData("ShiftAvailablePreview",
                attackInfo.pushDir,unit.PathAgent.Coordinate);
                BattleUIController.ActionPreviewer.RegistorPreview(shiftPreviewData,PreviewKey);


                //preview unit shift position
                var moveAction = unit.Move(attackInfo.pushType, targetTile, true, false);
                actionTileInfo.AddRange(moveAction.StartPreview());
                //unit.PathAgent.StartMovePreview(targetTile);
            }
        }
        return actionTileInfo.ToArray();
    }

    public void StopPreview()
    {
        foreach (var item in m_damagePreviewCache)
        {
            item.m_previewData.unitHealthBar.ResetPreview();
            item.m_previewData.spriteRenderer.color = Color.white;
            item.m_animationSeq.Kill();
        }
        m_damagePreviewCache.Clear();

        var previewKey = new PreviewKey(m_damageActionParam.unit.PathAgent);
        MoveAction.StopPreview(previewKey);
    }

    #endregion

    private void StartPreviewAnimation()
    {
        m_animationSeq = DOTween.Sequence();
        m_animationSeq.Append(m_previewData.spriteRenderer.DOFade(0.1f,0.2f))
        .Append(m_previewData.spriteRenderer.DOFade(1f,0.2f))
        .SetLoops(-1);
    }
}


public class DamageActionParam:IActionParam
{
    public ActionTileInfo attackInfo;
    public CoroutineDelegate animationAction;
    public UnitBase unit;
}


public class SelfDamageAction:UnitDamageAction
{

    public override IAction Build<T>(T p)
    {
        Priority = new ActionPriority{value = -1};
        m_damageActionParam = p as DamageActionParam;
        return this;
    }

    public override IEnumerator ExcuteAction()
    {
        if(m_damageActionParam.animationAction!= null)
            yield return m_damageActionParam.animationAction.Invoke();

        if(PreviewKey == this)
            BattleUIController.ActionPreviewer.ClearPreview(PreviewKey);
        var deltaStatus = UnitStatus.Empty;
        deltaStatus.hp = -m_damageActionParam.attackInfo.value;
        m_damageActionParam.unit.UpdateStatus(deltaStatus);
        yield return null;
    }
}
