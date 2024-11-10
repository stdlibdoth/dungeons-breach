using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SpawnerUnit : UnitBase
{

    [Header("Spawner Module")]
    [SerializeField] private SpawnerModule m_spawnerMoudle;

    protected override async void SpawnUnit()
    {
        m_pathAgent.Init();
        m_unitStatus = PreviewAppliedModuleStatus();
        m_unitStatus.hp = m_unitStatus.maxHP;
        m_unitStatus.moves = m_unitStatus.moveRange;
        var action = Spawn(m_pathAgent.Coordinate);
        await action.ExcuteAction();
        ResetActions();
    }

    public override UnitSpawnAction Spawn(IsoGridCoord coord)
    {
        var action = new UnitSpawnAction();
        var param = new SpawnActionParam
        {
            onSpawn = OnSpawn,
            unit = this,
            enableNotification = false,
        };
        action.Build(param);
        return action;
    }

    private async UniTask OnSpawn()
    {
        await UnitSpawnAnimation();
        ActionModuleParam param = new ActionModuleParam(this, new IsoGridCoord[] { IsoGridCoord.Zero }, true);
        m_spawnerMoudle.Build(param);
        ActionTurn.CreateOrGetActionTurn(ActionTurnType.EnemySpawn).RegistorAction(m_spawnerMoudle);
    }
}
