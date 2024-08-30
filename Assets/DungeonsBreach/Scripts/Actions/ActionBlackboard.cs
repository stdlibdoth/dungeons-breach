using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActionBlackboard :MonoBehaviour
{
    private List<IAction> m_actions = new List<IAction>();

    public void AddAction(IAction action)
    {
        m_actions.Add(action);
    }


    public IEnumerator ExcuteActions()
    {
        SortActions();
        for (int i = 0; i < m_actions.Count; i++)
        {
            Debug.Log(i);
            yield return StartCoroutine(m_actions[i].ExcuteAction());
        }
        m_actions.Clear();
    }

    private void SortActions()
    {
        m_actions.Sort(new ActionComparer());
    }
}
