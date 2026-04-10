using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(MMF_Player))]
    public class PlayFeedBack : Sequencable
    {
        [SerializeField, HideInInspector] private MMF_Player _feedbacks;

        private void Reset() {
            _feedbacks = GetComponent<MMF_Player>();
        }

        public override async Awaitable ExecuteAsync() {
            if (!_feedbacks) {
                Debug.Log("FeedBacks not connected");
            }
            _feedbacks.PlayFeedbacks();
            //await _feedbacks.PlayFeedbacksTask(transform.position);
        }
    }
}
