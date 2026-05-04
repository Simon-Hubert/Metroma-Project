using System;
using System.Collections;
using UnityEngine;

namespace Metroma
{
    public class MaskSpawner : MonoBehaviour
    {
        [SerializeField] private SpriteMask mask;
        private Coroutine routine;


        private void OnEnable() {
            if (routine != null) {
                StopCoroutine(routine);
            }

            routine = null;
            routine = StartCoroutine(SpawnRoutine());
        }

        private void OnDisable() {
            if (routine != null) {
                StopCoroutine(routine);
                routine = null;
            }
        }

        IEnumerator SpawnRoutine() {
            while (true) {
                yield return new WaitForEndOfFrame();
                Instantiate(mask.gameObject, transform.position, Quaternion.identity);
            }
        }
    }
}
