using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;


namespace Metroma.Audio
{

    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [Header("Global UI Sounds")]
        [SerializeField, Tooltip("Déclenché quand la souris survole un bouton.")]
        private UnityEvent OnGlobalHover;
        
        [SerializeField, Tooltip("Déclenché quand on sélectionne un bouton (Navigation Manette).")]
        private UnityEvent OnGlobalSelect;
        
        [SerializeField, Tooltip("Déclenché quand on clique / valide un bouton.")]
        private UnityEvent OnGlobalClick;
        
        [SerializeField, Tooltip("Déclenché lors d'une action réussie / tâche terminée.")]
        private UnityEvent OnGlobalComplete;



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }


        [Button("Test Global Hover")]
        public void PlayGlobalHover()
        {
            OnGlobalHover?.Invoke();
        }


        [Button("Test Global Select")]
        public void PlayGlobalSelect()
        {
            OnGlobalSelect?.Invoke();
        }


        [Button("Test Global Click")]
        public void PlayGlobalClick()
        {
            OnGlobalClick?.Invoke();
        }


        [Button("Test Global Complete")]
        public void PlayGlobalComplete()
        {
            OnGlobalComplete?.Invoke();
        }
    }
}
