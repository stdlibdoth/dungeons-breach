using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public struct ActionPriority
{
    public int value;


    public ActionPriority(int value)
    {
        this.value = value;
    }

    public bool Equals(ActionPriority obj)
    {
        return value == obj.value;
    }

    public override bool Equals(object obj)
    {
        return obj is ActionPriority other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static ActionPriority operator +(ActionPriority lhs, ActionPriority rhs)
    {
        return new ActionPriority(lhs.value + rhs.value);
    }

    public static ActionPriority operator -(ActionPriority lhs, ActionPriority rhs)
    {
        return new ActionPriority(lhs.value - rhs.value);
    }

    public static bool operator ==(ActionPriority lhs, ActionPriority rhs)
    {
        return lhs.value == rhs.value;
    }

    public static bool operator !=(ActionPriority lhs, ActionPriority rhs)
    {
        return !(lhs.value == rhs.value);
    }
}


public interface IAction
{
    public ActionPriority Priority {  get; set; }
    public IAction Build<T>(T param) where T : IActionParam;
    public IEnumerator ExcuteAction();
}

public interface IActionParam
{

}

public class AttackActionParam :IActionParam
{
    public AttackProfile profile;
    public Animator animator;
    public UnitBase unit;
}

public class DamageActionParam:IActionParam
{
    public AttackTileInfo attackInfo;
    public Animator animator;
    public UnitBase unit;
}

public class MoveActionParam:IActionParam
{
    public LocamotionType locamotion;
    public IsoGridCoord target;
    public MoveAgentDelegate moveAgentDelegate;
}

public delegate IEnumerator MoveAgentDelegate(LocamotionType locamotion_type, IsoGridCoord target);
public enum PlayBackMode
{
    Instant,
    Temp,
    EndOfTurn
}