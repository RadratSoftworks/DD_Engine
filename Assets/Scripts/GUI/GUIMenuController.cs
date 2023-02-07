using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class GUIMenuController : MonoBehaviour
{
    private ActionLibrary actionLibrary;
    private GUIControlSet controlSet;

    // Start is called before the first frame update
    void Start()
    {
        GameObject menuOptions = transform.Find("MenuOptions")?.gameObject;
        GUIMenuOptionsController controller = menuOptions.GetComponent<GUIMenuOptionsController>();

        controller.OnButtonClicked += OnButtonClicked;
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
        if (actionLibrary != null)
        {
            StartCoroutine(actionLibrary.HandleAction(controlSet.ActionInterpreter, name, Constants.OnClickScriptEventName));
        }
    }
}
