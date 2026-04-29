using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Metroma.FocusCam
{
    public class FocusCamMixer : PlayableBehaviour
    {
        private Camera _targetCamera;
        private Quaternion _defaultRotation;
        private Vector3 _defaultPosition;
        private float _defaultFOV;
        private bool _isDefaultValuesCaptured = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // A. Auto-Binding de la Caméra
            _targetCamera = (playerData as Camera) ?? Camera.main;
            if (_targetCamera == null) return;

            // B. Capture des valeurs initiales (UNE SEULE FOIS)
            if (!_isDefaultValuesCaptured)
            {
                _defaultRotation = _targetCamera.transform.rotation;
                _defaultPosition = _targetCamera.transform.position;
                _defaultFOV = _targetCamera.fieldOfView;
                _isDefaultValuesCaptured = true;
            }

            int inputCount = playable.GetInputCount();
            Quaternion blendedRotation = Quaternion.identity;
            Vector3 blendedPosition = Vector3.zero;
            float blendedFOV = 0f;
            
            float totalWeight = 0f;
            float totalPosWeight = 0f;
            float totalFOVWeight = 0f;

            // C. Calcul du blending des clips
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight <= 0.001f) continue;

                var inputPlayable = (ScriptPlayable<FocusCamBehaviour>)playable.GetInput(i);
                FocusCamBehaviour behaviour = inputPlayable.GetBehaviour();

                // C.1 Position (Placement)
                if (behaviour.overridePosition)
                {
                    blendedPosition += behaviour.cameraPosition * inputWeight;
                    totalPosWeight += inputWeight;
                }

                // C.2 Rotation (Orientation)
                Quaternion targetRot = Quaternion.identity;
                
                // On utilise la position actuelle de la caméra (ou celle déjà blendée) pour le LookAt
                Vector3 currentCamPos = _targetCamera.transform.position;

                if (behaviour.mode == FocusMode.LookAtPoint)
                {
                    Vector3 direction = (behaviour.position - currentCamPos).normalized;
                    targetRot = (direction != Vector3.zero) ? Quaternion.LookRotation(direction, Vector3.up) : _targetCamera.transform.rotation;
                }
                else if (behaviour.mode == FocusMode.KeepOrientation)
                {
                    targetRot = Quaternion.Euler(behaviour.rotation);
                }

                targetRot *= Quaternion.Euler(0, 0, behaviour.roll);

                if (totalWeight == 0f)
                    blendedRotation = targetRot;
                else
                    blendedRotation = Quaternion.Slerp(blendedRotation, targetRot, inputWeight / (totalWeight + inputWeight));

                // C.3 FOV (Zoom)
                if (behaviour.overrideFOV)
                {
                    blendedFOV += behaviour.fov * inputWeight;
                    totalFOVWeight += inputWeight;
                }

                totalWeight += inputWeight;
            }

            // D. Application finale
            if (totalWeight > 0.001f)
            {
                // 1. Application de la Position
                if (totalPosWeight > 0.001f)
                {
                    Vector3 finalPos = blendedPosition / totalPosWeight;
                    _targetCamera.transform.position = Vector3.Lerp(_defaultPosition, finalPos, totalWeight);
                }
                else
                {
                    _targetCamera.transform.position = Vector3.Lerp(_defaultPosition, _defaultPosition, totalWeight);
                }

                // 2. Application de la Rotation
                _targetCamera.transform.rotation = Quaternion.Slerp(_defaultRotation, blendedRotation, totalWeight);

                // 3. Application du FOV
                if (totalFOVWeight > 0.001f)
                {
                    float finalFOV = blendedFOV / totalFOVWeight;
                    _targetCamera.fieldOfView = Mathf.Lerp(_defaultFOV, finalFOV, totalWeight);
                }
                else
                {
                    _targetCamera.fieldOfView = Mathf.Lerp(_defaultFOV, _defaultFOV, totalWeight);
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(_targetCamera);
                    EditorUtility.SetDirty(_targetCamera.transform);
                }
#endif
            }
            else
            {
                // Si la piste est vide, on remet les valeurs par défaut
                if (_isDefaultValuesCaptured)
                {
                    _targetCamera.transform.position = _defaultPosition;
                    _targetCamera.transform.rotation = _defaultRotation;
                    _targetCamera.fieldOfView = _defaultFOV;
                }
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            _isDefaultValuesCaptured = false;
        }
    }

    [TrackColor(0.9f, 0.2f, 0.4f)]
    [TrackBindingType(typeof(Camera))]
    [TrackClipType(typeof(FocusCamClip))]
    public class FocusCamTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<FocusCamMixer>.Create(graph, inputCount);
        }
    }
}