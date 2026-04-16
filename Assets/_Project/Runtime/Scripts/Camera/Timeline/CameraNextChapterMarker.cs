using System;
using System.ComponentModel;
using UnityEngine;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker that triggers a transition to another cinematic chapter.
    /// Great for chaining different Timelines together smoothly.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/⏭ Next Chapter")]
    public class CameraNextChapterMarker : CameraMarkerBase
    {
        [Tooltip("Exact name of the chapter to play (from the Sequences module Chapters list).")]
        public string nextChapterName;

        [Tooltip("Duration of the blend from the current position to the start of the next chapter.")]
        public float blendDuration = 2.0f;

        [Tooltip("What should the camera look at during the blend (transition)?")]
        public TransitionLookAtMode lookAtMode = TransitionLookAtMode.TimelineDefault;


        [Tooltip("Speed/Smoothness of the camera movement during and after the blend.")]
        public float cameraSmoothSpeed = 5.0f;

        public override void Execute(CameraRig rig)
        {
            if (rig == null || rig.Sequences == null || string.IsNullOrEmpty(nextChapterName))
                return;

            int chapterIdx = rig.Sequences.Chapters.FindIndex(c => c.name.Equals(nextChapterName, StringComparison.OrdinalIgnoreCase));
            if (chapterIdx < 0)
                return;

            CameraChapter nextChapter = rig.Sequences.Chapters[chapterIdx];

            if (lookAtMode == TransitionLookAtMode.ChapterStart || lookAtMode == TransitionLookAtMode.ChapterEnd)
            {
                if (Application.isPlaying && rig.Rails != null && nextChapter.startRailIndex < rig.Rails.EditorRails.Count)
                {
                    var spline = rig.Rails.EditorRails[nextChapter.startRailIndex];
                    if (spline != null)
                    {
                        double percent = (lookAtMode == TransitionLookAtMode.ChapterStart) ? 0.05 : 0.95;
                        var sample = spline.Evaluate(percent);
            
                        GameObject tempAnchor = new GameObject($"TempLookAt_{lookAtMode}_{nextChapterName}");
                        tempAnchor.transform.position = sample.position;
            
                        rig.Rails.SetLookAt(tempAnchor.transform, blendDuration);
                        Destroy(tempAnchor, blendDuration + 0.1f);
                    }
                }
            }
            else if (lookAtMode == TransitionLookAtMode.TimelineDefault)
            {
                rig.Rails.SetLookAt(null, blendDuration);
            }
            
            rig.Sequences.PlayChapter(chapterIdx, blendDuration, cameraSmoothSpeed);
        }
    }
}