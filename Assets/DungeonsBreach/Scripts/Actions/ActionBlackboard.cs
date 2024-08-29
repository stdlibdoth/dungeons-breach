using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActionComparer : Comparer<IAction>
{
    public override int Compare(IAction x, IAction y)
    {
        return (x.Priority - y.Priority).value;
    }
}


public class ActionBlackboard :MonoBehaviour
{
    private List<IAction> m_actions;



    public void AddAction(IAction action)
    {
        m_actions.Add(action);
    }


    public IEnumerator ExcuteActions()
    {
        SortActions();
        for (int i = 0; i < m_actions.Count; i++)
        {
            yield return StartCoroutine(m_actions[i].ExcuteAction());
        }
    }

    private void SortActions()
    {
        m_actions.Sort(new ActionComparer());
    }
}
