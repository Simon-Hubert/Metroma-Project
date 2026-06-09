using UnityEngine;
using Metroma.Utils;


namespace Metroma.UI.Tests
{
    public class TestPauseAwaitable : MonoBehaviour
    {

        private async void Start()
        {
            Debug.Log("🟢 Début du test des Awaitables !");
            
            // On lance deux tests différents en parallèle
            _ = BoucleClassique();
            _ = BouclePersonnalisee();
        }


        // --- TEST 1 : Awaitable classique ---
        private async Awaitable BoucleClassique()
        {
            int compteur = 0;
            while (true)
            {
                await Awaitable.WaitForSecondsAsync(1f);
                compteur++;
                Debug.Log($"⏳ [Test 1 - WaitForSecondsAsync] Tic tac n°{compteur}");
            }
        }


        // --- TEST 2 : Awaitable Personnalisé (NextFrameAsync) ---
        private async Awaitable BouclePersonnalisee()
        {
            int frameCount = 0;
            while (true)
            {
                await AwaitableUtils.WaitWhilePausedAsync();
                await Awaitable.NextFrameAsync();
                
                frameCount++;
                if (frameCount % 100 == 0)
                {
                    Debug.Log($"⚙️ [Test 2 - NextFrameAsync] J'ai tourné 100 frames de plus !");
                }
            }
        }
    }
}
