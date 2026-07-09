


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UFlexList : ScrollRect
{
    private DepthArea m_DepthArea = new DepthArea();
    private List<object> m_Datas = new List<object>();
    public List<object> Datas => m_Datas;
    private List<IFlexListItem> m_Items = new List<IFlexListItem>();
    private Queue<IFlexListItem> m_FreeItems = new Queue<IFlexListItem>();

    private Rect m_VisibleAreaRect = new Rect();
    private Dictionary<int, Vector2> m_Index2Pos = new Dictionary<int, Vector2>();
    private int m_LastCalculatedIndex = 0;
    private List<int> m_VisibleIndexs = new List<int>();
    private Dictionary<int, bool> m_ItemVisibleState = new Dictionary<int, bool>();
    private Dictionary<int, IFlexListItem> m_Index2Item = new Dictionary<int, IFlexListItem>();

    private Func<object, IFlexListItem> m_ItemCreator;
    private Func<object, Vector2> m_SizeGetter;
    private Vector2 m_ContainerSize;
    protected override void Awake()
    {
        base.Awake();
       
        onValueChanged.AddListener(OnRectValueChanged);
        InitCheck();
    }
    private void InitCheck()
    {
        m_ContainerSize = new Vector2(content.rect.size.x, int.MaxValue);
        var ctorSize = GetContainerSize();
        m_DepthArea.SetContainer(ctorSize.x, ctorSize.y);
    }
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        OnRectValueChanged(normalizedPosition);
    }
    private void OnRectValueChanged(Vector2 normedPos)
    {
        UpdateVisibleAreaRect();
        OnVisibleAreaChanged();
    }
    private void UpdateVisibleAreaRect()
    {
        // 更新 m_VisibleAreaRect
        var contentPos = content.anchoredPosition;
        var viewSize = viewport.rect.size;
        var viewPos = viewport.anchoredPosition;
        var contentSize = content.sizeDelta;
        //Debug.Log($"{contentPos} {viewSize} {viewPos} {contentSize}");
        var endX = Math.Min(contentPos.x + contentSize.x, viewPos.x + viewSize.x);
        var endY = Math.Min(contentPos.y + contentSize.y, viewPos.y + viewSize.y);
        m_VisibleAreaRect.x = contentPos.x;
        m_VisibleAreaRect.y = -contentPos.y;
        m_VisibleAreaRect.width = endX - m_VisibleAreaRect.x;
        m_VisibleAreaRect.height = endY - m_VisibleAreaRect.y;
    }
    public void BindItemCreator(Func<object, IFlexListItem> func)
    {
        m_ItemCreator = func;
    }
    public void BindSizeGetter(Func<object, Vector2> func)
    {
        m_SizeGetter = func;
    }

    public void SetDataSoureces(List<object> datas, bool refreshLayout = true)
    {
        m_Datas = datas;
        InitCheck();
        DataChaned(refreshLayout);
    }
    public void DataChaned(bool refreshLayout = false)
    {
        if (refreshLayout)
        {
            ResetAll();
            OnVisibleAreaChanged();
        }
    }
    public void RemoveData(object data)
    {
        int dataIndex = Datas.IndexOf(data);
        Datas.RemoveAt(dataIndex);
        SetDataSoureces(Datas, false);
        DataChaned(true);
    }
    public void AppendData(object data)
    {
        m_Datas.Add(data);
        OnVisibleAreaChanged();
    }
    private void ResetAll()
    {
        m_LastCalculatedIndex = 0;
        ReleaseAllVisible();
    }
    private void ReleaseAllVisible()
    {
        int count = m_VisibleIndexs.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            ReleaseItem(m_VisibleIndexs[i]);
        }
    }

    private Vector2 GetContainerSize()
    {
        return m_ContainerSize; ;
    }
    private void CheckPos()
    {
        UpdateVisibleAreaRect();
        for (int i = m_LastCalculatedIndex; i < Datas.Count; i++)
        {
          
            if (!IsDataAreaFullVisiable())
            {
                m_LastCalculatedIndex = i;
                var size = GetSizeAt(i);
                var pos = m_DepthArea.CutArea(size.x, size.y);
                //Debug.Log("计算位置" + pos);
                if (m_Index2Pos.ContainsKey(i))
                {
                    m_Index2Pos[i] = pos;
                }
                else
                {
                    m_Index2Pos.Add(i, pos);
                }
            }
            else
            {
                break;
            }

        }
    }
    private bool IsAreaCrossVisible(Vector2 areaPos, Vector2 areaSize)
    {
        float startX = areaPos.x;
        float startY = areaPos.y;
        float endX = areaPos.x + areaSize.x;
        float endY = areaPos.y + areaSize.y;

        float visibleStartX = m_VisibleAreaRect.x;
        float visibleStartY = m_VisibleAreaRect.y;
        float visibleEndX = m_VisibleAreaRect.x + m_VisibleAreaRect.width;
        float visibleEndY = m_VisibleAreaRect.y + m_VisibleAreaRect.height;
        bool xCross = (startX >= visibleStartX && startX <= visibleEndX) || (visibleStartX >= startX && visibleStartX <= endX);
        bool yCross = (startY >= visibleStartY && startY <= visibleEndY) || (visibleStartY >= startY && visibleStartY <= endY);
        return xCross && yCross;
    }
    private Vector2 GetPosAt(int index)
    {
        return m_Index2Pos[index];
    }
    private Vector2 GetSizeAt(int index)
    {
        //Debug.Log($"GetSizeAt{index} {m_SizeGetter.Invoke(Datas[index])}");
        return m_SizeGetter.Invoke(Datas[index]);
    }
    private void OnVisibleAreaChanged()
    {
        m_LastCalculatedIndex = 0;
        CheckPos();
        HideLastVisibleItems();
        CalculateVisibleIndexs();
        DrawItems();
    }
    private void HideLastVisibleItems()
    {
        for (int i = 0; i < m_VisibleIndexs.Count; i++)
        {
            SetItemState(m_VisibleIndexs[i], false);
        }
        m_VisibleIndexs.Clear();
    }
    private void SetItemState(int index, bool state)
    {
        if (m_ItemVisibleState.ContainsKey(index))
        {
            m_ItemVisibleState[index] = state;
            return;
        }
        m_ItemVisibleState.Add(index, state);
    }
    private bool IsVisibling(int index)
    {
        return m_ItemVisibleState.ContainsKey(index) && m_ItemVisibleState[index];
    }
    private void CalculateVisibleIndexs()
    {
        for (int i = 0; i <= m_LastCalculatedIndex; i++)
        {
            if (!m_Index2Pos.ContainsKey(i)) continue;
            var pos = GetPosAt(i);
            var size = GetSizeAt(i);
            //Debug.Log($"m_LastCalculatedIndex{m_LastCalculatedIndex} pos:{pos} size:{size} IsAreaCrossVisible(pos, size){IsAreaCrossVisible(pos, size)} !IsVisibling(i){!IsVisibling(i)}");
            if (IsAreaCrossVisible(pos, size))
            {
                if (!IsVisibling(i))
                {
                    m_VisibleIndexs.Add(i);
                    SetItemState(i, true);
                }
            }
        }
    }
    private void ReleaseItem(int index)
    {
        var item = GetItemAt(index);
        HideItem(index);
        RemoveItemAt(index);
        item.Index = -1;
        m_FreeItems.Enqueue(item);
    }
    public object GetData(int index)
    {
        return Datas[index];
    }
    private void PutItem(int index)
    {
        var item = GetItem(GetData(index));
        SetItemIn(index, item);
        item.Index = index;
        SetSize(item, GetSizeAt(index));
        SetPos(item, GetPosAt(index));
        ShowItem(index);
    }
    private void ShowItem(int index)
    {
        var item = GetItemAt(index);
        item.Show();
    }
    private void HideItem(int index)
    {
        var item = GetItemAt(index);
        item.Hide();
    }
    private void DrawItems()
    {
        for (int i = 0; i < m_VisibleIndexs.Count; i++)
        {
            int index = m_VisibleIndexs[i];
            //Debug.Log("visibleIndex" + index);
            if (!IsVisibling(index))
            {
                // 回收不需显示的
                ReleaseItem(index);
            }
            else
            {
                // 显示需要显示的
                PutItem(index);
            }
        }
        int count = m_VisibleIndexs.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            int index = m_VisibleIndexs[i];
            if (!IsVisibling(index))
            {
                m_VisibleIndexs.RemoveAt(i);
            }
        }
    }
    private void SetPos(IFlexListItem item, Vector2 pos)
    {
        pos.y = -pos.y;
        item.SetPosition(pos);
    }
    private void SetSize(IFlexListItem item, Vector2 size)
    {
        item.SetSize(size);
    }
    private void SetItemIn(int index, IFlexListItem item)
    {
        if (m_Index2Item.ContainsKey(index))
        {
            m_Index2Item[index] = item;
        }
        else
        {
            m_Index2Item.Add(index, item);
        }
    }
    private IFlexListItem GetItemAt(int index)
    {
        m_Index2Item.TryGetValue(index, out var item);
        return item;
    }
    private void RemoveItemAt(int index)
    {
        m_Index2Item.Remove(index);
    }
    private IFlexListItem GetItem(object data)
    {
        if (m_FreeItems.Count > 0) return m_FreeItems.Dequeue();
        var item = m_ItemCreator.Invoke(data);
        m_Items.Add(item);
        return item;
    }
    private bool IsDataAreaFullVisiable()
    {
        var dataAreaRect = m_DepthArea.DataArea;
        var visibleRect = m_VisibleAreaRect;
        //Debug.Log(dataAreaRect + " " + visibleRect);
        if (dataAreaRect.startX <= visibleRect.x &&
        dataAreaRect.startY <= visibleRect.y &&
        dataAreaRect.endX >= visibleRect.x + visibleRect.width &&
        dataAreaRect.endY >= visibleRect.y + visibleRect.height)
            return true;

        return false;
    }




}
public interface IFlexListItem
{
    public int Index { get; set; }
    public float Width { get; }
    public float Height { get; }
    public void Show();
    public void Hide();
    void SetPosition(Vector2 pos);
    void SetSize(Vector2 size);
}
