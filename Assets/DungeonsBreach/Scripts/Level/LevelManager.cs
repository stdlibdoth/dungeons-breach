using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Random = Unity.Mathematics.Random;

public class LevelManager : Singleton<LevelManager>
{
    [Header("unit preafabs")]
    [SerializeField] private List<UnitBase> m_enemyUnitPrefabs;
    [SerializeField] private List<UnitBase> m_playerUnitPrefabs;

    [Header("themes")]
    [SerializeField] private UnitTheme m_unitTheme;


    private List<UnitBase> m_units = new List<UnitBase>();


    protected override void Awake()
    {
        base.Awake();
        m_unitTheme.GetTopic("UnitSpawn").AddListener(OnUnitSpawn);
        m_unitTheme.GetTopic("UnitDie").AddListener(OnUnitDie);

    }


    public static UnitBase[] Units
    {
        get
        {
            return GetSingleton().m_units.ToArray();
        }
    }

    public static IReadOnlyDictionary<IsoGridCoord,UnitBase> UnitCoordinates
    {
        get
        {
            var result = new Dictionary<IsoGridCoord,UnitBase>();
            foreach (var unit in GetSingleton().m_units)
            {
                result[unit.PathAgent.Coordinate] = unit;
            }
            return result;
        }
    }

    public static bool TryGetUnits(IsoGridCoord coord, out List<UnitBase> units)
    {
        units = new List<UnitBase>();
        foreach (var u in GetSingleton().m_units)
        {
            if (u.PathAgent.Coordinate == coord)
            {
                units.Add(u);
            }
        }
        return units.Count > 0;
    }


    private void OnUnitSpawn(UnitBase unit)
    {
        GetSingleton().m_units.Add(unit);
    }
    private void OnUnitDie(UnitBase unit)
    {
        GetSingleton().m_units.Remove(unit);
    }
}
