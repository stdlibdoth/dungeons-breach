using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionModule : Module, IAction
{
    public bool Actived { get;set; }
    public abstract ActionTileProfile ActionTileProfile { get; }

    public abstract ActionPriority Priority { get; set; }
    public abstract IEnumerator ExcuteAction();
    public abstract IAction Build<T>(T param) where T : IActionParam;
}
