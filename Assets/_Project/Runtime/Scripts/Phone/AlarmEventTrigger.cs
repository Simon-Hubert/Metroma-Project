using UnityEngine;
using Metroma.Core;


namespace Metroma.Animations
{
    public class AlarmEventTrigger : MonoBehaviour
    {
        [Header("Configuration de l'alarme")]
        [SerializeField, Tooltip("Heure à laquelle l'alarme doit sonner.")]
        private int TargetHour = 10;
        
        [SerializeField, Tooltip("Minute à laquelle l'alarme doit sonner.")]
        private int TargetMinute = 10;
        
        public void StartPreconfiguredAlarm()
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.SetAlarm(TargetHour, TargetMinute);
            }
            else
            {
                Debug.LogError("[AlarmEventTrigger] Impossible de lancer l'alarme : Aucun AlarmManager n'est présent dans la scène !");
            }
        }
    }
}
