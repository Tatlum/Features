using ErmineGames.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ErmineGames.Features
{
    public class DebugMessageFeature : Feature
    {
        private GameObject debugger;
        private DebugMessageJournalComponent debugMessageComponent;
        private Type[] superficiallyReflectTypes =
        {
            typeof(FeatureMessageBase),
            typeof(MonoBehaviour)
        };

        protected override void OnEnabled()
        {
            debugger = new GameObject(nameof(DebugMessageJournal));
            debugMessageComponent = debugger.AddComponent<DebugMessageJournalComponent>();
            debugMessageComponent.Journal = new DebugMessageJournal();
            
            sharedData.Message.OnMessageSent += OnMessageSent;
            sharedData.Message.OnRequestSent += OnRequestSent;
            sharedData.Message.OnMessageRead += OnMessageRead;
            sharedData.Message.OnRequestCompleted += OnRequestProcessed;
        }

        protected override void OnDisabled()
        {
            if (debugger != null)
            {
                Object.Destroy(debugger);
            }
            
            sharedData.Message.OnMessageSent -= OnMessageSent;
            sharedData.Message.OnRequestSent -= OnRequestSent;
            sharedData.Message.OnMessageRead -= OnMessageRead;
            sharedData.Message.OnRequestCompleted -= OnRequestProcessed;
        }

        private void OnMessageSent(FeatureMessage message)
        {
            debugMessageComponent.Journal.AddRecord(message.Id, CreateJournalRecord(message));
        }

        private void OnRequestSent(FeatureRequest request)
        {
            debugMessageComponent.Journal.AddRecord(request.Id, CreateJournalRecord(request));
        }

        private void OnMessageRead(FeatureMessage message)
        {
            if (!debugMessageComponent.Journal.TryGetRecord(message.Id, out var record))
            {
                return;
            }

            record.ReadByFeatures ??= new List<string>();
            record.ReadByFeatures.Add(ReflectionUtils.FindCallerInStack(typeof(Feature), 3)?.Name);
        }

        private void OnRequestProcessed(FeatureRequest request)
        {
            if (!debugMessageComponent.Journal.TryGetRecord(request.Id, out var record))
            {
                return;
            }

            record.RequestProcessedTime = request.ProcessedTime;
            record.ProcessedByFeature = ReflectionUtils.FindCallerInStack(typeof(Feature), 3)?.Name;
        }

        private JournalRecord CreateJournalRecord<T>(T messageBase) where T : FeatureMessageBase
        {
            var record = new JournalRecord
            {
                RecordId = messageBase.Id,
                RecordTime = DateTime.Now,
                DeclaringType = messageBase.GetType().Name,
                ReflectedData = ReflectionUtils.ReflectType(messageBase, superficiallyReflectTypes),
                CreatedByFeature = ReflectionUtils.FindCallerInStack(typeof(Feature), 4)?.Name
            };

            switch (messageBase)
            {
                case FeatureMessage:
                    record.ContentType = JournalContentType.Message;
                    break;
                case FeatureRequest request:
                    record.ContentType = JournalContentType.Request;
                    record.RequestCreationTime = request.CreationTime;
                    break;
            }
            
            return record;
        }
    }
}
