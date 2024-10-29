using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using System.Linq;


public class UnitStatusPanel : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private ModuleToggle m_prefab;
    [SerializeField] private Image m_portrait;
    [SerializeField] private Transform m_moduleHolder;


    [Header("Data")]
    [SerializeField] private UnitUIDataSO m_unitUIDataSO;
    [SerializeField] private ModuleUIDataSO m_moduleUIDataSO;


    private ObjectPool<ModuleToggle> m_togglePool;
    private Dictionary<string,ModuleToggle> m_toggles = new Dictionary<string,ModuleToggle>();
    private Dictionary<string,bool> m_prevToggleStatus = new Dictionary<string,bool>();
    private Dictionary<string,ActionModule> m_modules = new Dictionary<string, ActionModule>();

    private UnitBase m_prevSelection;

    private void Awake()
    {
        m_togglePool = new ObjectPool<ModuleToggle>(CreateFunction, OnGet, OnRelease, DestroyFunction, true, 3, 6);      
        m_portrait.gameObject.SetActive(false);
    }

    private void Start()
    {
        var theme = EventManager.GetTheme<UnitTheme>("UnitTheme");
        theme.GetTopic("SelectedUnitChange").AddListener(OnSelectedUnitChange);
    }


    private void PopulateUnitUIInfo(UnitBase unit)
    {
        var unitUIData = m_unitUIDataSO.GetData(unit.UnitName);
        if (unitUIData == null)
            return;

        m_portrait.sprite = unitUIData.portrait;
        m_portrait.gameObject.SetActive(true);
        foreach (var module in unit.Modules)
        {
            if (!(module is ActionModule actionModule))
                continue;

            ModuleToggle toggle = m_togglePool.Get();
            toggle.transform.SetAsLastSibling();
            m_toggles.Add(module.ModuleName, toggle);
            var moduleUIData = m_moduleUIDataSO.GetData(module.ModuleName);
            toggle.Init(moduleUIData.icon);
            m_modules.Add(actionModule.ModuleName, actionModule);
            if (unit.CompareTag("PlayerUnit"))
            {
                toggle.Toggle.enabled = true;
                toggle.Toggle.interactable = actionModule.IsAvailable;
                toggle.Toggle.SetIsOnWithoutNotify(actionModule.Actived & actionModule.IsAvailable);
                toggle.Toggle.onValueChanged.AddListener(OnAnyToggle);
                m_prevToggleStatus.Add(actionModule.ModuleName, toggle.Toggle.isOn);
                toggle.gameObject.SetActive(true);
                actionModule.OnActionAvailable.AddListener(OnModuleAvailble);
            }
            else
            {
                toggle.Toggle.interactable = true;
                toggle.Toggle.SetIsOnWithoutNotify(true);
                toggle.Toggle.enabled = false;
                toggle.gameObject.SetActive(true);
            }
        }
    }

    private void ClearPanel()
    {
        foreach (var item in m_toggles)
        {
            m_togglePool.Release(item.Value);
            m_modules[item.Key].OnActionAvailable.RemoveListener(OnModuleAvailble);
        }
        m_toggles.Clear();
        m_prevToggleStatus.Clear();
        m_modules.Clear();
        m_portrait.gameObject.SetActive(false);
    }

    private void OnModuleAvailble (string module_name, bool is_available)
    {
        if(m_toggles.TryGetValue(module_name, out var toggle))
        {
            var am = m_modules[module_name];
            toggle.Toggle.interactable = is_available;
            toggle.Toggle.SetIsOnWithoutNotify(am.Actived & is_available);
        }
    }


    private void OnSelectedUnitChange(UnitBase unit)
    {
        if (unit == m_prevSelection)
            return;

        ClearPanel();
        if (unit != null)
            PopulateUnitUIInfo(unit);

        m_prevSelection = unit;
    }

    private void OnAnyToggle(bool is_on)
    {
        var keys = m_prevToggleStatus.Keys.ToArray();
        foreach (var item in keys)
        {
            //change of value
            if(m_prevToggleStatus[item]!= m_toggles[item].Toggle.isOn)
            {
                m_prevToggleStatus[item] = is_on;
                ((ActionModule)m_modules[item]).Actived = is_on;
            }
            else
            {
                if(is_on)
                {
                    ((ActionModule)m_modules[item]).Actived = false;
                    m_prevToggleStatus[item] = false;
                    m_toggles[item].Toggle.SetIsOnWithoutNotify(false);
                }
            }
        }
    }

    #region pool

    ModuleToggle CreateFunction()
    {
        var t = Instantiate(m_prefab, m_moduleHolder);
        t.gameObject.SetActive(false);
        return t;
    }


    private void OnGet(ModuleToggle toggle)
    {
        //toggle.gameObject.SetActive(true);
    }


    private void OnRelease(ModuleToggle toggle)
    {
        toggle.Toggle.onValueChanged.RemoveAllListeners();
        toggle.gameObject?.SetActive(false);
        m_moduleHolder.SetAsLastSibling();
    }

    private void DestroyFunction(ModuleToggle toggle)
    {
        Destroy(toggle.gameObject);
    }

    #endregion

}

[System.Serializable]
public class UnitUIData
{
    [SerializeField] public Sprite portrait;
    [SerializeField] public string unitName;
}


[System.Serializable]
public class ModuleUIData
{
    [SerializeField] public Sprite icon;
    [SerializeField] public string mouduleName;
}