using ErmineGames.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace ErminGames.Features.Editor
{
    public partial class MessageDebuggerWindow : EditorWindow
    {
        private const string LIVE_UPDATE_PREFS_NAME = "MessageDebuggerWindow.LiveUpdate";
        private const int SEARCH_STRING_MIN_LENGTH = 2;
        private const char SEARCH_SYMBOL_END_OF_STRING = ']';
        
        private StyleSheet styleSheet;
        private DebugMessageJournalComponent journalComponent;
        private Toggle liveUpdateToggle;
        private ToolbarSearchField recordsFilter;
        private MultiColumnListView recordsList;
        private Label recordsErrorLabel;
        private List<JournalRecord> allRecords;
        private List<JournalRecord> boundRecords;
        private VisualElement detailsView;
        private bool isFilterEnabled;
        private bool isFilterResultOutdated;
        private string filterTextFormatted;
        private Dictionary<int, bool> filterCache = new();

        [MenuItem("Window/Analysis/Message Debugger %M", priority = 200)]
        public static void CreateWindow()
        {
            var window = GetWindow<MessageDebuggerWindow>();
            window.titleContent = new GUIContent("Message Debugger");
        }

        private void OnEnable()
        {
            isFilterEnabled = false;
            isFilterResultOutdated = false;
            filterTextFormatted = string.Empty;
        }

        public void CreateGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (!rootVisualElement.styleSheets.Contains(styleSheet))
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            CreateView();
            RefreshJournal();
            RefreshRecordList();

            liveUpdateToggle.RegisterValueChangedCallback(OnLiveUpdateValueChanged);
            recordsFilter.RegisterValueChangedCallback(OnRecordsFilterValueChanged);
        }

        private void OnInspectorUpdate()
        {
            recordsErrorLabel.text = string.Empty;

            if (EditorApplication.isPlaying &&
                liveUpdateToggle.value)
            {
                RefreshJournal();
                isFilterResultOutdated = true;
            }

            RefreshRecordList();

            recordsErrorLabel.enabledSelf = recordsErrorLabel.text != string.Empty;
        }

        private void OnDisable()
        {
            liveUpdateToggle.UnregisterValueChangedCallback(OnLiveUpdateValueChanged);
            recordsFilter.UnregisterValueChangedCallback(OnRecordsFilterValueChanged);
        }

        private void OnLiveUpdateValueChanged(ChangeEvent<bool> callback)
        {
            EditorPrefs.SetBool(LIVE_UPDATE_PREFS_NAME, callback.newValue);
        }

        private void OnRecordsFilterValueChanged(ChangeEvent<string> callback)
        {
            var filterText = callback.newValue;
            isFilterEnabled = filterText.Length >= SEARCH_STRING_MIN_LENGTH;
            filterTextFormatted = FormatFilterText(filterText);
            isFilterResultOutdated = true;
            filterCache.Clear();
        }

        private string FormatFilterText(string filterText)
        {
            filterText = filterText.ToLower();

            if (filterText.EndsWith(SEARCH_SYMBOL_END_OF_STRING))
            {
                filterText = filterText[..^1] + Environment.NewLine;
            }

            return filterText;
        }

        private void RefreshJournal()
        {
            if (journalComponent == null)
            {
                journalComponent = FindFirstObjectByType<DebugMessageJournalComponent>();
            }

            if (journalComponent == null)
            {
                recordsErrorLabel.text = "Can't find DebugMessageJournal component";
                return;
            }

            allRecords = journalComponent.Journal.GetRecords().ToList();
        }

        private void RefreshRecordList()
        {
            switch (isFilterEnabled)
            {
                case true when isFilterResultOutdated:
                    boundRecords = FilterJournalRecords(allRecords);
                    break;
                case false when boundRecords != allRecords:
                    boundRecords = allRecords;
                    break;
                default:
                    return;
            }

            recordsList.itemsSource = boundRecords;
            recordsList.RefreshItems();
        }

        private List<JournalRecord> FilterJournalRecords(List<JournalRecord> journalRecords)
        {
            var filteredRecords = new List<JournalRecord>();

            foreach (var record in journalRecords)
            {
                if (!filterCache.TryGetValue(record.RecordId, out var isMatch))
                {
                    isMatch = IsMatchFilter(FormatReflectedData(record.ReflectedData, false), filterTextFormatted);
                    filterCache.Add(record.RecordId, isMatch);
                }

                if (isMatch)
                {
                    filteredRecords.Add(record);
                }
            }

            isFilterResultOutdated = false;
            return filteredRecords;
        }

        private bool IsMatchFilter(string data, string filter)
        {
            return data.ToLower().Contains(filter.ToLower());
        }
    }
}