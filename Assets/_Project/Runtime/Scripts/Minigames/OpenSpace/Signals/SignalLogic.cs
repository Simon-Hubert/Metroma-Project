using System;
using System.Collections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Metroma
{
    public class SignalLogic : AOPSignal
    {
        [Header("Sprites")]
        [SerializeField, ReadOnly] private SignalState _signalState;
        [Space(5)]
        [SerializeField] private GameObject _objectIdle;
        [SerializeField] private GameObject _objectAlert;
        [SerializeField] private GameObject _objectActive;

        [Header("Id")]
        private bool CanCallProjection => GetTrackId() >= 0;

        private void Start() {
            SwitchObject(SignalState.Idle);
        }

        public override void ActiveSignal(OpenSpaceLogic opLogic, float alertDur, float reactionDur, float activeDur) {
            StartCoroutine(ActiveSignalCoroutine(opLogic, alertDur, reactionDur, activeDur));
        }
        private IEnumerator ActiveSignalCoroutine(OpenSpaceLogic opLogic, float alertDur, float reactionDur, float activeDur) {
            if (alertDur > 0) {
                SwitchObject(SignalState.Alert);
                while (alertDur > 0) {
                    alertDur -= Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
            
            SwitchObject(SignalState.Active);
            while (activeDur > 0) {
                if (reactionDur > 0) {
                    reactionDur -= Time.fixedDeltaTime;
                    activeDur -= Time.fixedDeltaTime;
                }
                else {
                    activeDur -= Time.fixedDeltaTime;

                    if (CanCallProjection && opLogic.IsCtrlMoving()) {
                        if (opLogic.GetProjectedIfOnTrack) opLogic.CallProjection(GetTrackId());
                        else opLogic.CallProjection();
                    }
                }
                
                yield return new WaitForFixedUpdate();
            }
            
            SwitchObject(SignalState.Idle);
            opLogic.CallSignalEnded();
            yield break;
        }

        private void SwitchObject(SignalState state) {
            if (_objectIdle != null) _objectIdle.SetActive(false);
            if (_objectAlert != null) _objectAlert.SetActive(false);
            if (_objectActive != null) _objectActive.SetActive(false);

            switch (state)
            {
                case SignalState.Idle :
                    if (_objectIdle != null) _objectIdle.SetActive(true);
                    break;
                case SignalState.Alert :
                    if (_objectAlert != null) _objectAlert.SetActive(true);
                    break;
                case SignalState.Active :
                    if (_objectActive != null) _objectActive.SetActive(true);
                    break;
            }
        }
    }
}
