using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SpawnerModule : ActionModule
{
    protected List<UnitDamageAction> m_tempDamagePreview = new List<UnitDamageAction>();

    [Header("Spawn unit")]
    [SerializeField] private List<UnitBase> m_spawnUnit;

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
        var coord = m_actionParam.unit.PathAgent.Coordinate;


        if(LevelManager.TryGetUnits(coord,out var units))
        {
            ActionTileInfo info = ActionTileInfo.Self;
            foreach (var unit in units)
            {
                await unit.Damage(info).ExcuteAction();
                await UniTask.WaitForSeconds(0.3f);
            }
            ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemySpawn).RegistorAction(this);
        }
        else
        {
            int index = Random.Range(0, m_spawnUnit.Count);
            Vector3 position = IsoGridMetrics.ToWorldPosition(coord, GridManager.ActivePathGrid);
            Instantiate(m_spawnUnit[index], position, Quaternion.identity);
            m_actionParam.unit.Die();
            await ActionTurn.ExcuteTempActions();
        }
        await UniTask.Yield();
    }

    public override IPreviewable<ActionModuleParam> GeneratePreview(ActionModuleParam data)
    {
        PreviewKey = new PreviewKey(this);
        m_actionParam = data;
        return this;
    }

    public override ActionTileInfo[] StartPreview()
    {
        ActionTileInfo info = ActionTileInfo.Self;
        var coord = m_actionParam.unit.PathAgent.Coordinate;
        List<ActionTileInfo> previewInfo = new List<ActionTileInfo>{ info };

        if (LevelManager.TryGetUnits(coord, out var units))
        {
            foreach (var unit in units)
            {
                var damageAction = unit.Damage(info);
                damageAction.PreviewKey = PreviewKey;
                m_tempDamagePreview.Add(damageAction);
                previewInfo.AddRange(damageAction.StartPreview());
            }
        }
        return previewInfo.ToArray();
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
