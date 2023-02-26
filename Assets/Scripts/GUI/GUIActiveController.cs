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

        [Tooltip("The maximum amount of units an arrow can translate from the starting position")]
        [SerializeField]
        private Vector2 arrowMaximumOutpointDist = new Vector2(21, 21);

        private GUIControlSet controlSet;
        private GUILayerController layerController;

        // The pivot of the collider should be the main view point we got to center to!
        private Vector3 actionCenterViewPoint;

        public bool PanToCenterWhenSelect { get; set; } = true;
        private bool dialogueChangeSubscribed = false;

        private void Awake()
        {
            layerController = GetComponentInParent<GUILayerController>();
        }

        private void Start()
        {
            arrows.SetActive(false);

            GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
            dialogueChangeSubscribed = true;

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

            OnDialogueStateChanged(GameManager.Instance.GadgetActive);
        }

        public void Setup(GUIControlSet set, Vector2 position, Vector2 size, Rect detectBounds, bool panToWhenSelected = true)
        {
            this.PanToCenterWhenSelect = panToWhenSelected;
            controlSet = set;

            transform.localPosition = GameUtils.ToUnityCoordinates(detectBounds.position + detectBounds.size / 2);

            // The size of the arrows point is accounted with the maximum outpoint state (point outwards) of the arrows.
            Vector3 sizeTransformed = (size - arrowMaximumOutpointDist) / 2;

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = GameUtils.ToUnitySize(detectBounds.size);
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

            arrows.transform.localPosition = GameUtils.ToUnityCoordinates(position + (size + arrowMaximumOutpointDist) / 2) - new Vector2(transform.localPosition.x, transform.localPosition.y);
            focusPoint.transform.localPosition = GameUtils.ToUnityCoordinates(position + size / 2) - new Vector2(transform.localPosition.x, transform.localPosition.y);

            for (int i = 0; i < 4; i++)
            {
                GameObject child = arrows.transform.GetChild(i).gameObject;
                SpriteAnimatorController controller = child.GetComponent<SpriteAnimatorController>();

                controller.Setup(positionMove[i], 0, FilePaths.ArrowAnimationsPath[i], null, origin[i]);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            actionCenterViewPoint = collision.transform.position;
            layerController.Location.ClaimActive(this);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (layerController.Location.ReleaseActive(this))
            {
                arrows.SetActive(false);
            }
        }

        public void OnConfirmed()
        {
            if (PanToCenterWhenSelect)
            {
                layerController.ScrollLocation(focusPoint.transform.InverseTransformPoint(actionCenterViewPoint));
            }

            GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnClickScriptEventName));
        }

        public void OnClaimSuccess()
        {
            arrows.SetActive(true);
            GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnFocusScriptEventName));
        }

        private void OnDialogueStateChanged(bool enabled)
        {
            // Disable all interactable actives when dialogue is on!
            gameObject.SetActive(!enabled);
        }
    }
}
