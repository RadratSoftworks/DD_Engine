using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class GUIMenuController : MonoBehaviour
{
    private ActionLibrary actionLibrary;

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

    // Literally just assume it as gadget for ease of mind
    public void LoadActionScript(string filename)
    {
        ResourceFile generalResources = ResourceManager.Instance.GeneralResources;
        if (!generalResources.Exists(filename))
        {
            throw new FileNotFoundException("Can't find action script file " + filename);
        }
        byte []data = generalResources.ReadResourceData(generalResources.Resources[filename]);

        using (MemoryStream stream = new MemoryStream(data))
        {
            actionLibrary = ActionParser.Parse(stream);
        }
    }

    public void OnButtonClicked(string name)
    {
        if (actionLibrary != null)
        {
            actionLibrary.HandleAction(name, Constants.OnClickScriptEventName);
        }
    }
}
