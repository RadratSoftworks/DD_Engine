using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;

        ResourceManager.Instance.OnResourcesReady += OnResourcesReady;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnResourcesReady()
    {
        GUIManager.Instance.LoadControlSet(FilePaths.MainChapterGUIControlFileName);
    }
}
