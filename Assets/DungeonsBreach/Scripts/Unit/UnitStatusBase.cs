using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitStatusBase
{
    public int maxHP;
    public int hp;
    public AttackProfile attack;
    public int defense;
    public int moveRange;
    public int moves;
    public int attackPoints;


    public static UnitStatusBase Empty
    {
        get
        {
            return new UnitStatusBase
            {
                attack = AttackProfile.Empty,
                defense = 0,
                maxHP = 0,
                hp = 0,
                moveRange = 0,
                moves = 0,
                attackPoints = 0,
            };
        }
    }


    public void Update(UnitStatusBase other)
    {
        if (!other.attack.IsEmpty())
            attack = other.attack;
        maxHP += other.maxHP;
        hp += (other.hp - other.defense);
        defense += other.defense;
        moveRange += other.moveRange;
        moves += other.moves;
        attackPoints += other.attackPoints;
    }
}
