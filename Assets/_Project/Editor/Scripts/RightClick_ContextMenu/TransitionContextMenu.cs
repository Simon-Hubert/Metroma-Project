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
        [MenuItem("CONTEXT/Sequence/Add Transition/Parallel")]
        private static void AddParallel(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ParallelTransition));

        [MenuItem("CONTEXT/Sequence/Add Transition/Series")]
        private static void AddSeries(MenuCommand cmd) => AddTransitionChild(cmd, typeof(SeriesTransition));
        
        [MenuItem("CONTEXT/Sequence/Add Transition/ManyParallel")]
        private static void AddManyParallel(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ManyParallelTransition));

        [MenuItem("CONTEXT/Sequence/Add Transition/ManySeries")]
        private static void AddManySeries(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ManySeriesTransition));

        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/CameraColor")]
        private static void AddCameraColor(MenuCommand cmd) => AddTransitionChild(cmd, typeof(CameraColorTransition));

        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/ColorShape")]
        private static void AddColorShape(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ColorShapeTransition));
        
        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/ColorSprite")]
        private static void AddColorSprite(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ColorSpriteTransition));
        
        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/MMF")]
        private static void AddMmf(MenuCommand cmd) => AddTransitionChild(cmd, typeof(MMFTransition));
        
        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/Spline")]
        private static void AddSpline(MenuCommand cmd) => AddTransitionChild(cmd, typeof(SplineTransition));
        
        [MenuItem("CONTEXT/Sequence/Add Transition/Specific/View")]
        private static void AddView(MenuCommand cmd) => AddTransitionChild(cmd, typeof(ViewTransition));
        
        private static void AddTransitionChild(MenuCommand command, System.Type type)
        {
            Sequence sequence = command.context as Sequence;
            if (sequence == null) return;

            GameObject child = new GameObject(type.Name);
            child.transform.SetParent(sequence.transform);
            child.transform.localPosition = Vector3.zero;

            child.AddComponent(type);
            Undo.RegisterCreatedObjectUndo(child, $"Add {type.Name}");
            Selection.activeObject = child;
            
            Debug.Log($"[Sequence] Ajout de {type.Name} à {sequence.name}");
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
