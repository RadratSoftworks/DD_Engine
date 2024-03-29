using UnityEngine;
using DDEngine.Action;

namespace DDEngine.GUI
{
    public class GUIMenuController : MonoBehaviour
    {
        [SerializeField]
        private GUIMenuOptionsController menuOptionsController;

        private ActionLibrary actionLibrary;
        private GUIControlSet controlSet;
        private AudioClip selectedAudio;

        public GameObject Options => menuOptionsController.gameObject;

        // Start is called before the first frame update
        void Start()
        {
            menuOptionsController.OnButtonClicked += OnButtonClicked;
            selectedAudio = SoundManager.Instance.GetAudioClip(FilePaths.MenuOptionClickSFXFileName);
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Setup(GUIControlSet set, string filename)
        {
            controlSet = set;
            actionLibrary = ActionLibraryLoader.Load(filename);
        }

        public void OnButtonClicked(string name)
        {
            GameManager.Instance.PlayNormalAudioPersistent(selectedAudio);

            if (actionLibrary != null)
            {
                GameManager.Instance.RunPersistentCoroutine(actionLibrary.HandleAction(controlSet.ActionInterpreter, name, Constants.OnClickScriptEventName));
            }
        }
    }
}