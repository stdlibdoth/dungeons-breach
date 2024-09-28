using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] private float m_width;
    [SerializeField] private float m_sidePadding;
    
    [Space]
    [SerializeField] private SpriteRenderer m_frameSprite;
    [SerializeField] private SpriteRenderer m_healthSprite;
    [SerializeField] private Transform m_blockersHolder;
    [SerializeField] private GameObject m_blockerPrefab;

    [Space]
    [SerializeField] private SerializedAnimatorStates m_animatorStates;
    [SerializeField] private Animator m_animator;

    private float m_unitWidth;
    private int m_maxHP;
    private int m_hp;
    private List<GameObject> m_blockers = new List<GameObject>();


    private List<int> m_deltaHP = new List<int>();
    private List<int> m_deltaMaxHP = new List<int>();

    private Sequence m_frameSeq;
    private Sequence m_healthSeq;

    public void Init(int maxHP)
    {
        ClearBlockers();
        m_maxHP = maxHP;
        float healthWidth = m_width - 2* m_sidePadding;
        m_frameSprite.size = new Vector2(m_width,m_frameSprite.size.y);
        m_healthSprite.size = new Vector2(healthWidth,m_healthSprite.size.y);
        m_unitWidth = healthWidth/maxHP;
        transform.localPosition = new Vector3(-m_width/2,0,0);
        int blockerNum = maxHP-1;
        for (int i = 1; i <= blockerNum; i++)
        {
            float x = m_unitWidth*i + m_sidePadding;
            var blocker = Instantiate(m_blockerPrefab,m_blockersHolder);
            blocker.transform.localPosition = new Vector3(x,0,0);
        }
        gameObject.SetActive(true);
    }


    public bool GenerateAnimationStateData(string animation_state, out AnimationStateData data)
    {
        data = null;
        if(m_animatorStates.TryGetAnimatorState(animation_state))
        {
            data = new AnimationStateData
            {
                animationState = animation_state,
                animator = m_animator,
            };
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loops"> -1 to set infinity</param>
    public void StartDamangeAnimation(int loops)
    {
        m_frameSeq = DOTween.Sequence(this);
        m_healthSeq = DOTween.Sequence(this);
        m_frameSeq.Append(m_frameSprite.DOFade(0.1f,0.2f))
        .Append(m_frameSprite.DOFade(1f,0.2f))
        .SetLoops(loops);
        m_healthSeq.Append(m_healthSprite.DOFade(0.1f,0.2f))
        .Append(m_healthSprite.DOFade(1f,0.2f))
        .SetLoops(loops);
    }

    private void StopDamageAnimation()
    {
        m_healthSprite.color = Color.white;
        m_frameSprite.color = Color.white;
        m_frameSeq.Kill();
        m_healthSeq.Kill();
    }

    private void ClearBlockers()
    {
        foreach (var item in m_blockers)
        {
            Destroy(item);
            m_blockers.Clear();
        }
    }

    public void SetHP(int hp)
    {
        int h = math.clamp(hp,0,m_maxHP);
        m_healthSprite.size = new Vector2(m_unitWidth*h,m_healthSprite.size.y);
        m_hp = h;
    }


    #region preview

    public void SetPreview(int delta_maxHP, int delta_hp)
    {
        ClearBlockers();
        if(delta_hp!=0)
            m_deltaHP.Add(delta_hp);
        if(delta_maxHP!=0)
            m_deltaMaxHP.Add(delta_maxHP);

        int maxHP = PropagateDelta(m_maxHP, m_deltaMaxHP);       
        int hp = PropagateDelta(m_hp, m_deltaHP);


        if(delta_maxHP!=0)
        {
            float healthWidth = m_width - 2* m_sidePadding;
            m_frameSprite.size = new Vector2(m_width,m_frameSprite.size.y);
            m_healthSprite.size = new Vector2(healthWidth,m_healthSprite.size.y);
            m_unitWidth = healthWidth/maxHP;      
            transform.localPosition = new Vector3(-m_width/2,0,0);
            int blockerNum = maxHP-1;
            for (int i = 1; i <= blockerNum; i++)
            {
                float x = m_unitWidth*i + m_sidePadding;
                var blocker = Instantiate(m_blockerPrefab,m_blockersHolder);
                blocker.transform.localPosition = new Vector3(x,0,0);
            }
        }
        
        int h = math.clamp(hp,0,m_maxHP);
        m_healthSprite.size = new Vector2(m_unitWidth*h,m_healthSprite.size.y);
        
    }

    public void ResetPreview()
    {
        StopDamageAnimation();
        Init(m_maxHP);
        SetHP(m_hp);
        m_deltaHP.Clear();
        m_deltaMaxHP.Clear();
    }
    #endregion


#region helper

    private int PropagateDelta(int initial, List<int> data)
    {
        int n = initial;
        for (int i = 0; i < data.Count; i++)
        {
            n +=data[i];
        }
        if(n<1)
            n = 1;
        return n;
    }

#endregion
}
