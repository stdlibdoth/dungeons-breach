using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System;


public enum LocamotionType
{
    Default,
    Instant,
    Shift,
}


[System.Serializable]
public struct ActionTileInfo
{
    public IsoGridDirection pushDir;
    public int pushDist;
    public LocamotionType pushType;
    public int value;
    public IsoGridCoord relativeCoord;


    public static ActionTileInfo Default
    {
        get
        {
            return new ActionTileInfo
            {
                pushDist = 0,
                pushDir = IsoGridDirection.SE,
                pushType = LocamotionType.Shift,
                relativeCoord = IsoGridMetrics.GridDirectionToCoord[(int)IsoGridDirection.SE],
                value = 1
            };
        }
    }

    //public AttackTileInfo Copy(AttackTileInfo info)
    //{
    //    return new AttackTileInfo
    //    {
    //        damageMoveType =new Type(),
    //    }
    //}
}

[System.Serializable]
public class ActionTileProfile
{
    public ActionTileInfo[] data;

    public IsoGridCoord[] Coordinates
    {
        get
        {
            IsoGridCoord[] coord = new IsoGridCoord[data.Length];
            for (int i = 0; i < coord.Length; i++)
            {
                coord[i] = data[i].relativeCoord;
            }
            return coord;
        }
    }

    public static ActionTileProfile Empty
    {
        get
        {
            return new ActionTileProfile
            {
                data = new ActionTileInfo[]
                {
                }
            };
        }
    }

    public static ActionTileProfile Default
    {
        get
        {
            return new ActionTileProfile
            {
                data = new ActionTileInfo[]
                {
                    ActionTileInfo.Default,
                }
            };
        }
    }

    public bool IsEmpty()
    {
        return data.Length == 0;
    }
}