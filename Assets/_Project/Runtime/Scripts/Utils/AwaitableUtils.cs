using UnityEngine;
using System.Threading;

namespace Metroma.Utils {
    public static class AwaitableUtils {
        public static CancellationToken ResetToken(ref CancellationTokenSource tokenSource) {
            CancelToken(ref tokenSource);

            tokenSource = new CancellationTokenSource();
            return tokenSource.Token;
        }
        public static async Awaitable WaitWhilePausedAsync() {
            while (Core.PauseManager.IsPaused) {
                await Awaitable.NextFrameAsync();
            }
        }

        public static async Awaitable NextFrameAsync(CancellationToken cancellationToken = default) {
            await Awaitable.NextFrameAsync(cancellationToken);
            await WaitWhilePausedAsync();
        }

        public static void CancelToken(ref CancellationTokenSource tokenSource) {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }
    }
}
