using System;
using UnityEngine;

namespace ErmineGames.Features
{
    public abstract class FeatureRequest : FeatureMessageBase
    {
        public DateTime CreationTime { get; } = DateTime.Now;
        public DateTime ProcessedTime { get; private set; }
        
        public FeatureRequestStatus Status { get; private set; }

        public void InProgress()
        {
            if (Status != FeatureRequestStatus.NotProcessed)
            {
                Debug.LogWarning("Request is already in progress or completed");
                return;
            }
            
            Status = FeatureRequestStatus.InProgress; 
        }

        public void Complete()
        {
            if (Status == FeatureRequestStatus.Completed)
            {
                Debug.LogWarning("Request is already completed");
                return;
            }
            
            Status = FeatureRequestStatus.Completed; 
            ProcessedTime = DateTime.Now;
        }
    }
}