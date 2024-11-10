using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

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


public class ActionComparer : Comparer<IAction>
{
    public override int Compare(IAction x, IAction y)
    {
        return (x.Priority - y.Priority).value;
    }
}

public interface IAction
{
    public ActionPriority Priority {  get; set; }
    public IAction Build<T>(T param) where T : IActionParam;
    public UniTask ExcuteAction();
}

public interface IActionParam
{

}


public delegate IEnumerator MoveAgentDelegate(LocamotionType locamotion_type, IsoGridCoord target);