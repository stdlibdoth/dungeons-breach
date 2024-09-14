using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthPoint : MonoBehaviour
{
    [SerializeField] private Image m_image;
    [SerializeField] private Sprite m_empty;
    [SerializeField] private Sprite m_full;




    public void SetHP(bool is_full)
    {
        m_image.sprite = is_full?m_full:m_empty;
    }
}
