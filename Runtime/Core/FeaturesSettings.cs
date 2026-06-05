using System;
using System.Collections.Generic;
using UnityEngine;

namespace ErmineGames.Features
{
    [CreateAssetMenu(menuName = "Features/Features settings", fileName = "FeaturesSettings")]
    public class FeaturesSettings : ScriptableObject
    {
        public event Action<Type, bool> IsEnabledChanged;
        
        [SerializeField]
        private List<FeaturesSettingsRecord> features = new();
        
        public List<FeaturesSettingsRecord> Features => features;

        private void OnValidate()
        {
            // Если в список добавили новый элемент, инициализируем его
            if (features != null)
            {
                foreach (var record in features)
                {
                    if (record != null)
                    {
                        record.InitializeFeaturesSettingsAsset(this);
                    }
                }
            }
        }
    
        public void RaiseIsEnabledChanged(Type featureType, bool isEnabled)
        {
            IsEnabledChanged?.Invoke(featureType, isEnabled);
        }
    
        [System.Serializable]
        public class FeaturesSettingsRecord
        {   
            [SerializeField]
            [HideInInspector]
            private FeaturesSettings featuresSettingsAsset;
        
            [SerializeField]
            public bool IsEnabled = true;
        
            [SerializeField]
            public string FeatureTypeName = string.Empty; // Сохраняем имя типа вместо Type
        
            [SerializeReference]
            public FeatureSettings Settings;

            public Type Feature => string.IsNullOrEmpty(FeatureTypeName) ? null : Type.GetType(FeatureTypeName);

            public FeaturesSettingsRecord()
            {
            }
        
            public FeaturesSettingsRecord(FeaturesSettings featuresSettingsAsset)
            {
                this.featuresSettingsAsset = featuresSettingsAsset;
            }

            public void InitializeFeaturesSettingsAsset(FeaturesSettings asset)
            {
                if (featuresSettingsAsset == null)
                {
                    featuresSettingsAsset = asset;
                }
            }

            #if UNITY_EDITOR
            private void OnValidate()
            {
                if (featuresSettingsAsset != null && isEnabledChanged)
                {
                    featuresSettingsAsset.RaiseIsEnabledChanged(Feature, IsEnabled);
                    isEnabledChanged = false;
                }
            }

            [SerializeField]
            [HideInInspector]
            private bool isEnabledChanged = false;

            public void NotifyIsEnabledChanged()
            {
                isEnabledChanged = true;
            }

            private string GetLabel()
            {
                if (Feature == null)
                    return "None";
                return UnityEditor.ObjectNames.NicifyVariableName(Feature.Name.Replace("Feature", string.Empty));
            }
            #endif

            private bool SettingsTypeFilter(Type settingsType)
            {
                var featureType = Feature;
                if (!(featureType?.BaseType?.GenericTypeArguments.Length > 0))
                {
                    return false;
                }
            
                return featureType.BaseType.GenericTypeArguments[0] == settingsType;
            }
        }
    }
}
