using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicOrderSort : MonoBehaviour
{
    [SerializeField]
    private SortingGroup m_SortingGroup;
    private const int m_YMultiplier = 100;
    public int OrderStep = 0;

    void Awake()
    {
        
        if (m_SortingGroup == null)
        {
            m_SortingGroup = gameObject.GetOrAddComponent<SortingGroup>();
        }
    }

    void LateUpdate()
    {
        int baseOrder = Mathf.RoundToInt(transform.position.y * -m_YMultiplier) ;
        m_SortingGroup.sortingOrder = baseOrder + OrderStep;
    }
}
