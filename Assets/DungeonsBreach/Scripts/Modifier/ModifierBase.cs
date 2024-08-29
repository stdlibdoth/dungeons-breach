using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModifierBase : MonoBehaviour
{
    [SerializeField] private UnitStatusBase unitParamBase;


    public virtual T Modify<T>(T param) where T: UnitStatusBase
    {
        if(!unitParamBase.attack.IsEmpty())
            param.attack = unitParamBase.attack;
        param.maxHP += unitParamBase.maxHP;
        param.moveRange += unitParamBase.moveRange;
        param.defense += unitParamBase.defense;
        param.hp += unitParamBase.hp;
        return param;
    }
}
