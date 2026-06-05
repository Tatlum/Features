using UnityEngine;

namespace ErmineGames.Features
{
    public abstract class Feature
    {
        public bool IsEnabled { get; private set; }
        
        protected FeaturesRuntimeSharedData sharedData;

        public void SetInitialParameters(FeaturesRuntimeSharedData featuresRuntimeSharedData)
        {
            sharedData = featuresRuntimeSharedData;
            Initialize();
        }
        
        public virtual void Initialize() { }

        public void Enable()
        {
            if (IsEnabled)
            {
                Debug.LogError("The feature is already enabled.");
                return;
            }
            
            IsEnabled = true;
            OnEnabled();
        }

        public void Disable()
        {
            if (!IsEnabled)
            {
                Debug.LogError("The feature is already disabled.");
                return;
            }
            
            IsEnabled = false;
            OnDisabled();
        }
        
        protected virtual void OnEnabled() {}
        
        protected virtual void OnDisabled() {}

        public virtual void Update() { }

        public virtual void FixedUpdate() { }
    }
    
    public abstract class Feature<TSettings> : Feature where TSettings : FeatureSettings
    {
        protected TSettings settings;

        public void SetInitialParameters(FeaturesRuntimeSharedData featuresRuntimeSharedData, 
            TSettings featureSettings)
        {
            settings = featureSettings;
            SetInitialParameters(featuresRuntimeSharedData);
        }
    }
}
