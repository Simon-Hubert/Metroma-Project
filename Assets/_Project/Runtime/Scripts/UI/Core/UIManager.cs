using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Metroma.UI
{

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField, Tooltip("Le panel ouvert au démarrage du jeu.")]
        private UIPanel InitialPanel;

        private Stack<UIPanel> PanelStack = new Stack<UIPanel>();

        private Dictionary<System.Type, UIPanel> RegisteredPanels = new Dictionary<System.Type, UIPanel>();

        private bool bIsTransitioning = false;
        
        public event Action<Type> OnPanelOpened;
        public event Action<Type> OnPanelClosed;
        public event Action OnAllPanelsClosed;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            RegisterAllChildPanels();
        }
        

        private void Start()
        {
            if (InitialPanel != null)
            {
                OpenPanel(InitialPanel.GetType());
            }
        }

        private void LateUpdate()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (PanelStack.Count > 0)
                {
                    RestoreFocusToPanel(PanelStack.Peek());
                }
            }
        }


        private void RegisterAllChildPanels()
        {
            UIPanel[] Panels = FindObjectsOfType<UIPanel>(true);
            foreach (UIPanel Panel in Panels)
            {
                Panel.Initialize();
                Panel.gameObject.SetActive(false);
                
                if (!RegisteredPanels.ContainsKey(Panel.GetType()))
                {
                    RegisteredPanels.Add(Panel.GetType(), Panel);
                }
            }
        }


        public void OpenPanel<T>() where T : UIPanel
        {
            OpenPanel(typeof(T));
        }


        private void OpenPanel(System.Type PanelType)
        {
            if (bIsTransitioning)
                return;

            if (RegisteredPanels.TryGetValue(PanelType, out UIPanel PanelToOpen))
            {
                if (PanelStack.Count > 0)
                {
                    UIPanel CurrentTop = PanelStack.Peek();
                    
                    if (EventSystem.current != null)
                        CurrentTop.LastSelected = EventSystem.current.currentSelectedGameObject;
                    
                    CurrentTop.OnDefocus();
                    
                    if (!PanelToOpen.IsPopup)
                    {
                        CurrentTop.Hide();
                    }
                }

                PanelStack.Push(PanelToOpen);
                PanelToOpen.Show();
                PanelToOpen.OnFocus();
                
                SetFocusToPanel(PanelToOpen);
                
                OnPanelOpened?.Invoke(PanelType);
            }
            else
            {
                Debug.LogError($"[UIManager] Cannot find panel of type {PanelType.Name} ! Did you add it as a child of UIManager?");
            }
        }


        public void CloseCurrentPanel()
        {
            if (bIsTransitioning || PanelStack.Count == 0)
                return;

            UIPanel CurrentTop = PanelStack.Pop();
            CurrentTop.OnDefocus();
            CurrentTop.Hide();
            
            OnPanelClosed?.Invoke(CurrentTop.GetType());

            if (PanelStack.Count > 0)
            {
                UIPanel PreviousTop = PanelStack.Peek();
                
                if (!CurrentTop.IsPopup)
                {
                    PreviousTop.Show();
                }
                
                PreviousTop.OnFocus();
                
                RestoreFocusToPanel(PreviousTop);
            }
        }
        
        private void SetFocusToPanel(UIPanel Panel)
        {
            if (EventSystem.current == null || Panel.FirstSelected == null) return;
            EventSystem.current.SetSelectedGameObject(Panel.FirstSelected.gameObject);
        }

        private void RestoreFocusToPanel(UIPanel Panel)
        {
            if (EventSystem.current == null) return;

            if (Panel.LastSelected != null && Panel.LastSelected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(Panel.LastSelected);
            }
            else if (Panel.FirstSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(Panel.FirstSelected.gameObject);
            }
        }
        
        public void CloseAllPanels()
        {
            if (bIsTransitioning)
                return;

            while (PanelStack.Count > 0)
            {
                UIPanel CurrentTop = PanelStack.Pop();
                CurrentTop.OnDefocus();
                CurrentTop.Hide();
                
                OnPanelClosed?.Invoke(CurrentTop.GetType());
            }
            
            OnAllPanelsClosed?.Invoke();
        }


        public void SetTransitioningState(bool bIsTransitioningState)
        {
            bIsTransitioning = bIsTransitioningState;
        }
    }
}
