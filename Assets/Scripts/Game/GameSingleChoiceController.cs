using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSingleChoiceController : MonoBehaviour
{
    private TMPro.TMP_Text choosenText;
    private TMPro.TMP_Text valueText;
    private RectTransform[] transforms;
    private RectTransform selfTransform;

    void Awake()
    {
        TMPro.TMP_Text[] texts = GetComponentsInChildren<TMPro.TMP_Text>();
        transforms = GetComponentsInChildren<RectTransform>();
        selfTransform = GetComponent<RectTransform>();

        if (texts.Length < 2)
        {
            Debug.LogWarning("Single choice GUI object is wrong! Recheck!");
        } else
        {
            choosenText = texts[0];
            valueText = texts[1];
        }
    }

    // NOTE: Use space so that the layout is not empty
    public void SetChoosen(bool value)
    {
        if (value)
        {
            choosenText.text = "-";
        } else
        {
            choosenText.text = " ";
        }
    }

    public void SetChoiceValue(string value)
    {
        choosenText.text = " ";
        valueText.text = value;

        choosenText.font = ResourceManager.Instance.GetFontAssetForLocalization();
        valueText.font = ResourceManager.Instance.GetFontAssetForLocalization();

        gameObject.SetActive(true);
    }

    public void UpdateLayout()
    {
        foreach (var transform in transforms)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(selfTransform);
    }

    public void Hide()
    {
        choosenText.text = " ";
        gameObject.SetActive(false);
    }
}
