using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(MMF_Player))]
    public class PlayFeedBack : ASequencable
    {
        [SerializeField, HideInInspector] private MMF_Player _feedbacks;
        [SerializeField] private bool _waitForEnd;

        private void Reset() {
            _feedbacks = GetComponent<MMF_Player>();
        }

        public override async Awaitable ExecuteAsync() {
            if (!_feedbacks) {
                Debug.Log("FeedBacks not connected");
            }

            if (_waitForEnd) {
                await _feedbacks.PlayFeedbacksTask(transform.position);
            }
            else {
                _feedbacks.PlayFeedbacks();
            }
        }
    }
}