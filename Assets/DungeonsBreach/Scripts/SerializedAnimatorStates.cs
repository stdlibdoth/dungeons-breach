using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializedAnimatorStates : MonoBehaviour
{

    [SerializeField] private List<string> m_animatorStates;
    [SerializeField] private Animator m_animator;

    public void Init(List<string> states)
    {
        m_animatorStates = new List<string>();
        for (int i = 0; i < states.Count; i++)
        {
            m_animatorStates.Add(states[i]);
        }
    }

    public bool TryGetAnimatorState(string state)
    {
        foreach (var name in m_animatorStates)
        {
            if(name == state)
            {
                return true;
            }
        }
        return false;
    }
}
