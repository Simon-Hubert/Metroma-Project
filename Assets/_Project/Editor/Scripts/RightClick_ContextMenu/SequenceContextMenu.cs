using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metroma;

namespace Metroma.Editor
{
    public class SequenceContextMenu
    {
        [MenuItem("CONTEXT/Sequence/Add Sequence/Spline"), MenuItem("GameObject/Sequence/Add Sequence/Spline")]
        private static void AddSpline(MenuCommand cmd) => AddSequenceChild(cmd, typeof(SplineSequence));
        
        [MenuItem("CONTEXT/Sequence/Add Sequence/CutToBlack"), MenuItem("GameObject/Sequence/Add Sequence/CutToBlack")]
        private static void AddCutToBlack(MenuCommand cmd) => AddSequenceChild(cmd, typeof(CutToBlackSequence));
        
        [MenuItem("CONTEXT/Sequence/Add Sequence/ApplyView"), MenuItem("GameObject/Sequence/Add Sequence/ApplyView")]
        private static void AddApplyView(MenuCommand cmd) => AddSequenceChild(cmd, typeof(ApplyViewSequence));
        
        [MenuItem("CONTEXT/Sequence/Add Sequence/ActivateObject"), MenuItem("GameObject/Sequence/Add Sequence/ActivateObject")]
        private static void AddActivateObject(MenuCommand cmd) => AddSequenceChild(cmd, typeof(ActivateObject));

        [MenuItem("CONTEXT/Sequence/Add Sequence/EnableBehaviour"), MenuItem("GameObject/Sequence/Add Sequence/EnableBehaviour")]
        private static void AddEnableBehaviour(MenuCommand cmd) => AddSequenceChild(cmd, typeof(EnableBehaviour));

        [MenuItem("CONTEXT/Sequence/Add Sequence/PlayFeedBack"), MenuItem("GameObject/Sequence/Add Sequence/PlayFeedBack")]
        private static void AddPlayFeedBack(MenuCommand cmd) => AddSequenceChild(cmd, typeof(PlayFeedBack));

        [MenuItem("CONTEXT/Sequence/Add Sequence/TransiSequencable"), MenuItem("GameObject/Sequence/Add Sequence/TransiSequencable")]
        private static void AddTransiSequencable(MenuCommand cmd) => AddSequenceChild(cmd, typeof(TransiSequencable));

        [MenuItem("CONTEXT/Sequence/Add Sequence/WaitForDuration"), MenuItem("GameObject/Sequence/Add Sequence/WaitForDuration")]
        private static void AddWaitForDuration(MenuCommand cmd) => AddSequenceChild(cmd, typeof(WaitForDuration));

        [MenuItem("CONTEXT/Sequence/Add Sequence/Sequence"), MenuItem("GameObject/Sequence/Add Sequence/Sequence")]
        private static void AddSequenceScript(MenuCommand cmd) => AddSequenceChild(cmd, typeof(Sequence));
        
        private static void AddSequenceChild(MenuCommand command, System.Type type)
        {
            Sequence sequence = command.context as Sequence;
            if (sequence == null)
            {
                sequence = (command.context as GameObject)?.GetComponent<Sequence>();
                if (sequence == null)
                {
                    Debug.LogError($"$[SequenceMenu] Can't find Sequence component on Object : {command.context}");
                    return;
                }
            }

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
