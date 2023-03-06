using UnityEngine;

namespace DDEngine.Installer
{
    public class AboutButtonController : MonoBehaviour
    {
        [SerializeField]
        private AboutPopupController popupController;

        public void OnClick()
        {
            popupController.Show();
        }
    }
}
