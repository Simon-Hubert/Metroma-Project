using System;
using UnityEngine;

namespace Metroma
{
    public class CutToBlackSequence : ASequencable
    {
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            BlackScreen.Show();
            await Awaitable.WaitForSecondsAsync(_duration);
            BlackScreen.Hide();
            
        }

        private void OnValidate() {
            name = $"Cut to black for {_duration} sec";
        }
    }
}
