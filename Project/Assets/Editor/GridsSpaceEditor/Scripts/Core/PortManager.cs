using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GridsSpaceEditor.Data.Enums;
using GridsSpaceEditor.Data.Models;
using UnityEngine;
using UnityEditor;

namespace GridsSpaceEditor.Core
{
    public class PortManager
    {
        private PortLibraryData m_Library = new PortLibraryData();
        private GridCellData m_EditingCell = null;
        private PortInstance m_SelectedPort = null;

        private const string k_LibraryFileName = "PortLibrary.json";
        private string m_LibraryFilePath => Path.Combine(
            Application.dataPath,
            "Editor",
            "GridsSpaceEditor",
            "GridData",
            k_LibraryFileName
        );

        public IReadOnlyList<PortInstance> Templates => m_Library.Templates;
        public PortInstance SelectedPort => m_SelectedPort;
        public GridCellData EditingCell => m_EditingCell;

        public event Action OnLibraryChanged;
        public event Action<PortInstance> OnPortSelected;

        public PortManager()
        {
            LoadLibrary();
        }

        public void SetEditingCell(GridCellData cell)
        {
            if (m_EditingCell != cell)
            {
                m_EditingCell = cell;
                m_SelectedPort = null;
            }
        }

        public void SelectPort(PortInstance port)
        {
            m_SelectedPort = port;
            OnPortSelected?.Invoke(port);
        }

        public PortInstance AddPort(EdgeSide side)
        {
            if (m_EditingCell == null) return null;

            if (!m_EditingCell.Ports.Any(p => p.Side == side))
            {
                var port = new PortInstance { Side = side };
                m_EditingCell.Ports.Add(port);
                m_SelectedPort = port;
                OnPortSelected?.Invoke(port);
                return port;
            }
            return null;
        }

        public void RemovePort(EdgeSide side)
        {
            if (m_EditingCell == null) return;

            m_EditingCell.Ports.RemoveAll(p => p.Side == side);
            if (m_SelectedPort?.Side == side)
                m_SelectedPort = null;
        }

        public void RemovePort(PortInstance port)
        {
            if (m_EditingCell == null) return;

            m_EditingCell.Ports.Remove(port);
            if (m_SelectedPort == port)
                m_SelectedPort = null;
        }

        public bool HasPort(EdgeSide side)
        {
            return m_EditingCell?.Ports.Any(p => p.Side == side) ?? false;
        }

        public PortInstance GetPort(EdgeSide side)
        {
            return m_EditingCell?.Ports.FirstOrDefault(p => p.Side == side);
        }

        public void ApplyPreset(PortInstance preset)
        {
            if (m_SelectedPort == null) return;
            m_SelectedPort.SyncFrom(preset, preset.PortID);
        }

        public void SaveAsNewPreset(PortInstance source)
        {
            var preset = PortInstance.Clone(source, source.PortID);
            m_Library.Templates.Add(preset);
            SaveLibrary();
        }

        public void UpdatePreset(int index, PortInstance updated)
        {
            if (index >= 0 && index < m_Library.Templates.Count)
            {
                m_Library.Templates[index] = PortInstance.Clone(updated, updated.PortID);
                SaveLibrary();
            }
        }

        public void DeletePreset(int index)
        {
            if (index >= 0 && index < m_Library.Templates.Count)
            {
                m_Library.Templates.RemoveAt(index);
                SaveLibrary();
            }
        }

        public void CreateNewPreset(PortInstance preset)
        {
            m_Library.Templates.Add(preset);
            SaveLibrary();
        }

        public void LoadLibrary()
        {
            try
            {
                if (File.Exists(m_LibraryFilePath))
                {
                    string json = File.ReadAllText(m_LibraryFilePath);
                    var loaded = JsonUtility.FromJson<PortLibraryData>(json);
                    if (loaded != null)
                        m_Library = loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"加载端口库失败: {e.Message}");
            }
        }

        public void SaveLibrary()
        {
            try
            {
                string directory = Path.GetDirectoryName(m_LibraryFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonUtility.ToJson(m_Library, true);
                File.WriteAllText(m_LibraryFilePath, json);
                OnLibraryChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"保存端口库失败: {e.Message}");
            }
        }
    }
}
