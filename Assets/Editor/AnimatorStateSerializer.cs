using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;

[CustomEditor(typeof(SerializedAnimatorStates))]
public class SerializedAnimatorStatesEditor : Editor
{
    private List<string> m_animatorStates;
    private Animator m_prevAnimator = null;

    private void Reset()
    {
        m_animatorStates = new List<string>();
    }

    public bool TryGetAnimatorStates(Animator animator)
    {
        AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
        if (ac == null)
            ac = ((animator.runtimeAnimatorController as AnimatorOverrideController).runtimeAnimatorController) as AnimatorController;
        AnimatorControllerLayer[] acLayers = ac.layers;
        List<AnimatorState> allStates = new List<AnimatorState>();
        foreach (AnimatorControllerLayer i in acLayers)
        {
            ChildAnimatorState[] animStates = i.stateMachine.states;
            foreach (ChildAnimatorState j in animStates)
            {
                allStates.Add(j.state);
                m_animatorStates.Add(j.state.name);
            }
        }
        return allStates.Count > 0;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var aniamtorState = serializedObject.FindProperty("m_animatorStates");
        var animator = serializedObject.FindProperty("m_animator").objectReferenceValue as Animator;
        if (animator != m_prevAnimator)
        {
            m_animatorStates = new List<string>();
            if (animator != null && TryGetAnimatorStates(animator))
            {
                aniamtorState.arraySize = m_animatorStates.Count;
                for (int i = 0; i < m_animatorStates.Count; i++)
                {
                    aniamtorState.GetArrayElementAtIndex(i).stringValue = m_animatorStates[i];
                }
            }
            else if (animator == null)
            {
                aniamtorState.ClearArray();
            }
        }
        m_prevAnimator = animator;
        serializedObject.ApplyModifiedProperties();
    }
}
