using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Metroma
{
    public class CS_AudioCustomEvent_Phone : MonoBehaviour
    {
        public AK.Wwise.Event SFX_Snooze_Button;

        public GameObject audioSource;

        public void SFX_Snooze_Button_Play()
        {
            SFX_Snooze_Button.Post(audioSource);
        }
    }
}
