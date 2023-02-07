using System;
using UnityEngine;

public class GUISoundController : MonoBehaviour
{
    private string filename;
    private string type;

    public void Setup(string filename, int playTypeNum)
    {
        this.filename = filename;
        this.type = (playTypeNum == 0) ? "ambient" : "normal";
    }

    private void Start()
    {
        GameManager.Instance.PlayAudioPersistent(filename, type);
    }

    private void OnEnable()
    {
        if (filename != null)
        {
            GameManager.Instance.PlayAudioPersistent(filename, type);
        }
    }
}
