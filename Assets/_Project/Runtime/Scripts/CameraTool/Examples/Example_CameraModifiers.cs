using UnityEngine;
using Metroma.CameraTool;
using Metroma.CameraTool.Modifiers;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple illustrant tous les effets visuels de type "Modifiers" applicables 
    /// sur la caméra (Shake, Haptics, Chromatic Aberration, HitStop, Dolly Zoom...).
    /// </summary>
    public class Example_CameraModifiers : MonoBehaviour
    {
        [SerializeField] private Transform lookAtTarget;

        private Camera GetCamera()
        {
            return CameraRig.Active != null ? CameraRig.Active.TargetCamera : null;
        }

        [Button("⚡ Shake & Haptic (Secousse + Manette)")]
        public void ApplyShake()
        {
            var cam = GetCamera();
            if (cam == null) return;
            
            // Secousse visuelle de la caméra
            cam.DoShake(intensity: 2f, duration: 0.5f, roughness: 10f);
            
            // Vibration manette (fallback géré automatiquement vers la manette active)
            cam.DoHaptic(AnimationCurve.Linear(0, 1, 1, 0), lowFreq: 0.5f, highFreq: 0.8f, duration: 0.5f);
        }

        [Button("⚡ Dolly Zoom (Effet Vertigo)")]
        public void ApplyDollyZoom()
        {
            var cam = GetCamera();
            if (cam == null) return;
            
            // Recule la position de 5 unités (-5) tout en réduisant le FOV à 30 pour conserver le cadrage
            cam.DoDollyZoom(pushDistance: -5f, targetFOV: 30f, duration: 2f);
        }

        [Button("⚡ Regarder Cible (LookAt Temporaire)")]
        public void ApplyTemporaryLookAt()
        {
            var cam = GetCamera();
            if (cam == null || lookAtTarget == null) return;
            
            // Tourne doucement la caméra vers cet objet pendant 3 secondes, tout en gardant sa position actuelle
            cam.SetTemporaryLookAt(lookAtTarget, duration: 3f);
        }

        [Button("⚡ Hit Stop & Chromatic Aberration (Impact)")]
        public void ApplyImpactVFX()
        {
            var cam = GetCamera();
            if (cam == null) return;
            
            // Gèle légèrement le temps (ralenti global du jeu)
            cam.DoHitStop(duration: 0.15f, timeScale: 0.05f);
            
            // Ajoute un effet visuel de choc d'aberration chromatique
            cam.DoChromaticAberration(intensity: 1f, duration: 0.5f);
        }

        [Button("⚡ Stopper Tous Les Modificateurs")]
        public void StopAllModifiers()
        {
            var cam = GetCamera();
            if (cam == null) return;
            
            // Nettoie tous les shakes, flashs et offsets actifs (parfait en cas de cinématique brutale)
            cam.StopAllCameraModifiers();
        }
    }
}
