using System.ComponentModel;
using Metroma.CameraTool;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine;

namespace Metroma.CameraTool.Timeline
{
    public class CameraToolMixerBehaviour : PlayableBehaviour
    {
        private CameraRig _boundRig;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is GameObject go)
            {
                _boundRig = go.GetComponent<CameraRig>();
            }
            else
            {
                _boundRig = playerData as CameraRig;
            }
            
            if (_boundRig == null)
                return;

            int inputCount = playable.GetInputCount();
            CameraPose blendedPose = CameraPose.Identity;
            float blendedLookAtWeight = 0f;
            float totalWeight = 0f;

#if UNITY_EDITOR
            float maxWeight = -1f;
            int dominantChapter = -1;
            int dominantRail = -1;
            float dominantLocalProgress = 0f;
            float dominantGlobalProgress = 0f;
#endif

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight <= 0.001f)
                    continue;

                ScriptPlayable<CameraToolBehaviour> inputPlayable = (ScriptPlayable<CameraToolBehaviour>)playable.GetInput(i);
                CameraToolBehaviour behaviour = inputPlayable.GetBehaviour();

                double officialTime = _boundRig.Sequences.Director ? _boundRig.Sequences.Director.time : playable.GetTime();
                double clipLocalTime = officialTime - behaviour.clipStartTime;

                double normalizedTime = 0;
                double activeDuration = (double)behaviour.clipDuration - behaviour.startPadding - behaviour.endPadding;

                if (clipLocalTime < (double)behaviour.startPadding)
                {
                    normalizedTime = 0;
                }
                else if (clipLocalTime > (double)behaviour.clipDuration - behaviour.endPadding)
                {
                    normalizedTime = 1;
                }
                else
                {
                    normalizedTime = activeDuration > 0.0001 ? (clipLocalTime - (double)behaviour.startPadding) / activeDuration : 1.0;
                }
                
                float easedTime = (behaviour.easingCurve != null) ? behaviour.easingCurve.Evaluate((float)normalizedTime) : (float)normalizedTime;
                float clipGlobalProgress = Mathf.Lerp(behaviour.startProgress, behaviour.endProgress, easedTime);

                CameraPose sample;
                if (behaviour.railIndex >= 0)
                {
                    sample = _boundRig.GetPoseOnRail(behaviour.railIndex, easedTime, behaviour.chapterIndex);
                }
                else
                {
                    sample = _boundRig.Rails.SampleRailLocal(0, clipGlobalProgress);
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
                if (inputWeight > maxWeight)
                {
                    maxWeight = inputWeight;
                    dominantChapter = behaviour.chapterIndex;
                    dominantRail = behaviour.railIndex;
                    dominantLocalProgress = easedTime; 
                    dominantGlobalProgress = clipGlobalProgress;
                }
#endif
            }

            if (totalWeight > 0.001f)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && dominantChapter >= 0)
                {
                    _boundRig.EditorReportVisualState(dominantChapter, dominantRail, dominantLocalProgress);

                    if (_boundRig.Rails != null)
                    {
                        _boundRig.Rails.GlobalProgress = dominantGlobalProgress;
                    }
                }
#endif
                float finalLookAt = blendedLookAtWeight / totalWeight;
                _boundRig.ApplyTimelineState(blendedPose, finalLookAt);
            }
        }
    }
}