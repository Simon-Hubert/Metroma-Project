using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    public class CamWiggleSetter : MonoBehaviour
    {
        [SerializeField] private CamWiggleAnchor _anchor;

        private void Awake() {
            _anchor.Target = GetComponent<MMWiggle>();
        }
    }
}
