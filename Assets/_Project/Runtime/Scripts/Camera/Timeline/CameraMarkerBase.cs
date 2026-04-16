using System.ComponentModel;
using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Base class for all Camera Markers.
    /// Implements a Command Pattern to simplify adding new markers without editing the Core Rig.
    /// </summary>
    [Serializable]
    public abstract class CameraMarkerBase : Marker, INotification
    {
        public virtual PropertyName id => new PropertyName(GetType().Name);
        
        /// <summary> Method called when the marker is hit by the Timeline playhead. </summary>
        public abstract void Execute(CameraRig rig);
    }
}
