using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ObstacleUnit : UnitBase
{

    [SerializeField] protected List<Sprite> m_damageSprites;
    private float m_damageIndexResolution;

    public override void UpdateStatus(UnitStatus delta_status)
    {
        base.UpdateStatus(delta_status);
        int index = (int)math.round(m_damageIndexResolution*m_unitStatus.hp);
        m_spriteRenderer.sprite = m_damageSprites[index];
    }

    public override UnitSpawnAction Spawn(IsoGridCoord coord)
    {
        m_damageIndexResolution = (m_damageSprites.Count -1)/(float)m_unitStatus.maxHP;
        return new UnitSpawnAction();
    }

}
