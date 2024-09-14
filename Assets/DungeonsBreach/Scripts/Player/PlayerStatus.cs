using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public struct PlayerStatus
{
    public int maxHP;
    public int hp;
    public float defence;

    public PlayerStatus(int max, int hp, float def)
    {
        this.maxHP = max;
        this.hp = hp;
        this.defence = def;
    }

    public static PlayerStatus operator +(PlayerStatus status1, PlayerStatus status2)
    {
        return new PlayerStatus(
            status1.maxHP + status2.maxHP,
            status1.hp + status2.hp,
            status1.defence + status2.defence
        );
    }

    public static PlayerStatus operator -(PlayerStatus status1, PlayerStatus status2)
    {
        return new PlayerStatus(
            status1.maxHP - status2.maxHP,
            status1.hp - status2.hp,
            status1.defence - status2.defence
        );
    }


}



public struct PlayerStatusModifier
{
    public PlayerStatus statusDelta;


    public static PlayerStatus operator +(PlayerStatus status, PlayerStatusModifier modifier)
    {
        return status + modifier.statusDelta;
    }

        public static PlayerStatus operator -(PlayerStatus status, PlayerStatusModifier modifier)
    {
        return status - modifier.statusDelta;
    }
}
