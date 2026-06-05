using ErmineGames.Utils;
using System;
using System.Collections.Generic;

namespace ErmineGames.Features
{
    public class FeatureMessageManager
    {
        public Action<FeatureRequest> OnRequestSent { get; set; }
        public Action<FeatureMessage> OnMessageSent { get; set; }
        public Action<FeatureMessage> OnMessageRead { get; set; }
        public Action<FeatureRequest> OnRequestInProgress { get; set; }
        public Action<FeatureRequest> OnRequestCompleted { get; set; }
        
        private readonly SequentialIDGenerator idGenerator = new();
        private readonly Dictionary<Type, MessageBox<FeatureRequest>> requestsByType = new();
        private readonly Dictionary<Type, MessageBox<FeatureMessage>> messagesByType = new();

        public void SendRequest<T>(T request) where T : FeatureRequest
        {
            var type = typeof(T);
            request.Id = idGenerator.Generate();
            
            if (!requestsByType.ContainsKey(type))
            {
                requestsByType.Add(type, new MessageBox<FeatureRequest>());
            }
            
            requestsByType[type].AddMessage(request);
            
            OnRequestSent?.Invoke(request);
        }

        public void SendMessage<T>(T message) where T : FeatureMessage
        {
            var type = typeof(T);
            message.Id = idGenerator.Generate();
            
            if (!messagesByType.ContainsKey(type))
            {
                messagesByType.Add(type, new MessageBox<FeatureMessage>());
            }
            
            messagesByType[type].AddMessage(message);
            
            OnMessageSent?.Invoke(message);
        }
        
        public void ProcessRequests<T>(Action<T> action) where T : FeatureRequest
        {
            if (!requestsByType.TryGetValue(typeof(T), out var box))
            {
                return;
            }

            foreach (var request in box.GetReadyContent())
            {
                if (request.Status != FeatureRequestStatus.NotProcessed)
                {
                    continue;
                }
                
                action?.Invoke((T)request);

                if (request.Status == FeatureRequestStatus.Completed)
                {
                    OnRequestCompleted?.Invoke(request);
                }
            }
        }
        
        public void ProcessMessages<T>(Action<T> action) where T : FeatureMessage
        {
            if (!messagesByType.TryGetValue(typeof(T), out var box))
            {
                return;
            }

            foreach (var message in box.GetReadyContent())
            {
                action?.Invoke((T)message);
                OnMessageRead?.Invoke(message);
            }
        }

        public void Update()
        {
            foreach (var box in requestsByType.Values)
            {
                box.Deliver(request => request.Status == FeatureRequestStatus.Completed);
            }
            
            foreach (var box in messagesByType.Values)
            {
                box.Deliver();
            }
        }
    }
}
