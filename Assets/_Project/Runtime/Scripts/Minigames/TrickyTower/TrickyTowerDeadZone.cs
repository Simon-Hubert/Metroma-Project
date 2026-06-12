using System;
using UnityEngine;

namespace Metroma
{
    public class TrickyTowerDeadZone : MonoBehaviour
    {
        [SerializeField] private TrickyTowerLogic _gameLogic;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent<TrickyTowerFolders>(out var folder)) {
                if (!folder.GetIsOnTable) _gameLogic.RemoveLaunchedFolder();
                
                Destroy(other.gameObject);
                _gameLogic.AddDeadFolder();
            }
        }
    }
}
