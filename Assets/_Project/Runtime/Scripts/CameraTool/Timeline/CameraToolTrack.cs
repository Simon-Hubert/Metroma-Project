using System.ComponentModel;
using Metroma.CameraTool;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Custom Timeline track that drives a <see cref="CameraTool"/> component.
    /// Accepts <see cref="CameraToolClip"/> clips and <see cref="CameraEventMarker"/> markers.
    /// </summary>
    [TrackColor(0.2f, 0.6f, 1f)]
    [TrackBindingType(typeof(CameraRig))]
    [TrackClipType(typeof(CameraToolClip))]
    public class CameraToolTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<CameraToolMixerBehaviour>.Create(graph, inputCount);
        }
    }
}
