using UnityEngine;

namespace DDEngine.GUI
{
    public class GUILocationSetup : MonoBehaviour
    {
        public GameObject activeCollideGameObject;

        private BoxCollider2D activeCollider;

        // Start is called before the first frame update
        void Start()
        {
            activeCollider = GetComponent<BoxCollider2D>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}