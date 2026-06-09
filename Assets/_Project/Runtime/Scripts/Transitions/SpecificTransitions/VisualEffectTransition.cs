using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;
using UnityEngine.VFX;

namespace Metroma
{
    public class VisualEffectTransition : ATransition
    {
        [SerializeField] private VisualEffect _vfx;
        [SerializeField] private bool _waitForVfxEnd;
        [Space(7)]
        [Tooltip("If Empty/Incorrect, will not wait until end of VFX")][SerializeField] private string _loopDurationParamName = "LoopDuration";
        [Tooltip("If Empty/Incorrect or Count is infinite, will not wait until end of VFX")][SerializeField] private string _loopCountParamName = "LoopCount";
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            try {
                _vfx.Play();

                if (CanWait()) {
                    float current = 0;
                    float target = _vfx.GetFloat(_loopDurationParamName) * _vfx.GetUInt(_loopCountParamName);

                    while (current < target) {
                        current += Time.deltaTime;
                        await Awaitable.NextFrameAsync(cancelToken);
                    }
                }
            }
            catch (OperationCanceledException) {

            }
            finally {
                
            }
        }

        private bool CanWait() {
            if (_waitForVfxEnd!) return false;

            if (!_vfx.HasFloat(_loopDurationParamName)) return false;
            if (!_vfx.HasUInt(_loopCountParamName) || _vfx.GetUInt(_loopCountParamName) <= 0) return false;

            return true;
        }
    }
}