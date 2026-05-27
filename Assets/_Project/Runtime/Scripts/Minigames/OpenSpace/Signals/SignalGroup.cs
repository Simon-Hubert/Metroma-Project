using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class SignalGroup : AOPSignal
    {
        [Space(7)]
        [SerializeField] private List<AOPSignal> _signals = new List<AOPSignal>();
        
        public override void ActiveSignal(OpenSpaceLogic opLogic, float alertDur, float reactionDur, float activeDur)
        {
            foreach (AOPSignal signal in _signals) {
                signal.ActiveSignal(opLogic, alertDur, reactionDur, activeDur);
            }
        }
    }
}
