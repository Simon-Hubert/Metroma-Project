using UnityEditor;
using UnityEngine;
using System.IO;

namespace Metroma.Editor
{
    public static class ATransitionCreation
    {
        [MenuItem("Assets/Create/Metroma/Script/new ATransition", false, 80)]
        public static void CreateScript()
        {
            string path = GetSelectedPathOrFallback();

            string fileName = "NewTransition.cs";

            string fullPath = Path.Combine(path, fileName);

            string scriptContent = 
                @"using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
        public class NewTransition : ATransition
    {
        protected override Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            throw new System.NotImplementedException();
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

