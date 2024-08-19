using UnityEngine;
using TMPro;

public class IsoTileBase : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_coordTMP;
    [SerializeField] private bool m_showCoord;
    private IsoGridCoord m_coord;


    public void SetCoord(IsoGridCoord coord)
    {
        m_coord = coord;
        if(m_showCoord)
            m_coordTMP.text = coord.ToString();
    }
}
