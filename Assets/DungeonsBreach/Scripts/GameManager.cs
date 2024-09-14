using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : Singleton<GameManager>
{

    [SerializeField] private IsoGridCoord m_start;
    [SerializeField] private IsoGridCoord m_end;

    [Space]
    [Header("Player status")]
    [SerializeField] private PlayerStatus m_playerStatus;
    [SerializeField] private PlayerStatusBar m_PlayerStatusBar;



    private void Start() 
    {
        GetSingleton().m_PlayerStatusBar.SetHealthBar(GetSingleton().m_playerStatus);
    }

    public static PlayerStatus PlayerStatus
    {
        get { return GetSingleton().m_playerStatus; }
    }

    public static void UpdatePlayerStatus(PlayerStatus delta)
    {
        GetSingleton().m_playerStatus += delta;
        GetSingleton().m_PlayerStatusBar.SetHealthBar(GetSingleton().m_playerStatus);
    }

    public static Coroutine DispachCoroutine(IEnumerator coroutine)
    {
        return GetSingleton().StartCoroutine(coroutine);
    }
}
