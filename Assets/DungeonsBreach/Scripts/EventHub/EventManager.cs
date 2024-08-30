using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    [Header("Editor References")]
    [SerializeField] private List<ThemeBase> m_editorThemeRefs;

    private static EventManager m_singleton;
    private Dictionary<string,ThemeBase> m_themes;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    private void Init()
    {
        m_singleton = GetSingleton();
        m_themes = new Dictionary<string, ThemeBase>();
        for (int i = 0; i < m_editorThemeRefs.Count; i++)
        {
            m_themes[m_editorThemeRefs[i].Name] = m_editorThemeRefs[i];
        }
    }

    private static T NewTheme<T>(string theme_name) where T : ThemeBase
    {
        GameObject obj = new GameObject(theme_name);
        obj.transform.SetParent(m_singleton.transform);
        T theme = obj.AddComponent<T>();
        theme.Init(theme_name);
        m_singleton.m_themes.Add(theme_name, theme);
#if UNITY_EDITOR
        m_singleton.m_editorThemeRefs.Add(theme);
#endif
        return theme;
    }

    public static T GetTheme<T>(string theme_name) where T: ThemeBase
    {
        if (m_singleton.m_themes.ContainsKey(theme_name))
            return (T)m_singleton.m_themes[theme_name];
        else
            return NewTheme<T>(theme_name);
    }

    public static void RemoveTheme(string theme_name)
    {
        m_singleton.m_themes.Remove(theme_name);
#if UNITY_EDITOR
        m_singleton.m_editorThemeRefs.RemoveAll(x=>x.Name== theme_name);
#endif
    }

    public static void Excute(EventInfo info)
    {
        if (!m_singleton.m_themes.ContainsKey(info.theme))
            return;
        ((Theme)m_singleton.m_themes[info.theme]).GetTopic(info.topic).Invoke();
    }
    public static void Excute<T0>(EventInfo info, T0 data)
    {
        if (!m_singleton.m_themes.ContainsKey(info.theme))
            return;
        ((Theme<T0>)m_singleton.m_themes[info.theme]).GetTopic(info.topic).Invoke(data);
    }
    public static void Excute<T0,T1>(EventInfo info, T0 d0, T1 d1)
    {
        if (!m_singleton.m_themes.ContainsKey(info.theme))
            return;
        ((Theme<T0,T1>)m_singleton.m_themes[info.theme]).GetTopic(info.topic).Invoke(d0,d1);
    }

    public static void Excute<T0, T1, T2>(EventInfo info, T0 d0, T1 d1, T2 d2)
    {
        if (!m_singleton.m_themes.ContainsKey(info.theme))
            return;
        ((Theme<T0, T1, T2>)m_singleton.m_themes[info.theme]).GetTopic(info.topic).Invoke(d0, d1, d2);
    }
}
