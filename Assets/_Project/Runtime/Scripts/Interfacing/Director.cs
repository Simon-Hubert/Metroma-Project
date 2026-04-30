using System;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
using Metroma.Transitions;
using UnityEngine;
using UnityEngine.Playables;

namespace Metroma.Interfacing
{
    public class Director : MonoBehaviour
    {   
        [SerializeField] private CameraRig _tool;

        private void Start() {
            _tool.Sequences.PlayChapter(0);
        }
    }
}
