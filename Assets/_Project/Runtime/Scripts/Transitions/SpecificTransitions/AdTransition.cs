using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class AdTransition : ATransition
    {
        [SerializeField] private MiniGameTransitionData _data;
        private static readonly int ZTest = Shader.PropertyToID("_ZTest");
        private static readonly int Transition = Shader.PropertyToID("_Transition");
        
        void ActivateZTest(Material mat) {
            mat.SetInt(ZTest, 8);
        }
        
        void DeactivateZTest(Material mat) {
            mat.SetInt(ZTest, 4);
        }
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                if (_data.IsIn) {
                    ActivateZTest(_data.Material.Material);
                    float t = 0f;
                    while (t < _data.Duration) {
                        float p = t / _data.Duration;
                        _data.Material.Material.SetFloat(Transition, 1 - p);
                        t += Time.deltaTime;
                        await Awaitable.NextFrameAsync(cancelToken);
                    }

                    _data.Material.Material.SetFloat(Transition, 0);
                }
                else {
                    float t = 0f;
                    while (t < _data.Duration) {
                        float p = t / _data.Duration;
                        _data.Material.Material.SetFloat(Transition, p);
                        t += Time.deltaTime;
                        await Awaitable.NextFrameAsync(cancelToken);
                    }

                    _data.Material.Material.SetFloat(Transition, 1);
                    DeactivateZTest(_data.Material.Material);
                }
            }
            catch (OperationCanceledException oce) {

            }
            finally {
                
            }
        }
    }
}
