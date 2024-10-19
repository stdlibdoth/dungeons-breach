using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
