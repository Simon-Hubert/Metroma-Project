using System;
using System.Collections.Generic;
using System.Linq;
using Metroma.Transitions;
using UnityEditor;
using UnityEngine;

namespace Metroma.Editor
{
    public class TransitionContextMenu : MonoBehaviour
    {
        [MenuItem("CONTEXT/ATransition/Add Transition/Parallel"), MenuItem("GameObject/Transition/Add Transition/Parallel")]
        private static void AddParallel(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ParallelTransition));

        [MenuItem("CONTEXT/ATransition/Add Transition/Series"), MenuItem("GameObject/Transition/Add Transition/Series")]
        private static void AddSeries(MenuCommand cmd) => AddTransitionChild(cmd, typeof(SeriesTransition));
        
        [MenuItem("CONTEXT/ATransition/Add Transition/ManyParallel"), MenuItem("GameObject/Transition/Add Transition/ManyParallel")]
        private static void AddManyParallel(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ManyParallelTransition));

        [MenuItem("CONTEXT/ATransition/Add Transition/ManySeries"), MenuItem("GameObject/Transition/Add Transition/ManySeries")]
        private static void AddManySeries(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ManySeriesTransition));

        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/CameraColor"), MenuItem("GameObject/Transition/Add Transition/CameraColor")]
        private static void AddCameraColor(MenuCommand cmd) => AddTransitionChild(cmd, typeof(CameraColorTransition));

        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/ColorShape"), MenuItem("GameObject/Transition/Add Transition/ColorShape")]
        private static void AddColorShape(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ColorShapeTransition));
        
        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/ColorSprite"), MenuItem("GameObject/Transition/Add Transition/ColorSprite")]
        private static void AddColorSprite(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ColorSpriteTransition));
        
        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/MMF"), MenuItem("GameObject/Transition/Add Transition/Mmf")]
        private static void AddMmf(MenuCommand cmd) => AddTransitionChild(cmd, typeof(MMFTransition));
        
        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/Spline"), MenuItem("GameObject/Transition/Add Transition/Spline")]
        private static void AddSpline(MenuCommand cmd) => AddTransitionChild(cmd, typeof(SplineTransition));
        
        [MenuItem("CONTEXT/ATransition/Add Transition/Specific/View"), MenuItem("GameObject/Transition/Add Transition/View")]
        private static void AddView(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ViewTransition));
        
        private static void AddTransitionChild(MenuCommand command, System.Type type)
        {
            ATransition transition = command.context as ATransition;
            if (transition == null)
            {
                transition = (command.context as GameObject)?.GetComponent<ATransition>();
                if (transition == null)
                {
                    Debug.LogError($"$[TransitionMenu] Can't find ATransition component on Object : {command.context}");
                    return;
                }
            }

            GameObject child = new GameObject(type.Name);
            child.transform.SetParent(transition.transform);
            child.transform.localPosition = Vector3.zero;

            child.AddComponent(type);
            Undo.RegisterCreatedObjectUndo(child, $"Add {type.Name}");
            Selection.activeObject = child;
            
            Debug.Log($"[Sequence] Ajout de {type.Name} à {transition.name}");
        }

        private static List<Type> GetAllDerivedTypes(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t))
                .ToList();
        }
    }
}