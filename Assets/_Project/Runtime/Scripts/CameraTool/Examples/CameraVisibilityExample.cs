using UnityEngine;
using Metroma.CameraTool.Modules;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple illustrant l'utilisation du système de détection de visibilité.
    /// Idéal pour les énigmes, l'investigation ou les dialogues qui se déclenchent 
    /// quand le joueur regarde quelque chose.
    /// </summary>
    public class CameraVisibilityExample : MonoBehaviour
    {
        public SequenceModule sequenceModule;
        public string targetEventId = "object_seen";

        private void OnEnable()
        {
            if (sequenceModule != null)
                sequenceModule.onVisibilityEvent.AddListener(OnObjectSeen);
        }

        private void OnDisable()
        {
            if (sequenceModule != null)
                sequenceModule.onVisibilityEvent.RemoveListener(OnObjectSeen);
        }

        /// <summary> Déclenché quand un marker de visibilité sur la Timeline a validé les conditions </summary>
        private void OnObjectSeen(string eventId)
        {
            Debug.Log($"[Visibilité] Événement reçu : {eventId}");

            if (eventId == targetEventId)
            {
                Debug.Log($"Le joueur a fixé {targetEventId} ! Action déclenchée.");
            }
        }
    }
}
