using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CursorData",menuName = "ScriptableObjects/CursorData",order =1)]
public class CursorDataSO : ScriptableObject
{
    public List<CursorData> m_data;
}
