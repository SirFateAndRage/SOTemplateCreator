using UnityEngine;
using UnityEditor;
using System.IO;

namespace SOCreator.Editor
{
    /// <summary>
    /// Editor tool to quickly create custom ScriptableObjects
    /// </summary>
    public class SOTemplateCreator: EditorWindow
    {
        private string scriptName = "NewScriptableObject";
        private string menuPath = "ScriptableObjects";
        private bool addHeader = true;
        private bool addOnEnable = true;
        private bool addOnValidate = true;
        private bool addSampleFields = true;

        [MenuItem("Assets/Create/SO Template Creator", priority = 80)]
        private static void CreateScriptableObjectScript()
        {
            ShowWindow();
        }

        [MenuItem("Tools/ScriptableObject Creator")]
        private static void ShowWindow()
        {
            var window = GetWindow<SOTemplateCreator>("SO Creator");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ScriptableObject Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            scriptName = EditorGUILayout.TextField("Script Name:", scriptName);
            menuPath = EditorGUILayout.TextField("Menu Path:", menuPath);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Options:", EditorStyles.boldLabel);

            addHeader = EditorGUILayout.Toggle("Add [Header] attribute", addHeader);
            addOnEnable = EditorGUILayout.Toggle("Add OnEnable method", addOnEnable);
            addOnValidate = EditorGUILayout.Toggle("Add OnValidate method", addOnValidate);
            addSampleFields = EditorGUILayout.Toggle("Add sample fields", addSampleFields);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Create ScriptableObject Script", GUILayout.Height(40)))
            {
                CreateScript();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This will create a new ScriptableObject with [CreateAssetMenu] configured.\n" +
                "The file will be saved in the currently selected folder.",
                MessageType.Info);
        }

        private void CreateScript()
        {
            if (string.IsNullOrWhiteSpace(scriptName))
            {
                EditorUtility.DisplayDialog("Error", "Script name cannot be empty.", "OK");
                return;
            }

            scriptName = scriptName.Replace(" ", "");

            string path = GetSelectedPath();
            string fullPath = Path.Combine(path, scriptName + ".cs");

            if (File.Exists(fullPath))
            {
                if (!EditorUtility.DisplayDialog("File exists",
                    $"The file {scriptName}.cs already exists. Do you want to overwrite it?",
                    "Yes", "No"))
                {
                    return;
                }
            }

            string content = GenerateScriptContent();

            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log($"✓ ScriptableObject created: {fullPath}");
        }

        private string GenerateScriptContent()
        {
            string content = "using UnityEngine;\n\n";
            content += $"[CreateAssetMenu(fileName = \"{scriptName}\", menuName = \"{menuPath}/{scriptName}\")]\n";
            content += $"public class {scriptName} : ScriptableObject\n";
            content += "{\n";

            if (addSampleFields)
            {
                if (addHeader)
                {
                    content += "    [Header(\"Settings\")]\n";
                }
                content += "    [SerializeField] private string displayName;\n";
                content += "    [SerializeField] private int value;\n\n";

                content += "    // Add your fields here\n";
            }

            if (addOnEnable || addOnValidate)
            {
                content += "\n";
            }

            if (addOnEnable)
            {
                content += "    private void OnEnable()\n";
                content += "    {\n";
                content += "        // Initialization when the asset is loaded\n";
                content += "    }\n";
            }

            if (addOnValidate)
            {
                if (addOnEnable)
                {
                    content += "\n";
                }
                content += "    private void OnValidate()\n";
                content += "    {\n";
                content += "        // Validation in the editor\n";
                content += "    }\n";
            }

            content += "}\n";

            return content;
        }

        private string GetSelectedPath()
        {
            string path = "Assets";

            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }

            return path;
        }
    }
}
