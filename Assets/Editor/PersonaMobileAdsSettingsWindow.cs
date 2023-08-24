using UnityEditor;
using UnityEngine;

public class PersonaMobileAdsSettingsWindow : EditorWindow
{
    private PersonaAdSDKConfig config;

    [MenuItem("Assets/Persona Mobile Ads/Settings...")]
    public static void ShowWindow()
    {
        GetWindow<PersonaMobileAdsSettingsWindow>("Persona Mobile Ads Settings");
    }

    private void OnGUI()
    {
        config = Resources.Load<PersonaAdSDKConfig>("Persona/PersonaAdSDKConfig");
        if (config == null)
        {
            config = PersonaAdSDKConfig.CreateConfig();

            if (config == null)
            {
                Debug.LogError("PersonaAdSDKConfig not found. Make sure the config exists.");
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Persona Mobile Ads", EditorStyles.boldLabel);

        GUILayout.BeginVertical("box");

        GUILayout.Space(10);

        GUILayout.Label("API Key:", EditorStyles.boldLabel);
        config.apiKey = EditorGUILayout.TextField(config.apiKey);

        GUILayout.Space(10);

        GUILayout.Label("Environment:", EditorStyles.boldLabel);
        config.environment = EditorGUILayout.TextField(config.environment);

        GUILayout.Space(20);

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(config); // Mark the ScriptableObject as dirty
            AssetDatabase.SaveAssets(); // Save the changes
            Close();
        }

        GUILayout.EndVertical();
    }
}
