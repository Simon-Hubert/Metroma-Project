    using System.Collections;
    using UnityEngine;

namespace Metroma
{
    public class SharkControllable : FreeRoamAControllable
    {
        [SerializeField] private float _dashDistance;
        [SerializeField] private float _dashFriction;

        [Header("Water FX")]
        [SerializeField] private WaterInteraction _waterInteraction;
        [SerializeField] private float _dashWakeBurst = 1.2f;
        [SerializeField] private float _dashWakeBurstDuration = 0.3f;

        private Vector2 _dashForce;
        private Coroutine dashRoutine;

        protected override void FixedUpdate() {
            base.FixedUpdate();
            rb2D.linearVelocity += _dashForce;
        }

        protected override void InputActionStart(bool action) {
            base.InputActionStart(action);
            Debug.Log("Dashed");
            if (_waterInteraction != null) _waterInteraction.Burst(_dashWakeBurst, _dashWakeBurstDuration);
            _dashForce += moveDirection * _dashDistance;
            Vector2.ClampMagnitude(_dashForce, _dashDistance);
            if (dashRoutine == null) {
                dashRoutine = StartCoroutine(DashCoroutine());
            }
        }
        

        IEnumerator DashCoroutine() {
            while (true) {
                yield return new WaitForFixedUpdate();
                _dashForce -= _dashFriction * _dashForce * Time.deltaTime;
            }
        }
    }
}
