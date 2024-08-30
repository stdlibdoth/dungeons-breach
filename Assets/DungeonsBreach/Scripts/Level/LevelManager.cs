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

    private List<UnitBase> m_units = new List<UnitBase>();



    //private void Start()
    //{
    //    foreach (var item in m_units)
    //    {
            
    //    }
    //}


    public static UnitBase[] Units
    {
        get
        {
            return GetSingleton().m_units.ToArray();
        }
    }

    public static bool TryGetUnit(IsoGridCoord coord, out UnitBase unit)
    {
        unit = null;
        foreach (var u in GetSingleton().m_units)
        {
            if (u.Agent.Coordinate == coord)
            {
                unit = u;
                return true;
            }
        }
        return false;
    }


    public static void AddUnit(UnitBase unit)
    {
        GetSingleton().m_units.Add(unit);
    }



    public static void SpawnUnits()
    {
        var seed = (uint)DateTime.Now.Ticks;
        var singleton = GetSingleton();
        Debug.Log(seed);
        var range = singleton.m_enemyUnitPrefabs.Count;
        if(range>0)
        {
            for (int i = 0; i < 3; i++)
            {
                Random rand = new Random(seed);
                var index = rand.NextInt(range);
                var enemy = Instantiate(singleton.m_enemyUnitPrefabs[index]);
                GetSingleton().m_units.Add(enemy);
            }
        }

        foreach (var prefab in singleton.m_playerUnitPrefabs)
        {
            var unit = Instantiate(prefab);
            unit.gameObject.transform.position = Vector3.zero;
            GetSingleton().m_units.Add(unit);
        }
    }


    public static void RemoveUnit<T>(T unit) where T : UnitBase
    {
        GetSingleton().m_units.Remove(unit);
    }
}
