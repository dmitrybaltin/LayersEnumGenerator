using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class LayersEnumGenerator : EditorWindow
{
    private const string DefaultOutputPath = "Assets/Scripts/Layers.cs";
    private const string DefaultNamespaceName = "GameNamespace";
    private const string DefaultEnumName = "Layers";

    private static string outputPath = DefaultOutputPath;
    private static string namespaceName = DefaultNamespaceName;
    private static string enumName = DefaultEnumName;

    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        // Load saved settings
        LoadPreferences();

        // Monitor changes in layers
        EditorApplication.delayCall += MonitorLayerChanges;
    }

    [MenuItem("Tools/Layer Auto Generator Settings")]
    public static void ShowSettingsWindow()
    {
        var window = GetWindow<LayersEnumGenerator>("Layer Generator Settings");
        window.minSize = new Vector2(400, 200);
    }

    [MenuItem("Tools/Generate Layers Enum")]
    public static void GenerateLayersEnum()
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var layers = new string[32];
        for (int i = 0; i < 32; i++)
            layers[i] = LayerMask.LayerToName(i);

        using (var writer = new StreamWriter(outputPath))
        {
            writer.WriteLine("// This file is auto-generated. Changes will be overwritten.");
            writer.WriteLine($"namespace {namespaceName}");
            writer.WriteLine("{");
            writer.WriteLine($"    public enum {enumName}");
            writer.WriteLine("    {");

            for (int i = 0; i < layers.Length; i++)
                if (!string.IsNullOrEmpty(layers[i]))
                {
                    // Replace spaces and invalid characters
                    var sanitizedLayerName = Regex.Replace(layers[i], @"[^a-zA-Z0-9_]", "_");
                    writer.WriteLine($"        {sanitizedLayerName} = {i},");
                }

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        // Refresh assets in the editor
        AssetDatabase.Refresh();
    }

    private static void MonitorLayerChanges()
    {
        var lastLayerState = GetLayerState();

        // Periodically check for layer changes
        EditorApplication.update += () =>
        {
            var currentLayerState = GetLayerState();
            if (currentLayerState != lastLayerState)
            {
                lastLayerState = currentLayerState;
                GenerateLayersEnum();
                Debug.Log("Layers Enum updated due to layer changes.");
            }
        };
    }

    private static string GetLayerState()
    {
        var layers = new string[32];
        for (int i = 0; i < 32; i++)
            layers[i] = LayerMask.LayerToName(i);

        // Create a hash or string representation of the layers
        return string.Join(",", layers);
    }

    private void OnGUI()
    {
        GUILayout.Label("Layer Auto Generator Settings", EditorStyles.boldLabel);

        // Path field
        EditorGUILayout.LabelField("Output Path", EditorStyles.label);
        outputPath = EditorGUILayout.TextField(outputPath);

        // Namespace name field
        EditorGUILayout.LabelField("Namespace Name", EditorStyles.label);
        namespaceName = EditorGUILayout.TextField(namespaceName);

        // Enum name field
        EditorGUILayout.LabelField("Enum Name", EditorStyles.label);
        enumName = EditorGUILayout.TextField(enumName);

        GUILayout.Space(10);

        if (GUILayout.Button("Save Settings"))
            SavePreferences();

        if (GUILayout.Button("Generate Layers Enum"))
        {
            GenerateLayersEnum();
            Debug.Log("Layers Enum generated.");
        }
    } 

    private static void SavePreferences()
    {
        EditorPrefs.SetString("LayerAutoGenerator_OutputPath", outputPath);
        EditorPrefs.SetString("LayerAutoGenerator_NamespaceName", namespaceName);
        EditorPrefs.SetString("LayerAutoGenerator_EnumName", enumName);
        Debug.Log("Layer Auto Generator settings saved.");
    }

    private static void LoadPreferences()
    {
        outputPath = EditorPrefs.GetString("LayerAutoGenerator_OutputPath", DefaultOutputPath);
        namespaceName = EditorPrefs.GetString("LayerAutoGenerator_NamespaceName", DefaultNamespaceName);
        enumName = EditorPrefs.GetString("LayerAutoGenerator_EnumName", DefaultEnumName);
    }
}
