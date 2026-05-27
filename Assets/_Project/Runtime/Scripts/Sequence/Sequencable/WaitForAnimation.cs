using System;
using MoreMountains.Feedbacks;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class WaitForAnimation : ASequencable
    {
        [SerializeField] private Animation _animation;

        public override async Awaitable ExecuteAsync() {
            _animation.Play();
            bool finished = false;
            await Awaitable.EndOfFrameAsync();
            while (!finished) {
                finished = !_animation.isPlaying;
                await Awaitable.EndOfFrameAsync();
            }
        }
    }
}
