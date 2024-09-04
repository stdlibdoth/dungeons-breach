using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnchor : MonoBehaviour
{
    [SerializeField] private Transform m_anchorSE;
    [SerializeField] private Transform m_anchorSW;
    [SerializeField] private Transform m_anchorNW;
    [SerializeField] private Transform m_anchorNE;


    private Transform[] Anchors
    {
        get
        {
            return new Transform[]
            {
                m_anchorSE,
                m_anchorSW,
                m_anchorNW,
                m_anchorNE,
            };
        }
    }

    public Transform GetAnchor(IsoGridDirection dir)
    {
        return Anchors[(int)dir];
    }


}
