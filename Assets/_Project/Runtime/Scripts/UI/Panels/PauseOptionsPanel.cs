using UnityEngine;


namespace Metroma.UI.Panels
{
    public class PauseOptionsPanel : OptionsPanel
    {
        public override void Initialize()
        {
            // IMPORTANT : On défini ce panel comme Popup.
            // Si on ne le fait pas, le UIManager va cacher le PauseMenuPanel,
            // ce qui déclenchera l'événement OnPauseMenuClosed et reprendra le jeu !
            bIsPopup = true;
            
            base.Initialize();
        }
    }
}
