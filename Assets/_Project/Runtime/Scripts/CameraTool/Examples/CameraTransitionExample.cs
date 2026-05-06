using UnityEngine;
using Metroma.CameraTool;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple illustrant comment manipuler les transitions et les poses libres.
    /// Idéal pour les caméras de dialogue ou les focus temporaires sans Timeline.
    /// </summary>
    public class CameraTransitionExample : MonoBehaviour
    {
        [Header("Configuration")]
        public CameraRig rig;
        public Transform targetPoint;
        public float duration = 2.0f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary> Effectue un fondu fluide vers une pose statique </summary>
        [Button("🛤️ Transition vers Cible")]
        public void TransitionToTarget()
        {
            if (rig == null || targetPoint == null) return;

            CameraPose targetPose = new CameraPose
            {
                position = targetPoint.position,
                rotation = targetPoint.rotation,
                fov = 60f,
                up = Vector3.up
            };

            // On lance la transition. Le Rig s'occupera du mélange fluide.
            rig.Transitions.StartTransition(targetPose, duration, curve);
        }

        /// <summary> Téléporte instantanément la caméra </summary>
        [Button("🛤️ Snap vers Cible")]
        public void SnapToTarget()
        {
            if (rig == null || targetPoint == null) return;

            CameraPose targetPose = new CameraPose
            {
                position = targetPoint.position,
                rotation = targetPoint.rotation,
                fov = 60f,
                up = Vector3.up
            };

            rig.Transitions.SnapToPose(targetPose);
        }

        /// <summary> Redonne le contrôle au Rig ou au Gameplay </summary>
        [Button("🛤️ Retour au contrôle standard")]
        public void ReturnControl()
        {
            if (rig == null) return;
            
            // Cette méthode effectue un fondu entre la pose statique et la position logique du Rig.
            rig.Transitions.ReturnToRigControl(duration);
        }
    }
}
