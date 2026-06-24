using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;


namespace Metroma
{
    public class CS_AudioCustomEvent_Phone : MonoBehaviour
    {
        public AK.Wwise.Event SFX_Snooze_Button;

        public GameObject audioSource;


        [Button]
        public void SFX_Snooze_Button_Play()
        {
            SFX_Snooze_Button.Post(audioSource);
        }
    }
}
