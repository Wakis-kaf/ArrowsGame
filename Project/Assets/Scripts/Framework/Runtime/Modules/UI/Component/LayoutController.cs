using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class LayoutController
    {
        private Rect contentArea;
        private Vector2 cornerOffset;
        private Vector2 dataArea;

        // 从visibleArea 中剔除padding 后的区域
        private Rect dataBounds;

        private Vector2 flexContentSize;
        private Vector2 offset;
        private Vector2 paddingOffset;
        private Rect visibleArea; // 当前content 在 view 中的可视区域
        private Rect visibleDataBounds;

        public LayoutData CalculateLayout(LayoutData layout)
        {
            // 计算顺序 ChildAlignment padding
            layout.visibleItemSet.Clear();
            // 计算可视区域
            CalculateVisibleArea(layout);
            // 计算数据需要占据的空间
            CalculateDataArea(layout);
            CalculateDataAreaOffsetRelativeContent(layout);
            CalculatePaddingOffset(layout);
            offset = paddingOffset + cornerOffset;
            layout.offset = offset;
            // 计算
            dataBounds.x = offset.x;
            dataBounds.y = offset.y;
            dataBounds.width = dataArea.x;
            dataBounds.height = dataArea.y;
            // 取两个区域的交集
            CalculateVisibleDataArea(layout);
            // 计算该交集中的数据坐标
            CalculateVisibleDataIndexList(layout);
            CalculateFlexContentSize(layout);
            CalculateTotalRenderCount(layout);
            return layout;
        }

        public Vector2 GetPositionByIndex(LayoutData layout, int index)
        {
            // 通过index
            Vector2 allSize = layout.containerSize;
            allSize.x -= Mathf.Abs(layout.padding.left - layout.padding.right);
            allSize.y -= Mathf.Abs(layout.padding.top - layout.padding.bottom);
            int rowCount = GetRowCount(layout, allSize.x, allSize.y);
            int columnCount = GetColumnCount(layout, allSize.x, allSize.y);
            Vector2 res = Vector2.zero;
            int curColumn;
            int curRow;

            if (layout.axis == Axis.Horizontal)
            {
                curColumn = index % columnCount;
                curRow = (int)(index / columnCount);
            }
            else
            {
                curRow = index % rowCount;
                curColumn = (int)(index / rowCount);
            }

            if (layout.corner == Corner.LowerLeft || layout.corner == Corner.LowerRight) // 上下翻
            {
                curRow = rowCount - curRow - 1;
            }

            if (layout.corner == Corner.UpperRight || layout.corner == Corner.LowerRight) // 左右翻
            {
                curColumn = columnCount - curColumn - 1;
            }

            res.x = curColumn * layout.itemSize.x + curColumn * layout.spacing.x + layout.offset.x;
            res.y = -curRow * layout.itemSize.y - curRow * layout.spacing.y - layout.offset.y;
            return res + layout.customOffset;
        }

        private void CalculateDataArea(LayoutData layout)
        {
            // 获取每行 先计算内容
            Vector2 allSize = Vector2.zero;
            allSize.x = Mathf.Max(layout.containerSize.x, 0);
            allSize.y = Mathf.Max(layout.containerSize.y, 0);
            allSize.x -= Mathf.Abs(layout.padding.left - layout.padding.right);
            allSize.y -= Mathf.Abs(layout.padding.top - layout.padding.bottom);
            int rowCount = GetRowCount(layout, allSize.x, allSize.y);
            int columnCount = GetColumnCount(layout, allSize.x, allSize.y);
            dataArea.x = (columnCount - 1) * layout.spacing.x + columnCount * layout.itemSize.x;
            dataArea.y = (rowCount - 1) * layout.spacing.y + rowCount * layout.itemSize.y;
            dataArea.x = Mathf.Max(dataArea.x, 0);
            dataArea.y = Mathf.Max(dataArea.y, 0);
        }

        private void CalculateDataAreaOffsetRelativeContent(LayoutData layout)
        {
            // 注意这里的dataArea 是剔除了padding 的，所以会偏大
            Vector2 offset = Vector2.zero;
            switch (layout.alignment)
            {
                case ChildAlignment.UpperLeft:
                    break;

                case ChildAlignment.UpperRight:
                    offset.x = layout.containerSize.x - dataArea.x;
                    break;

                case ChildAlignment.UpperCenter:
                    offset.x = (layout.containerSize.x - dataArea.x) / 2;
                    break;

                case ChildAlignment.MiddleLeft:
                    offset.y = (layout.containerSize.y - dataArea.y) / 2;
                    break;

                case ChildAlignment.MiddleCenter:
                    offset.x = (layout.containerSize.x - dataArea.x) / 2;
                    offset.y = (layout.containerSize.y - dataArea.y) / 2;
                    break;

                case ChildAlignment.MiddleRight:
                    offset.x = layout.containerSize.x - dataArea.x;
                    offset.y = (layout.containerSize.y - dataArea.y) / 2;
                    break;

                case ChildAlignment.LowerLeft:
                    offset.y = layout.containerSize.y - dataArea.y;
                    break;

                case ChildAlignment.LowerCenter:
                    offset.x = (layout.containerSize.x - dataArea.x) / 2;
                    offset.y = layout.containerSize.y - dataArea.y;
                    break;

                case ChildAlignment.LowerRight:
                    offset.x = layout.containerSize.x - dataArea.x;
                    offset.y = layout.containerSize.y - dataArea.y;
                    break;
            }
            cornerOffset = offset;
        }

        private void CalculateFlexContentSize(LayoutData layout)
        {
            Vector2 contentSize = dataBounds.size;
            contentSize.x += layout.padding.left + layout.padding.right;
            contentSize.y += layout.padding.top + layout.padding.bottom;
            layout.flexContentSize = contentSize;
        }

        private void CalculatePaddingOffset(LayoutData layout)
        {
            paddingOffset.x = layout.padding.left - layout.padding.right;
            paddingOffset.y = layout.padding.top - layout.padding.bottom;
        }

        private void CalculateTotalRenderCount(LayoutData layout)
        {
            int width = layout.columnEnd - layout.columnStart;
            int height = layout.rowEnd - layout.rowStart;
            layout.maxVisibleItemCount = width * height;
        }

        private void CalculateVisibleArea(LayoutData layout)
        {
            float viewLeft = layout.viewRelativeStart.x;
            float viewRight = layout.viewRelativeStart.x + layout.viewSize.x;
            float viewTop = layout.viewRelativeStart.y;
            float viewBottom = layout.viewRelativeStart.y + layout.viewSize.y;
            float x = Mathf.Clamp(viewLeft, 0, Mathf.Min(layout.containerSize.x, viewRight));
            float y = Mathf.Clamp(viewTop, 0, Mathf.Min(layout.containerSize.y, viewBottom));
            float width = Mathf.Min(layout.containerSize.x, viewRight) - x;
            float height = viewBottom - y;
            visibleArea.x = x;
            visibleArea.y = y;
            visibleArea.width = width;
            visibleArea.height = height;
        }

        private void CalculateVisibleDataArea(LayoutData layoutData)
        {
            float left = Mathf.Max(visibleArea.x, dataBounds.x);
            float top = Mathf.Max(visibleArea.y, dataBounds.y);
            float right = Mathf.Min(visibleArea.x + visibleArea.width, dataBounds.x + dataBounds.width);
            float bottom = Mathf.Min(visibleArea.y + visibleArea.height, dataBounds.y + dataBounds.height);
            visibleDataBounds.x = left;
            visibleDataBounds.y = top;
            visibleDataBounds.width = right - left;
            visibleDataBounds.height = bottom - top;
        }

        private void CalculateVisibleDataIndexList(LayoutData layout)
        {
            // 默认左上
            int columnStart = Mathf.FloorToInt((float)(visibleDataBounds.x - dataBounds.x) / (layout.itemSize.x + layout.spacing.x));
            int columnEnd = Mathf.CeilToInt((float)(visibleDataBounds.x + visibleDataBounds.width - dataBounds.x) / (layout.itemSize.x + layout.spacing.x));
            int rowStart = Mathf.FloorToInt((float)(visibleDataBounds.y - dataBounds.y) / (layout.itemSize.y + layout.spacing.y));
            int rowEnd = Mathf.CeilToInt((float)(visibleDataBounds.y + visibleDataBounds.height - dataBounds.y) / (layout.itemSize.y + layout.spacing.y));

            Vector2 allSize = layout.containerSize;
            allSize.x -= Mathf.Abs(layout.padding.left - layout.padding.right);
            allSize.y -= Mathf.Abs(layout.padding.top - layout.padding.bottom);
            int rowCount = GetRowCount(layout, allSize.x, allSize.y);
            int columnCount = GetColumnCount(layout, allSize.x, allSize.y);
            for (int row = rowStart; row < Mathf.Min(rowEnd, rowCount); row++)
            {
                if (row < 0 || row > rowCount) continue;
                for (int column = columnStart; column < Mathf.Min(columnEnd, columnCount); column++)
                {
                    if (column < 0 || column > columnCount) continue;
                    int index = GetIndex(layout, row, column, rowCount, columnCount);
                    if (index >= layout.itemCount) continue;
                    layout.visibleItemSet.Add(index);
                }
            }
            layout.visibleItemSet.Sort();
            layout.columnStart = columnStart;
            layout.columnEnd = Mathf.Min(columnEnd, columnCount);
            layout.rowStart = rowStart;
            layout.rowEnd = Mathf.Min(rowEnd, rowCount);
        }

        private int GetColumnCount(LayoutData layoutData, float contentWidth, float contentHeight)
        {
            if (layoutData.axis == Axis.Vertical)
            {
                switch (layoutData.layoutConstraint)
                {
                    case Constraint.AutoExpand:
                        return 1;

                    case Constraint.FixedCount:
                        return Mathf.CeilToInt((float)layoutData.itemCount / layoutData.axisFixedCount);

                    case Constraint.FixedSize:
                        int rowCount = GetRowCount(layoutData, contentWidth, contentHeight);
                        int colCount = Mathf.CeilToInt((float)layoutData.itemCount / rowCount);
                        return Mathf.Max(colCount,1);
                }
            }
            else
            {
                switch (layoutData.layoutConstraint)
                {
                    case Constraint.AutoExpand:
                        return layoutData.itemCount;

                    case Constraint.FixedCount:
                        return Mathf.Min(layoutData.itemCount, layoutData.axisFixedCount);
                        //return layoutData.axisFixedCount;

                    case Constraint.FixedSize:
                        int colCount = Mathf.Min(Mathf.FloorToInt(
                            (float)(contentWidth - layoutData.itemSize.x) / layoutData.itemSize.x) + 1,
                            layoutData.itemCount);

                        return Mathf.Max(colCount,1);
                }
            }
            return 0;
        }

        private int GetIndex(LayoutData layout, int row, int column, int rowTotal, int columnTotal)
        {
            var corner = layout.corner;
            if (corner == Corner.LowerLeft || corner == Corner.LowerRight) // 上下翻
            {
                row = rowTotal - row - 1;
            }

            if (corner == Corner.UpperRight || corner == Corner.LowerRight) // 左右翻
            {
                column = columnTotal - column - 1;
            }
            return layout.axis == Axis.Horizontal ? row * columnTotal + column : column * rowTotal + row;
        }

        private int GetRowCount(LayoutData layoutData, float contentWidth, float contentHeight)
        {
            if (layoutData.axis == Axis.Horizontal)
            {
                switch (layoutData.layoutConstraint)
                {
                    case Constraint.AutoExpand:
                        return 1;

                    case Constraint.FixedCount:
                        return Mathf.CeilToInt((float)layoutData.itemCount / layoutData.axisFixedCount);

                    case Constraint.FixedSize:
                        int columnCount = GetColumnCount(layoutData, contentWidth, contentHeight);
                        int rowCount = Mathf.CeilToInt((float)layoutData.itemCount / columnCount);
                        return Mathf.Max(rowCount,1);
                }
            }
            else
            {
                switch (layoutData.layoutConstraint)
                {
                    case Constraint.AutoExpand:
                        return layoutData.itemCount;

                    case Constraint.FixedCount:
                        return Mathf.Min(layoutData.itemCount, layoutData.axisFixedCount);
                        //return layoutData.axisFixedCount;

                    case Constraint.FixedSize:
                        int rowCount = Mathf.Min(Mathf.FloorToInt(
                            (float)(contentHeight - layoutData.itemSize.y) / layoutData.itemSize.y) + 1
                            , layoutData.itemCount);
                        return Mathf.Max(rowCount,1);
                }
            }

            return 0;
        }
    }

    public class LayoutData
    {
        public ChildAlignment alignment;
        public Axis axis;
        public int axisFixedCount;
        public int columnEnd;
        public int columnStart;
        public Vector2 containerSize;
        public Corner corner;
        public Vector2 customOffset;
        public Vector2 flexContentSize;
        public int itemCount;
        public Vector2 itemSize;
        public Constraint layoutConstraint;
        public int maxVisibleItemCount;
        public Vector2 offset;
        public Padding padding;
        public int rowEnd;
        public int rowStart;
        public Vector2 spacing;
        public Vector2 viewRelativeStart;
        public Vector2 viewSize;
        public List<int> visibleItemSet = new List<int>();

        public bool IsVisible(int index)
        {
            return visibleItemSet.Contains(index);
        }
    }
}