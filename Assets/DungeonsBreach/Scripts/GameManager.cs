using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : Singleton<GameManager>
{

    [SerializeField] private IsoGridCoord m_start;
    [SerializeField] private IsoGridCoord m_end;

    [SerializeField] private PlayerStatus m_playerStatus;


    public PlayerStatus PlayerStatus
    {
        get { return m_playerStatus; }
        set { m_playerStatus = value; }
    }


    private void Start()
    {

    }


    public static Coroutine DispachCoroutine(IEnumerator coroutine)
    {
        return GetSingleton().StartCoroutine(coroutine);
    }
}
