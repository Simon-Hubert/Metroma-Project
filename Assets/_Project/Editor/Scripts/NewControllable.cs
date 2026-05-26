using UnityEditor;
using UnityEngine;
using System.IO;

namespace Metroma.Editor
{
    public static class ScriptCreation
    {
        [MenuItem("Assets/Create/Metroma/Script/new AControllable", false, 80)]
        public static void CreateScript()
        {
            string path = GetSelectedPathOrFallback();

            string fileName = "NewControllable.cs";

            string fullPath = Path.Combine(path, fileName);

            string scriptContent = 
                @"using UnityEngine;

namespace Metroma
{
    public class NewControllable : AControllable
    {
        protected override void InputMoveStart(Vector2 move)
        {
            base.InputMoveStart(move);
        }
        protected override void InputMovePerformed(Vector2 move)
        {
            base.InputMovePerformed(move);
        }
        protected override void InputMoveEnd(Vector2 move)
        {
            base.InputMoveEnd(move);
        }

        protected override void InputActionStart(bool action)
        {
            base.InputActionStart(action);
        }
        protected override void InputActionPerformed(bool action)
        {
            base.InputActionPerformed(action);
        }
        protected override void InputActionEnd(bool action)
        {
            base.InputActionEnd(action);
        }
    }
}";

            File.WriteAllText(fullPath, scriptContent);

            AssetDatabase.Refresh();
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
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

