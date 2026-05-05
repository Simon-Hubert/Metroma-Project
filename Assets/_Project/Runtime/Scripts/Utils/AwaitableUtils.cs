using UnityEngine;
using System.Threading;

namespace Metroma.Utils {
    public static class AwaitableUtils {
        public static CancellationToken ResetToken(ref CancellationTokenSource tokenSource) {
            CancelToken(ref tokenSource);

            tokenSource = new CancellationTokenSource();
            return tokenSource.Token;
        }
        
        public static void CancelToken(ref CancellationTokenSource tokenSource) {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }
    }
}
