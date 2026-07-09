using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GridsSpaceEditor.Core;
using GridsSpaceEditor.Data.Models;
using GridsSpaceEditor.UI.Components;
using GridsSpaceEditor.UI.Shared;
using GridsSpaceEditor.UI.Tabs.GridTab;
using GridsSpaceEditor.UI.Tabs.PortTab;
using UnityEditor;
using UnityEngine;

public class GridsSpaceEditorWindow : EditorWindow
{
    private GridManager m_GridManager;
    private PortManager m_PortManager;
    private ShapeCodeEngine m_ShapeCodeEngine;

    private GridView m_GridView;
    private IOSection m_IOSection;

    private GridEditTab m_GridEditTab;
    private ParamsPreviewTab m_ParamsPreviewTab;
    private PropertiesTab m_PropertiesTab;
    private DataOverviewTab m_DataOverviewTab;
    private PortEditorTab m_PortEditorTab;

    private TabToolbar m_MainTabToolbar;
    private TabToolbar m_GridSubTabToolbar;
    private TabToolbar m_PortSubTabToolbar;

    private SystemData m_SystemData;
    private int m_MainTabIndex = 0;
    private int m_GridSubTabIndex = 0;
    private int m_PortSubTabIndex = 0;
    private bool m_IsResizingSplitter = false;

    private Vector2 m_RightPanelScrollPos;
    private List<GridCellData> m_Cells;

    private const string m_SystemDataPath = "Assets/Editor/GridsSpaceEditor/GridData/SystemData.json";
    private const string m_TempDataPath = "Assets/Editor/GridsSpaceEditor/GridData/Temp.json";

    [MenuItem("Window/Grids Editor")]
    public static void OpenWindow()
    {
        GridsSpaceEditorWindow window = GetWindow<GridsSpaceEditorWindow>("Grids Editor");
        window.minSize = new Vector2(900, 600);
    }

    private void OnEnable()
    {
        LoadSystemData();
        LoadTempData();
        InitializeComponents();
        SyncIOShapeCode();
    }

    private void InitializeComponents()
    {
        m_GridManager = new GridManager();
        m_PortManager = new PortManager();
        m_ShapeCodeEngine = new ShapeCodeEngine(m_PortManager);
        m_ShapeCodeEngine.SetGridManager(m_GridManager);

        if (m_SystemData.GridTypes.Count == 0)
            m_SystemData.GridTypes.Add("Default");
        if (m_SystemData.GridTypes.Count > 0)
            m_GridManager.SetDefaultType(m_SystemData.GridTypes[0]);

        m_GridManager.SetCells(m_Cells ?? new List<GridCellData>());

        m_GridView = new GridView(m_GridManager, m_SystemData);
        m_GridView.SetPortManager(m_PortManager);
        m_IOSection = new IOSection(m_SystemData);
        m_IOSection.SetShapeCodeEngine(m_ShapeCodeEngine);

        m_GridEditTab = new GridEditTab(m_GridView, m_SystemData, () =>
        {
            SaveSystemData();
            Repaint();
        });
        m_ParamsPreviewTab = new ParamsPreviewTab(m_GridManager, m_SystemData);
        m_PropertiesTab = new PropertiesTab(m_SystemData);
        m_DataOverviewTab = new DataOverviewTab(m_GridManager);
        m_PortEditorTab = new PortEditorTab(m_GridManager, m_PortManager, m_ShapeCodeEngine, OnDataChanged, () => Repaint());

        m_MainTabToolbar = new TabToolbar("网格编辑", "端口编辑");
        m_GridSubTabToolbar = new TabToolbar("编辑", "参数预览", "属性模板", "数据总览");
        m_PortSubTabToolbar = new TabToolbar("端口检查器", "预设库", "形状码");

        m_GridManager.OnDataChanged += OnDataChanged;
        m_IOSection.OnExportRequested += ExportGridData;
        m_IOSection.OnImportRequested += LoadGridData;
        m_IOSection.OnSystemDataChanged += SaveSystemData;
        m_IOSection.OnShapeCodeImported += OnShapeCodeImported;
        m_PropertiesTab.OnSystemDataChanged += SaveSystemData;
        m_ParamsPreviewTab.OnDataChanged += OnDataChanged;
    }

    private void OnDataChanged()
    {
        m_Cells = m_GridManager.Cells.ToList();
        m_IOSection.UpdateCurrentShapeCode(m_ShapeCodeEngine.Generate(m_Cells));
        SaveTempData();
        Repaint();
    }

    private void OnShapeCodeImported()
    {
        m_Cells = m_GridManager.Cells.ToList();
        m_IOSection.UpdateCurrentShapeCode(m_ShapeCodeEngine.Generate(m_Cells));
        SaveTempData();
        Repaint();
    }

    private void OnGUI()
    {
        if (m_GridView == null) InitializeComponents();

        const float splitterW = 5f;
        float rightPanelWidth = Mathf.Clamp(m_GridView.RightPanelWidth, 250f, Mathf.Max(250f, position.width - 80f));
        m_GridView.RightPanelWidth = rightPanelWidth;
        float leftWidth = Mathf.Max(50f, position.width - rightPanelWidth - splitterW);

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

        Rect gridAreaRect = GUILayoutUtility.GetRect(leftWidth, position.height, GUILayout.Width(leftWidth), GUILayout.MinWidth(50f), GUILayout.ExpandHeight(true));

        bool portEditTab = m_MainTabIndex == 1;
        // 网格编辑模式下不显示端口，只在端口编辑模式下显示
        m_GridView.DrawView(gridAreaRect, portEditTab, false);

        DrawGridToolbarOverlay(gridAreaRect);

        Rect splitterRect = GUILayoutUtility.GetRect(splitterW, position.height, GUILayout.Width(splitterW), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(splitterRect, new Color(0.15f, 0.15f, 0.15f));
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

        if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition))
            m_IsResizingSplitter = true;

        if (m_IsResizingSplitter)
        {
            m_GridView.RightPanelWidth = position.width - Event.current.mousePosition.x;
            Repaint();
        }

        if (Event.current.rawType == EventType.MouseUp)
            m_IsResizingSplitter = false;

        EditorGUILayout.BeginVertical(GUILayout.Width(rightPanelWidth), GUILayout.ExpandHeight(true));

        GUILayout.Space(5);
        // 右侧面板宽度（勿用 currentViewWidth，否则会撑满整窗）
        float ioWrapWidth = Mathf.Max(80f, rightPanelWidth - 22f);
        m_IOSection.Draw(ioWrapWidth);

        GUILayout.Space(5);

        m_MainTabIndex = m_MainTabToolbar.Draw();

        if (m_MainTabIndex == 0)
        {
            m_GridSubTabIndex = m_GridSubTabToolbar.Draw();
        }
        else
        {
            m_PortSubTabIndex = m_PortSubTabToolbar.Draw();
        }

        m_RightPanelScrollPos = EditorGUILayout.BeginScrollView(m_RightPanelScrollPos, false, false);
        EditorGUILayout.BeginVertical();

        GUILayout.Space(10);

        try
        {
            if (m_MainTabIndex == 0)
            {
                DrawGridTabs();
            }
            else
            {
                DrawPortTab();
            }
        }
        catch (ArgumentException) { }

        GUILayout.Space(20);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        int inputMode = m_MainTabIndex == 0 ? m_GridSubTabIndex : 1;
        m_GridView.HandleInput(gridAreaRect, inputMode);

        if (m_MainTabIndex == 1 && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (m_GridView.HandleEdgeHotspotClick(Event.current.mousePosition, gridAreaRect, true))
            {
                Event.current.Use();
            }
            else if (m_GridView.HandlePortClick(Event.current.mousePosition, gridAreaRect))
            {
                Event.current.Use();
            }
        }

        if (m_MainTabIndex == 1 && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            if (m_GridView.HandleEdgeHotspotClick(Event.current.mousePosition, gridAreaRect, false))
            {
                Event.current.Use();
            }
        }

        HandleKeyInput();
    }

    private void DrawGridToolbarOverlay(Rect gridRect)
    {
        float barW = Mathf.Min(420f, Mathf.Max(120f, gridRect.width - 8f));
        GUILayout.BeginArea(new Rect(gridRect.x + 4f, gridRect.y + 4f, barW, 26f));
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("回到原点", GUILayout.Width(64)))
        {
            m_GridView.ResetView();
            Repaint();
        }

        GUILayout.Space(4);

        EditorGUI.BeginChangeCheck();
        m_SystemData.CenterAlignment = GUILayout.Toggle(m_SystemData.CenterAlignment, "居中原点", GUILayout.Width(60));
        if (EditorGUI.EndChangeCheck())
        {
            SaveSystemData();
            OnDataChanged();
            Repaint();
        }

        GUILayout.Space(4);

        // 端口标签显示开关
        EditorGUI.BeginChangeCheck();
        m_SystemData.ShowPortLabels = GUILayout.Toggle(m_SystemData.ShowPortLabels, "端口标签", GUILayout.Width(65));
        if (m_SystemData.ShowPortLabels)
        {
            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();
            m_SystemData.PortLabelFontSize = EditorGUILayout.IntSlider(m_SystemData.PortLabelFontSize, 3, 14, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                SaveSystemData();
                Repaint();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            SaveSystemData();
            Repaint();
        }

        GUILayout.Space(4);

        GUI.color = ColorPalette.DangerButton;
        if (GUILayout.Button("清除", GUILayout.Width(44)))
        {
            if (EditorUtility.DisplayDialog("警告", "确定要清除所有网格数据吗？", "确定", "取消"))
            {
                m_GridManager.ClearAll();
                OnDataChanged();
            }
        }
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawGridTabs()
    {
        switch (m_GridSubTabIndex)
        {
            case 0: m_GridEditTab.Draw(); break;
            case 1: m_ParamsPreviewTab.Draw(); break;
            case 2: m_PropertiesTab.Draw(); break;
            case 3: m_DataOverviewTab.Draw(); break;
        }
    }

    private void DrawPortTab()
    {
        m_PortEditorTab.RefreshShapeCode();
        m_PortEditorTab.Draw(m_PortSubTabIndex);
    }

    private void HandleKeyInput()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && m_GridManager.SelectedCoords.Count > 0 && m_MainTabIndex == 0)
        {
            Vector2Int moveDir = Vector2Int.zero;
            bool isArrowKey = true;

            switch (e.keyCode)
            {
                case KeyCode.UpArrow: moveDir = Vector2Int.up; break;
                case KeyCode.DownArrow: moveDir = Vector2Int.down; break;
                case KeyCode.LeftArrow: moveDir = Vector2Int.left; break;
                case KeyCode.RightArrow: moveDir = Vector2Int.right; break;
                default: isArrowKey = false; break;
            }

            if (isArrowKey)
            {
                m_GridManager.MoveSelection(moveDir);
                OnDataChanged();
                e.Use();
            }
        }
    }

    private void SaveSystemData()
    {
        string directory = Path.GetDirectoryName(m_SystemDataPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(m_SystemDataPath, JsonUtility.ToJson(m_SystemData, true));
    }

    private void SaveTempData()
    {
        string directory = Path.GetDirectoryName(m_TempDataPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(m_TempDataPath, JsonUtility.ToJson(new GridSaveData { Cells = m_Cells }, true));
    }

    private void LoadSystemData()
    {
        if (File.Exists(m_SystemDataPath))
        {
            try
            {
                m_SystemData = JsonUtility.FromJson<SystemData>(File.ReadAllText(m_SystemDataPath));
                if (m_SystemData == null)
                    m_SystemData = new SystemData();
            }
            catch
            {
                m_SystemData = new SystemData();
            }
        }
        else
        {
            m_SystemData = new SystemData();
        }

        if (m_SystemData.GridTypes == null)
            m_SystemData.GridTypes = new List<string>();
        if (m_SystemData.Templates == null)
            m_SystemData.Templates = new List<GridCellData>();
        if (m_SystemData.TypeLocks == null)
            m_SystemData.TypeLocks = new List<bool>();
        if (m_SystemData.TemplateLocks == null)
            m_SystemData.TemplateLocks = new List<bool>();
        if (m_SystemData.PortLibrary == null)
            m_SystemData.PortLibrary = new PortLibraryData();
    }

    private void LoadTempData()
    {
        if (File.Exists(m_TempDataPath))
        {
            try
            {
                var data = JsonUtility.FromJson<GridSaveData>(File.ReadAllText(m_TempDataPath));
                m_Cells = data?.Cells ?? new List<GridCellData>();
            }
            catch
            {
                m_Cells = new List<GridCellData>();
            }
        }
        else
        {
            m_Cells = new List<GridCellData>();
        }
    }

    private void SyncIOShapeCode()
    {
        if (m_ShapeCodeEngine != null && m_IOSection != null)
            m_IOSection.UpdateCurrentShapeCode(m_ShapeCodeEngine.Generate(m_Cells));
    }

    private void ExportGridData()
    {
        var saveData = new GridSaveData { Cells = m_Cells };
        m_IOSection.ExportData(saveData);
    }

    private void LoadGridData()
    {
        var data = m_IOSection.ImportData();
        if (data != null)
        {
            m_Cells = data.Cells;
            if (m_GridManager != null)
                m_GridManager.SetCells(m_Cells);
            m_IOSection.UpdateCurrentShapeCode(m_ShapeCodeEngine.Generate(m_Cells));
            OnDataChanged();
        }
    }
}
