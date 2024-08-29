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
public struct AttackTileInfo
{
    public IsoGridDirection pushDir;
    public int pushDist;
    public LocamotionType pushType;
    public int value;
    public IsoGridCoord relativeCoord;


    public static AttackTileInfo Default
    {
        get
        {
            return new AttackTileInfo
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
public class AttackProfile
{
    public AttackTileInfo[] data;
    public static AttackProfile Empty
    {
        get
        {
            return new AttackProfile
            {
                data = new AttackTileInfo[]
                {
                }
            };
        }
    }

    public static AttackProfile Default
    {
        get
        {
            return new AttackProfile
            {
                data = new AttackTileInfo[]
                {
                    AttackTileInfo.Default,
                }
            };
        }
    }

    public bool IsEmpty()
    {
        return data.Length == 0;
    }
}