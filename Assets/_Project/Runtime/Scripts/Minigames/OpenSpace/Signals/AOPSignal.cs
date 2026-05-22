using System;
using UnityEngine;
using System.Collections;

namespace Metroma
{
    [Serializable]
    public abstract class AOPSignal : MonoBehaviour
    {
        [Header("Signal Info")]
        [SerializeField] private bool _isAlwaysCalled = false;
        public bool IsAlwaysCalled() => _isAlwaysCalled;
        
        [SerializeField] private int _trackId = 0;
        public int GetTrackId() => _trackId;
        
        [SerializeField] private int _segmentId = 0;
        public int GetSegmentId() => _segmentId;
        
        public abstract void ActiveSignal(OpenSpaceLogic opLogic, float alertDur, float reactionDur, float activeDur);
    }
}
