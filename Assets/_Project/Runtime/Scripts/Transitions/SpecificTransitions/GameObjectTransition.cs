using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class GameObjectTransition : ATransition
    {
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private bool _objectState;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            try {
                _gameObject?.SetActive(_objectState);
            }
            catch (OperationCanceledException) {

            }
        }
    }
}