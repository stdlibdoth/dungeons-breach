using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PreviewKey : IComparable<PreviewKey>, IEquatable<PreviewKey>
{
    private static int m_count = 0;

    private static PreviewKey m_globalKey = new PreviewKey();

    private int m_id;
    private object m_keyObj;

    public static PreviewKey GlobalKey{
        get{return m_globalKey;}
    }

    public PreviewKey()
    {
        m_keyObj = new object();
        m_id = m_count;
        m_count++;
    }

    public PreviewKey(object obj)
    {
        m_keyObj = obj;
        m_id = m_count;
        m_count++;
    }

    // public static bool IsNull(PreviewKey key)
    // {
    //     return key.Equals(null);
    // }
    public void UpdateKey(object obj)
    {
        m_keyObj = obj;
    }

    public int CompareTo(PreviewKey other)
    {
        return m_id - other.m_id;
    }

    public bool Equals(PreviewKey other)
    {
        return m_keyObj == other.m_keyObj;
    }

    public override bool Equals(object obj)
    {
        return m_keyObj.Equals(obj);
    }

    public override int GetHashCode()
    {
        return m_keyObj.GetHashCode();
    }

    public static bool operator == (PreviewKey key1, PreviewKey key2)
    {
        return key1.m_keyObj.Equals(key2.m_keyObj);
    }

    public static bool operator != (PreviewKey key1, PreviewKey key2)
    {
        return !key1.m_keyObj.Equals(key2.m_keyObj);
    }
    public static bool operator == (PreviewKey key1, object objKey)
    {
        return key1.m_keyObj.Equals(objKey);
    }

    public static bool operator != (PreviewKey key1, object objKey)
    {
        return !key1.m_keyObj.Equals(objKey);
    }
}
