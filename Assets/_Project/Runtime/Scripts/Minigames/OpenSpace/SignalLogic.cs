using System;
using System.Collections;
using UnityEngine;

namespace Metroma
{
    public enum SignalState
    {
        Idle = 0,
        Alert = 1,
        Active = 2
    }
    
    public class SignalLogic : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _objectIdle;
        [SerializeField] private Sprite _objectAlert;
        [SerializeField] private Sprite _objectActive;

        [Header("Id")]
        [SerializeField, Min(0)] private int _trackId;
        public int GetTrackId { get => _trackId; }
        [SerializeField, Min(0)] private int _segmentId;
        public int GetSegmentId { get => _segmentId; }

        private void Start()
        {
            
        }

        public IEnumerator ActiveSignal(OpenSpaceLogic opLogic, float alertDur, float reactionDur, float activeDur) {
            //_spriteRender.sprite = _spriteAlert;
            while (alertDur > 0) {
                alertDur -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            
            //_spriteRender.sprite = _spriteActive;
            while (activeDur > 0) {
                if (reactionDur > 0) {
                    reactionDur -= Time.fixedDeltaTime;
                    activeDur -= Time.fixedDeltaTime;
                }
                else {
                    activeDur -= Time.fixedDeltaTime;

                    if (opLogic.IsCtrlMoving()) {
                        if (opLogic.GetProjectedIfOnTrack) opLogic.CallProjection(GetTrackId);
                        else opLogic.CallProjection();
                    }
                }
                
                yield return new WaitForFixedUpdate();
            }
            
            //_spriteRender.sprite = _spriteIdle;
            opLogic.CallSignalEnded();
            yield break;
        }

        private void SwitchObject(SignalState state) {
            
        }
    }
}
