using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using R3;

public class R3Test : MonoBehaviour
{

    void Start()
    {
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Subscribe(ob => Debug.Log(Time.time)).AddTo(this);
    }
}