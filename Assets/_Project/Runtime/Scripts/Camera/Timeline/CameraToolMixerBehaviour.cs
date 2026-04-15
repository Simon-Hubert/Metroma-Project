using UnityEngine.Playables;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Mixer that blends overlapping <see cref="CameraToolClip"/> clips
    /// and writes the final spline progress + lookAt weight to the bound <see cref="CameraTool"/>.
    /// Zero-allocation per frame.
    /// </summary>
    public class CameraToolMixerBehaviour : PlayableBehaviour
    {
        private CameraTool _boundCameraTool;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _boundCameraTool = playerData as CameraTool;
            if (_boundCameraTool == null)
                return;

            int inputCount = playable.GetInputCount();
            CameraPose blendedPose = CameraPose.Identity;
            float blendedLookAtWeight = 0f;
            float totalWeight = 0f;

#if UNITY_EDITOR
            float maxWeight = -1f;
            int dominantChapter = -1;
            float dominantProgress = 0f;
#endif

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight <= 0.001f)
                {
                    continue;
                }

                ScriptPlayable<CameraToolBehaviour> inputPlayable = (ScriptPlayable<CameraToolBehaviour>)playable.GetInput(i);
                CameraToolBehaviour behaviour = inputPlayable.GetBehaviour();

                float normalizedTime = (float)(inputPlayable.GetTime() / inputPlayable.GetDuration());
                float easedTime = behaviour.easingCurve.Evaluate(normalizedTime);

                CameraPose sample;
                if (behaviour.railIndex >= 0)
                {
                    // 1. RAIL MODE: Evaluate the full 0-1 range of the targeted rail, respecting chapter easing
                    sample = _boundCameraTool.GetPoseOnRail(behaviour.railIndex, easedTime, behaviour.chapterIndex);
                }
                else
                {
                    // Legacy/Chapter Evaluation (mapped 0-1 within the chapter's range)
                    float clipProgress = behaviour.startProgress + (behaviour.endProgress - behaviour.startProgress) * easedTime;
                    sample = _boundCameraTool.GetPoseOnChapter(behaviour.chapterIndex, clipProgress);
                }

                if (totalWeight <= 0f)
                {
                    blendedPose = sample;
                }
                else
                {
                    float blendAlpha = inputWeight / (totalWeight + inputWeight);
                    blendedPose = CameraPose.Lerp(blendedPose, sample, blendAlpha);
                }

                blendedLookAtWeight += behaviour.lookAtWeight * inputWeight;
                totalWeight += inputWeight;

#if UNITY_EDITOR
                // In Editor, track the dominant clip to sync visual debugs
                if (inputWeight > maxWeight)
                {
                    maxWeight = inputWeight;
                    dominantChapter = behaviour.chapterIndex;
                    
                    if (behaviour.railIndex >= 0)
                    {
                        // Pass 'easedTime' (0-1 of the rail) to get the Chapter's global 0-1
                        dominantProgress = _boundCameraTool.GetGlobalProgressFromRail(behaviour.railIndex, easedTime, behaviour.chapterIndex);
                    }
                    else
                    {
                        // It's a chapter clip, calculate mapped progress
                        dominantProgress = behaviour.startProgress + (behaviour.endProgress - behaviour.startProgress) * easedTime;
                    }
                }
#endif
            }

            if (totalWeight > 0.001f)
            {
#if UNITY_EDITOR
                // Report visual state for gizmo/dot rendering
                if (!UnityEngine.Application.isPlaying && dominantChapter >= 0)
                {
                    _boundCameraTool.EditorReportVisualState(dominantChapter, dominantProgress);
                }
#endif
                float finalLookAt = blendedLookAtWeight / totalWeight;
                _boundCameraTool.ApplyTimelinePose(blendedPose, finalLookAt);
            }
        }
    }
}
