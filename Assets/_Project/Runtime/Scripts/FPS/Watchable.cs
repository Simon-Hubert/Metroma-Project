using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;


namespace Metroma
{
    public class Watchable : MonoBehaviour
    {
        [Header("Params")] 
        [SerializeField] private float _timeToTrigger;
        [SerializeField] private bool _logs;
        [SerializeField] private UnityEvent OnStartWatch, OnWatched;
        
        private CancellationTokenSource _watchCancellationTokenSource;

        public void Watch()
        {
            CancelWatching();

            _watchCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            OnStartWatch?.Invoke();
            if(_logs) Debug.Log($"[Watchable] StartWatching on: {name}");
            _ = Watching();
        }


        private async Awaitable Watching()
        {
            CancellationTokenSource currentTokenSource = _watchCancellationTokenSource;

            try
            {
                await Awaitable.WaitForSecondsAsync(
                    _timeToTrigger,
                    currentTokenSource.Token
                );

                if (currentTokenSource.IsCancellationRequested)
                    return;

                if(_logs) Debug.Log($"[Watchable] Watched on: {name}");
                OnWatched?.Invoke();
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                if (_watchCancellationTokenSource == currentTokenSource)
                    _watchCancellationTokenSource = null;

                currentTokenSource.Dispose();
            }
        }

        public void CancelWatching()
        {
            if (_logs) Debug.Log($"[Watchable] CancelWatching on: {name}");
            if (_watchCancellationTokenSource == null)
                return;

            _watchCancellationTokenSource.Cancel();
            _watchCancellationTokenSource = null;
        }

        private void OnDisable()
        {
            CancelWatching();
        }

    }
}
