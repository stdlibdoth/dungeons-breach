using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(Toggle))]
public class ModuleToggle : MonoBehaviour
{
    [SerializeField] private Toggle m_toggle;

    [SerializeField] private Image m_backgroundImage;
    [SerializeField] private Image m_checkmarkImage;


    public Toggle Toggle
    {
        get
        {
            return m_toggle;
        }
    }


    private void Reset()
    {
        m_toggle = GetComponent<Toggle>();
    }


    public void Init(Sprite icon)
    {
        m_toggle.SetIsOnWithoutNotify(false);
        m_backgroundImage.sprite = icon;
        m_checkmarkImage.sprite = icon;
        //m_toggle.group = group;
    }


}
