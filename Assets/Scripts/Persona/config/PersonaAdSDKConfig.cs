using System.IO;
using UnityEngine;

namespace IO.Persona.MobileAds.Unity
{
    public class PersonaAdSDKConfig : ScriptableObject
    {
        public string apiKey = "";
        public string environment = "";

        public static PersonaAdSDKConfig? CreateConfig()
        {
            PersonaAdSDKConfig config = null;
#if UNITY_EDITOR
            string folderPath = "Assets/Resources/Persona";
            // Ensure that the parent directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            config = CreateInstance<PersonaAdSDKConfig>();

            // Combine the provided folder path with the asset name
            string assetPath = Path.Combine(folderPath, "PersonaAdSDKConfig.asset");

            UnityEditor.AssetDatabase.CreateAsset(config, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            return config;
        }
    }
}