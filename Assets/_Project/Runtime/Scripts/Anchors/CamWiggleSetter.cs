using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    public class CamWiggleSetter : MonoBehaviour
    {
        [SerializeField] private MMWiggle _wiggle;
        [SerializeField] private CamWiggleAnchor _anchor;

        private void Awake() {
            _anchor.Target = _wiggle;
        }
    }
}
