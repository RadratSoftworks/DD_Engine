using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI
{
    public class GUILabelController : MonoBehaviour
    {
        private TMPro.TMP_Text labelText;
        private MeshRenderer labelTextRenderer;
        private string textId;

        private void UpdateText(GUIControlSet ownSet)
        {
            labelText = GetComponent<TMPro.TMP_Text>();
            labelText.font = ResourceManager.Instance.GetFontAssetForLocalization();
            labelText.text = ownSet.GetLanguageString(textId);
        }

        public void Setup(GUIControlSet ownSet, Vector2 position, string text, int depth)
        {
            this.textId = text;

            transform.localPosition = GameUtils.ToUnityCoordinates(position);

            labelTextRenderer = GetComponentInChildren<MeshRenderer>();
            labelTextRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(depth);

            UpdateText(ownSet);
            ownSet.LocalizationChanged += UpdateText;
        }
    }
}