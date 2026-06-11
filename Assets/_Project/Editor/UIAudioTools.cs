#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Metroma.Audio;


namespace Metroma.EditorTools
{
    public class UIAudioTools
    {
        [MenuItem("Tools/Metroma/Ajouter Audio sur tous les Boutons (Scène)")]
        public static void AutoAddAudioToAllButtons()
        {
            Button[] allButtons = Object.FindObjectsOfType<Button>(true);
            int addedCount = 0;

            foreach (Button btn in allButtons)
            {
                if (btn.GetComponent<UIButtonAudio>() == null)
                {
                    Undo.AddComponent<UIButtonAudio>(btn.gameObject);
                    addedCount++;
                }
            }

            Debug.Log($"[Metroma Tools] Opération terminée. Le script UIButtonAudio a été ajouté à {addedCount} boutons !");
        }
    }
}
#endif
