using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SocialPlatforms;
public struct DepthBlock
{
    public int index;
    public float startX;
    public float startY;
    public float width;
    public float height;
    public float endX;
    public float endY;
}
public class UnitqueSet
{
    private HashSet<int> m_Set = new HashSet<int>();
    public void Remove(DepthBlock db)
    {
        m_Set.Remove(db.GetHashCode());
    }

    public bool Contains(DepthBlock depthBlock)
    {
        return m_Set.Contains(depthBlock.GetHashCode());
    }

    public void Add(DepthBlock depthBlock)
    {
        m_Set.Add(depthBlock.GetHashCode());
    }
    public void Clear()
    {
        m_Set.Clear();
    }
}
public class DepthArea
{
    private List<DepthBlock> m_DepthBlocks;
    private List<DepthBlock> m_AddingBlocks;
    private UnitqueSet m_UnitqueSet;
    private DepthBlock m_DataArea;
    public DepthBlock DataArea => m_DataArea;
    private float m_ContainerHight;
    private float m_ContainerWidth;

    private Dictionary<float, Dictionary<float, DepthBlock>> m_StartY2StartX2dBMap;
    public void SetContainer(float width, float height)
    {
        Debug.Log("11111111111111111");
        m_ContainerWidth = width;
        m_ContainerHight = height;
        m_DepthBlocks.Clear();
        DepthBlock initBlock = new DepthBlock(); 
        initBlock.width = width;
        initBlock.height = height;
        m_DepthBlocks.Add(initBlock);
    }
    public DepthArea()
    {
        m_DataArea = new DepthBlock();
        m_UnitqueSet = new UnitqueSet();
        m_AddingBlocks = new List<DepthBlock>();
        m_DepthBlocks = new List<DepthBlock>();
        m_StartY2StartX2dBMap = new Dictionary<float, Dictionary<float, DepthBlock>>();
    }
    public void Clear()
    {
        m_DataArea = new DepthBlock();
        m_UnitqueSet.Clear();
        m_DepthBlocks.Clear();
        m_AddingBlocks.Clear();
    }
    public Vector2 CutArea(float width, float height)
    {
        int insertIndex = FindInsertIndex(width, height, out Vector2 pos);
        var depthBlock = RemoveDepthBlockAt(insertIndex);
        CutDepthBlock(depthBlock, pos.x, pos.y, width, height);
        SplitWhiltArea(depthBlock, width, height);
        CheckCutAllDepthBlocks(pos.x, pos.y, width, height);
        UpdateAndSortBlocks();
        UpdateDataAreaSize(pos.x, pos.y, width, height);
        return pos;
    }
    private void UpdateAndSortBlocks()
    {
        for (int i = 0; i < m_AddingBlocks.Count; i++)
        {
            m_DepthBlocks.Add(m_AddingBlocks[i]);
        }
        UniqueBlocks();
        m_AddingBlocks.Clear();
        m_DepthBlocks.Sort(BlocksSortInverseCmp);
        //for (int i = 0;i< m_DepthBlocks.Count; i++)
        //{
        //    var depthBlock = m_DepthBlocks[i];
        //    Debug.Log("UpdateAndSortBlocks" + depthBlock.width + " " + depthBlock.height + " " + depthBlock.startX + " " + depthBlock.startY);
        //}
    }
    private int BlocksSortInverseCmp(DepthBlock a, DepthBlock b)
    {
        if (a.startY == b.startY)
        {
            if (a.startX == b.startX)
            {
                return a.width > b.width ? -1 : 1;
            }
            return a.startX < b.startX ? -1 : 1;
        }
        return a.startY < b.startY ? -1 : 1;
    }
    private void UniqueBlocks()
    {

    }
    private void UpdateDataAreaSize(float posX, float posY, float width, float height)
    {
        float endX = posX + width;
        float endY = posY + height;
        if (posX < m_DataArea.startX)
        {
            m_DataArea.startX = posX;
        }
        if (posY < m_DataArea.startY)
        {
            m_DataArea.startY = posY;
        }
        if (endX > m_DataArea.endX)
        {
            m_DataArea.endX = endX;
        }
        if (endY > m_DataArea.endY)
        {
            m_DataArea.endY = endY;
        }
    }
    private void SplitWhiltArea(DepthBlock splitBlock, float width, float height)
    {
        DepthBlock block = new DepthBlock();
        block.startX = splitBlock.startX;
        block.startY = splitBlock.startY + height;
        block.width = splitBlock.width;
        block.height = this.m_ContainerHight - (splitBlock.startY + height);
        //Debug.Log($"White {this.m_ContainerHight} {splitBlock.startY} {height} {splitBlock.width}");
        AddBlock(block);
    }
    private void CheckCutAllDepthBlocks(float posX, float posY, float width, float height)
    {
        int count = this.m_DepthBlocks.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            CutDepthBlock(m_DepthBlocks[i], posX, posY, width, height);
        }
    }
    private void CutDepthBlock(DepthBlock depthBlock, float posX, float posY, float width, float height)
    {
        float curItemEndY = posY + height;
        float curItemStartX = posX;
        float curItemStartY = posY;
        float curItemEndX = curItemStartX + width;
        float curDepthBlockEndX = depthBlock.startX + depthBlock.width;
        float curDepthBlockEndY = depthBlock.startY + depthBlock.height;

        if (curItemStartY >= depthBlock.startY && curItemEndX > depthBlock.startX)
        {
            RemoveDepthBlock(depthBlock);
            CutCorveredBlocks(depthBlock, curItemStartX, curItemStartY, width, height);
        }
        else if (curItemEndY > depthBlock.startY && curItemStartX < curDepthBlockEndX && curItemStartX >= depthBlock.startX)
        {
            RemoveDepthBlock(depthBlock);
            CutBlowBlocks(depthBlock, curItemStartX, curItemStartY, width, height);
        }



    }
    private void AddBlock(DepthBlock depthBlock)
    {
        if (depthBlock.height <= 0 || depthBlock.width <= 0) return;
        if (this.m_UnitqueSet.Contains(depthBlock)) return;

        bool hasMap1 = m_StartY2StartX2dBMap.TryGetValue(depthBlock.startY, out var startX2DbMap);
        if (!hasMap1)
        {
            startX2DbMap = new Dictionary<float, DepthBlock>();
            m_StartY2StartX2dBMap.Add(depthBlock.startY, startX2DbMap);
        }
        bool hasMap2 = startX2DbMap.TryGetValue(depthBlock.startX, out var maxWidthDB);
        float maxWidth = maxWidthDB.width;
        if (depthBlock.width >= maxWidthDB.width)
        {
            if (hasMap2)
            {
                startX2DbMap[depthBlock.startX] = depthBlock;
            }
            else
            {
                startX2DbMap.Add(depthBlock.startX, depthBlock);
            }
        }
        //Debug.Log("Adding"+depthBlock.width +" "+depthBlock.height+" "+depthBlock.startX+" "+depthBlock.startY);
        m_AddingBlocks.Add(depthBlock);
        m_UnitqueSet.Add(depthBlock);
    }
    private void CutBlowBlocks(DepthBlock depthBlock, float curItemStartX, float curItemStartY, float width, float height)
    {
        float curItemEndX = curItemStartX + width;
        float curItemEndY = curItemStartY + height;
        float curDepthBlockEndX = depthBlock.startX + depthBlock.width;
        float curDepthBlockEndY = depthBlock.startY + depthBlock.height;

        DepthBlock leftBlock = new DepthBlock();
        leftBlock.startX = depthBlock.startX;
        leftBlock.startY = depthBlock.startY;
        leftBlock.width = curItemStartX - depthBlock.startX;
        leftBlock.height = depthBlock.height;
        AddBlock(leftBlock);

        DepthBlock rightBlock = new DepthBlock();
        rightBlock.startX = curItemEndX;
        rightBlock.startY = depthBlock.startY;
        rightBlock.width = curDepthBlockEndX - curItemEndX;
        rightBlock.height = depthBlock.height;
        AddBlock(rightBlock);

        DepthBlock bottomBlock = new DepthBlock();
        bottomBlock.startX = depthBlock.startX;
        bottomBlock.startY = curItemEndY;
        bottomBlock.width = depthBlock.width;
        bottomBlock.height = curDepthBlockEndY - curItemEndY;
        AddBlock(bottomBlock);
    }
    private void CutCorveredBlocks(DepthBlock depthBlock, float curItemStartX, float curItemStartY, float width, float height)
    {
        float curItemEndX = curItemStartX + width;
        float curDepthBlockEndX = depthBlock.startX + depthBlock.width;
        DepthBlock cullBlock = new DepthBlock();
        cullBlock.startX = curItemEndX;
        cullBlock.startY = depthBlock.startY;
        cullBlock.width = curDepthBlockEndX - curItemEndX;
        cullBlock.height = depthBlock.height;
        AddBlock(cullBlock);
    }
    private int FindInsertIndex(float width, float height, out Vector2 pos)
    {
        int count = m_DepthBlocks.Count;
        pos = Vector2.zero;
        int insertIndex = -1;
        Debug.Log(m_DepthBlocks.Count);
        for (int i = count - 1; i >= 0; i--)
        {
            var dB = m_DepthBlocks[i];
            //Debug.Log(dB.width + "  width" + width);
            if (dB.width >= width)
            {
                pos.x = dB.startX;
                pos.y = dB.startY;
                insertIndex = i;
            }
        }
        Debug.Log("insertIndex " + insertIndex);
        return insertIndex;
    }
    private DepthBlock RemoveDepthBlockAt(int index)
    {
        Debug.Log("RemoveDepthBlockAt" + index);
        var dB = this.m_DepthBlocks[index];
        this.RemoveDepthBlockCommon(dB);
        this.m_DepthBlocks.RemoveAt(index);
        return dB;
    }
    private DepthBlock RemoveDepthBlock(DepthBlock dB)
    {
        int index = this.m_DepthBlocks.IndexOf(dB);
        if (index == -1) return dB;
        return RemoveDepthBlockAt(index);
    }
    private void RemoveDepthBlockCommon(DepthBlock db)
    {
        if (m_StartY2StartX2dBMap.TryGetValue(db.startY, out var startX2DbMap))
        {
            if (startX2DbMap.TryGetValue(db.startY, out var maxWidthDB))
            {
                if (maxWidthDB.index == db.index)
                {
                    startX2DbMap.Remove(db.startY);
                }
            }
            if (startX2DbMap.Count == 0)
            {
                m_StartY2StartX2dBMap.Remove(db.startY);
            }
        }

        this.m_UnitqueSet.Remove(db);


    }


}
