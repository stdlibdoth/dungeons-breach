using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UnitStatus
{
    public int maxHP;
    public int hp;
    public int defense;
    public int moveRange;
    public int moves;


    public static UnitStatus Empty
    {
        get
        {
            return new UnitStatus
            {
                maxHP = 0,
                hp = 0,
                defense = 0,
                moveRange = 0,
                moves = 0,
            };
        }
    }

    public static UnitStatus Action
    {
        get
        {
            return new UnitStatus
            {
                maxHP = 0,
                hp = 0,
                defense = 0,
                moveRange = 0,
                moves = 0,
            };
        }
    }


    public static UnitStatus operator +(UnitStatus lhs, UnitStatus rhs)
    {
        return new UnitStatus
        {
            maxHP = lhs.maxHP + rhs.maxHP,
            hp = lhs.hp + rhs.hp,
            defense = lhs.defense + rhs.defense,
            moveRange = lhs.moveRange + rhs.moveRange,
            moves = lhs.moves + rhs.moves,
        };
    }

    public static UnitStatus operator -(UnitStatus lhs, UnitStatus rhs)
    {
        return new UnitStatus
        {
            maxHP = lhs.maxHP - rhs.maxHP,
            hp = lhs.hp - rhs.hp,
            defense = lhs.defense - rhs.defense,
            moveRange = lhs.moveRange - rhs.moveRange,
            moves = lhs.moves - rhs.moves,
        };
    }
}
