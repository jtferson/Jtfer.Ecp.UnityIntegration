using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Jtfer.Ecp.Unity.Editor.Templates
{
    /// <summary>
    /// Generates templates of ecs user classes.
    /// </summary>
    sealed class TemplateGenerator : ScriptableObject
    {
        const string Title = "ECP template generator";

        const string StartupTemplate = "Startup.cs.txt";
        const string InitOperationTemplate = "InitOperation.cs.txt";
        const string UpdateOperationTemplate = "UpdateOperation.cs.txt";
        const string ComponentTemplate = "Component.cs.txt";
        const string ComponentFlagTemplate = "ComponentFlag.cs.txt";
        const string ComponentOneFrameTemplate = "ComponentOneFrame.cs.txt";

        [MenuItem("Assets/Create/ECP/Create Startup template", false, -200)]
        static void CreateStartupTpl()
        {
            var assetPath = GetAssetPath();
            CreateAndRenameAsset(
                string.Format("{0}/EcpStartup.cs", assetPath),
                GetIcon(), name => {
                    if (CreateTemplateInternal(GetTemplateContent(StartupTemplate), name) == null)
                    {
                        if (EditorUtility.DisplayDialog(Title, "Create data folders?", "Yes", "No"))
                        {
                            CreateEmptyFolder(string.Format("{0}/Components", assetPath));
                            CreateEmptyFolder(string.Format("{0}/Operations", assetPath));
                            CreateEmptyFolder(string.Format("{0}/UnityComponents", assetPath));
                            CreateEmptyFolder(string.Format("{0}/Services", assetPath));
                            AssetDatabase.Refresh();
                        }
                    }
                });
        }

        static void CreateEmptyFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    File.Create(string.Format("{0}/.gitkeep", folderPath));
                }
                catch { }
            }
        }

        [MenuItem("Assets/Create/ECP/Systems/Create InitOperation template", false, -199)]
        static void CreateInitSystemTpl()
        {
            CreateAndRenameAsset(
                string.Format("{0}/InitOperation.cs", GetAssetPath()),
                GetIcon(), name => CreateTemplateInternal(GetTemplateContent(InitOperationTemplate), name));
        }

        [MenuItem("Assets/Create/ECP/Systems/Create UpdateOperation template", false, -198)]
        static void CreateRunSystemTpl()
        {
            CreateAndRenameAsset(
                string.Format("{0}/UpdateOperation.cs", GetAssetPath()),
                GetIcon(), name => CreateTemplateInternal(GetTemplateContent(UpdateOperationTemplate), name));
        }

        [MenuItem("Assets/Create/ECP/Components/Create Component (common) template", false, -197)]
        static void CreateComponentTpl()
        {
            CreateAndRenameAsset(
                string.Format("{0}/EcpComponent.cs", GetAssetPath()),
                GetIcon(), name => CreateTemplateInternal(GetTemplateContent(ComponentTemplate), name));
        }

        [MenuItem("Assets/Create/ECP/Components/Create Component (no-data) template", false, -196)]
        static void CreateComponentFlagTpl()
        {
            CreateAndRenameAsset(
                string.Format("{0}/EcpComponentFlag.cs", GetAssetPath()),
                GetIcon(), name => CreateTemplateInternal(GetTemplateContent(ComponentFlagTemplate), name));
        }

        [MenuItem("Assets/Create/ECP/Components/Create OneFrame Component template", false, -195)]
        static void CreateOneFrameComponentTpl()
        {
            CreateAndRenameAsset(
                string.Format("{0}/EcpOneFrameComponent.cs", GetAssetPath()),
                GetIcon(), name => CreateTemplateInternal(GetTemplateContent(ComponentOneFrameTemplate), name));
        }

        public static string CreateTemplate(string proto, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "Invalid filename";
            }
            var ns = EditorSettings.projectGenerationRootNamespace.Trim();
            if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace))
            {
                ns = "Client";
            }
            proto = proto.Replace("#NS#", ns);
            proto = proto.Replace("#SCRIPTNAME#", SanitizeClassName(Path.GetFileNameWithoutExtension(fileName)));
            try
            {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), proto);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            AssetDatabase.Refresh();
            return null;
        }

        static string SanitizeClassName(string className)
        {
            var sb = new StringBuilder();
            var needUp = true;
            foreach (var c in className)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(needUp ? char.ToUpperInvariant(c) : c);
                    needUp = false;
                }
                else
                {
                    needUp = true;
                }
            }
            return sb.ToString();
        }

        static string CreateTemplateInternal(string proto, string fileName)
        {
            var res = CreateTemplate(proto, fileName);
            if (res != null)
            {
                EditorUtility.DisplayDialog(Title, res, "Close");
            }
            return res;
        }

        static string GetTemplateContent(string proto)
        {
            // hack: its only one way to get current editor script path. :(
            var pathHelper = ScriptableObject.CreateInstance<TemplateGenerator>();
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(pathHelper)));
            UnityEngine.Object.DestroyImmediate(pathHelper);
            try
            {
                return File.ReadAllText(Path.Combine(path, proto));
            }
            catch
            {
                return null;
            }
        }

        static string GetAssetPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path) && AssetDatabase.Contains(Selection.activeObject))
            {
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = Path.GetDirectoryName(path);
                }
            }
            else
            {
                path = "Assets";
            }
            return path;
        }

        static Texture2D GetIcon()
        {
            return EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        }

        static void CreateAndRenameAsset(string fileName, Texture2D icon, Action<string> onSuccess)
        {
            var action = ScriptableObject.CreateInstance<CustomEndNameAction>();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, fileName, icon, null);
        }

        sealed class CustomEndNameAction : EndNameEditAction
        {
            public Action<string> Callback;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                if (Callback != null)
                {
                    Callback(pathName);
                }
            }
        }
    }
}
