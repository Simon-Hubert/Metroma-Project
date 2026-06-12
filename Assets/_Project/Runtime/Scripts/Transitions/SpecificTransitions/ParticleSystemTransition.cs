using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;
using UnityEngine.VFX;

namespace Metroma
{
    public class ParticleSystemTransition : ATransition
    {
        [SerializeField] private ParticleSystem _vfx;
        [SerializeField] private bool _active = true;
        
        [SerializeField] private bool _waitForVfxEnd;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            try {
                if (_active) {
                    _vfx.Play();

                    if (!_vfx.main.loop) {
                        float current = 0;
                        float target = _vfx.main.duration;

                        while (current < target) {
                            current += Time.deltaTime;
                            await Awaitable.NextFrameAsync(cancelToken);
                        }
                    }
                }
                else _vfx.Stop();
            }
            catch (OperationCanceledException) {

            }
            finally {
                
            }
        }
    }
}