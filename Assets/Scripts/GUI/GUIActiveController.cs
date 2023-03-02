using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI
{
    public class GUIActiveController : MonoBehaviour
    {
        [SerializeField]
        private GameObject arrows;

        [SerializeField]
        private GameObject focusPoint;

        [Tooltip("The minimum amount of units an arrow can translate from the starting position")]
        [SerializeField]
        private Vector2 arrowMinimumOutpointDist = new Vector2(21, 21);

        private GUIControlSet controlSet;
        private GUILayerController layerController;

        // The pivot of the collider should be the main view point we got to center to!
        private Vector3 actionCenterViewPoint;

        public bool PanToCenterWhenSelect { get; set; } = true;
        private bool dialogueChangeSubscribed = false;

        private void Awake()
        {
            layerController = GetComponentInParent<GUILayerController>();
            layerController.Location.ActiveConfirmed += OnConfirmed;
        }

        private void Start()
        {
        }

        public void Setup(GUIControlSet set, Vector2 position, Vector2 size, Rect detectBounds, bool panToWhenSelected = true)
        {
            this.PanToCenterWhenSelect = panToWhenSelected;
            controlSet = set;

            // Indent and reduce the size a bit, the reason is physics is working with small unit
            // So active keep encountering each other, not even trigger exit when the collider is stuck between
            transform.localPosition = GameUtils.ToUnityCoordinates(detectBounds.position + detectBounds.size / 2 + new Vector2(1, 1));

            // The size of the arrows point is accounted with the maximum outpoint state (point outwards) of the arrows.
            Vector3 sizeTransformed = (size - arrowMinimumOutpointDist) / 2;

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = GameUtils.ToUnitySize(new Vector2(Mathf.Max(detectBounds.size.x - 2, 1.0f), Mathf.Max(detectBounds.size.y - 2, 1.0f)));
            }

            Vector3[] positionMove =
            {
                Vector3.Scale(sizeTransformed, new Vector3(-1, -1)),
                Vector3.Scale(sizeTransformed, new Vector3(1, -1)),
                Vector3.Scale(sizeTransformed, new Vector3(-1, 1)),
                Vector3.Scale(sizeTransformed, new Vector3(1, 1))
            };

            Vector2[] origin =
            {
                Vector2.right,
                Vector2.zero,
                Vector3.one,
                Vector2.up
            };

            arrows.transform.localPosition = GameUtils.ToUnityCoordinates(position + (size + arrowMinimumOutpointDist) / 2) - new Vector2(transform.localPosition.x, transform.localPosition.y);
            focusPoint.transform.localPosition = GameUtils.ToUnityCoordinates(position + size / 2) - new Vector2(transform.localPosition.x, transform.localPosition.y);

            for (int i = 0; i < 4; i++)
            {
                GameObject child = arrows.transform.GetChild(i).gameObject;
                SpriteAnimatorController controller = child.GetComponent<SpriteAnimatorController>();

                controller.Setup(positionMove[i], 0, FilePaths.ArrowAnimationsPath[i], null, origin[i]);
            }

            arrows.SetActive(false);
            dialogueChangeSubscribed = false;

            controlSet.StateChanged += enabled =>
            {
                if (dialogueChangeSubscribed == enabled)
                {
                    return;
                }

                if (enabled)
                {
                    GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
                }
                else
                {
                    GameManager.Instance.DialogueStateChanged -= OnDialogueStateChanged;
                }

                dialogueChangeSubscribed = enabled;
            };
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            actionCenterViewPoint = collision.transform.position;
            arrows.SetActive(true);
            GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnFocusScriptEventName));
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            arrows.SetActive(false);
        }

        public void OnConfirmed()
        {
            if (!arrows.activeSelf)
            {
                return;
            }

            if (PanToCenterWhenSelect)
            {
                layerController.AccurateScrollLocation(focusPoint.transform.InverseTransformPoint(actionCenterViewPoint));
            }

            GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnClickScriptEventName));
        }

        private void OnDialogueStateChanged(bool enabled)
        {
            // Disable all interactable actives when dialogue is on!
            gameObject.SetActive(!enabled);
        }
    }
}
