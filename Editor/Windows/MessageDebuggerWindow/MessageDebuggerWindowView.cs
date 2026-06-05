using ErmineGames.Features;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ErminGames.Features.Editor
{
    public partial class MessageDebuggerWindow
    {
        private void CreateView()
        {
            var splitView = new TwoPaneSplitView(0, 395, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

            CreateRecordsView(splitView);
            CreateDetailsView(splitView);
        }

        private void CreateRecordsView(VisualElement parent)
        {
            var recordsContainer = new VisualElement();
            recordsContainer.AddToClassList("records-container");
            parent.Add(recordsContainer);

            CreateRecordsSettings(recordsContainer);
            CreateRecordsErrorLabel(recordsContainer);
            CreateRecordsList(recordsContainer);
        }

        private void CreateRecordsSettings(VisualElement parent)
        {
            var settingsContainer = new VisualElement();
            settingsContainer.AddToClassList("records-settings");
            parent.Add(settingsContainer);

            liveUpdateToggle = new Toggle();
            liveUpdateToggle.AddToClassList("records-live-update");
            liveUpdateToggle.text = "Live Update";
            liveUpdateToggle.value = EditorPrefs.GetBool(LIVE_UPDATE_PREFS_NAME);
            settingsContainer.Add(liveUpdateToggle);

            recordsFilter = new ToolbarSearchField();
            recordsFilter.AddToClassList("records-filter");
            settingsContainer.Add(recordsFilter);
        }

        private void CreateRecordsErrorLabel(VisualElement parent)
        {
            recordsErrorLabel = new Label();
            recordsErrorLabel.AddToClassList("records-list-error-label");
            recordsErrorLabel.enabledSelf = false;
            parent.Add(recordsErrorLabel);
        }

        private void CreateRecordsList(VisualElement parent)
        {
            recordsList = new MultiColumnListView();
            recordsList.AddToClassList("records-list");

            recordsList.columns.Add(new Column
            {
                title = "Record",
                width = 200,
                makeCell = () =>
                {
                    var element = new VisualElement();
                    element.AddToClassList("record-name-container");

                    var recordIcon = new Label();
                    recordIcon.name = "recordIcon";
                    element.Add(recordIcon);

                    var recordLabel = new Label();
                    recordLabel.name = "recordLabel";
                    recordLabel.AddToClassList("record-name");
                    element.Add(recordLabel);

                    return element;
                },
                bindCell = (element, index) =>
                {
                    var recordIcon = element.Q<Label>("recordIcon");
                    var recordLabel = element.Q<Label>("recordLabel");

                    recordIcon.ClearClassList();
                    recordIcon.AddToClassList(GetRecordIconStyle(boundRecords[index]));

                    recordLabel.text = FormatRecordName(boundRecords[index]);
                }
            });

            recordsList.columns.Add(new Column
            {
                title = "Type",
                width = 80,
                makeCell = () =>
                {
                    var cell = new Label();
                    cell.AddToClassList("records-list-cell");
                    return cell;
                },
                bindCell = (element, index) =>
                {
                    var typeLabel = element as Label;
                    typeLabel.text = boundRecords[index].ContentType.ToString();

                }
            });

            recordsList.columns.Add(new Column
            {
                title = "Time",
                width = 100,
                makeCell = () =>
                {
                    var cell = new Label();
                    cell.AddToClassList("records-list-cell");
                    return cell;
                },
                bindCell = (element, index) => { (element as Label).text = FormatTime(boundRecords[index].RecordTime); }
            });

            recordsList.selectionType = SelectionType.Single;
            recordsList.selectionChanged += SetDetailsViewContent;

            parent.Add(recordsList);
        }

        private void CreateDetailsView(VisualElement parent)
        {
            detailsView = new VisualElement();
            detailsView.AddToClassList("details-container");
            parent.Add(detailsView);
        }

        private void SetDetailsViewContent(IEnumerable<object> objects)
        {
            detailsView.Clear();

            if (recordsList.selectedItem is not JournalRecord record)
            {
                return;
            }

            CreateDetailsHeader(detailsView, record);
            CreateDetailsData(detailsView, record);
        }

        private void CreateDetailsHeader(VisualElement parent, JournalRecord record)
        {
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("details-header-area");
            parent.Add(headerContainer);

            var createdByFeatureLabel = new Label();
            createdByFeatureLabel.text = $"Created by feature: {record.CreatedByFeature}";
            createdByFeatureLabel.AddToClassList("details-header-label");
            headerContainer.Add(createdByFeatureLabel);

            if (record.ContentType == JournalContentType.Message)
            {
                if (record.ReadByFeatures is { Count: > 0 })
                {
                    var readByFeaturesLabel = new Label();
                    readByFeaturesLabel.text = record.ReadByFeatures.Count == 1
                        ? $"Read by feature: {record.ReadByFeatures[0]}"
                        : $"Read by features:\n  {string.Join("\n  ", record.ReadByFeatures)}";
                    readByFeaturesLabel.AddToClassList("details-header-label");
                    headerContainer.Add(readByFeaturesLabel);
                }
            }

            if (record.ContentType == JournalContentType.Request)
            {
                var requestCreationTimeLabel = new Label();
                requestCreationTimeLabel.text = $"Creation time: {FormatTime(record.RequestCreationTime)}";
                requestCreationTimeLabel.AddToClassList("details-header-label");
                headerContainer.Add(requestCreationTimeLabel);

                var requestProcessedTimeLabel = new Label();
                requestProcessedTimeLabel.text = $"Processed time: {FormatTime(record.RequestProcessedTime)}";
                requestProcessedTimeLabel.AddToClassList("details-header-label");
                headerContainer.Add(requestProcessedTimeLabel);

                var processedByFeatureLabel = new Label();
                processedByFeatureLabel.text = $"Processed by feature: {record.ProcessedByFeature}";
                processedByFeatureLabel.AddToClassList("details-header-label");
                headerContainer.Add(processedByFeatureLabel);
            }
        }

        private void CreateDetailsData(VisualElement parent, JournalRecord record)
        {
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("details-data-area");
            parent.Add(dataContainer);

            var formatedDataLabel = new Label();
            formatedDataLabel.text = FormatReflectedData(record.ReflectedData, true);
            formatedDataLabel.AddToClassList("details-data-label");
            dataContainer.Add(formatedDataLabel);
        }

        private string GetRecordIconStyle(JournalRecord record)
        {
            if (record.ContentType != JournalContentType.Request)
            {
                return "record-icon-message";
            }

            return record.RequestProcessedTime == default
                ? "record-icon-request-not-processed"
                : "record-icon-request-processed";
        }
    }
}
