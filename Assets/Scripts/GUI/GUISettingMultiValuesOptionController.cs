using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GUISettingMultiValuesOptionController : GUIMenuSelectableBehaviour
{
    [SerializeField]
    private SpriteAnimatorController focusIdleAnimation;

    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    [SerializeField]
    private TMPro.TMP_Text optionText;

    private RectTransform rectTransform;

    private List<Tuple<string, string>> valuesAndTextValuesId;
    private string settingName;

    private GUIControlSet ownSet;
    private int currentValueIndex = 0;

    private void SetupOptionDisplayText()
    {
        optionText.text = ownSet.GetLanguageString(valuesAndTextValuesId[currentValueIndex].Item2);
        optionText.font = ResourceManager.Instance.GetFontAssetForLocalization();
    }

    private void UpdateSettingAndText()
    {
        GameSettings.SetIngameSettingValue(settingName, valuesAndTextValuesId[currentValueIndex].Item1);
        SetupOptionDisplayText();
    }

    public void Setup(GUIControlSet ownSet, GUIControlSettingMultiValuesOptionDescription description, ref Vector2 positionBase)
    {
        Renderer textRenderer = optionText.GetComponent<Renderer>();
        textRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);

        this.ownSet = ownSet;
        this.settingName = description.SettingName;

        if (description.Id != null)
        {
            this.name = description.Id;
        }

        valuesAndTextValuesId = description.ValuesAndValueTextIds;

        rectTransform = GetComponent<RectTransform>();
        transform.localPosition = GameUtils.ToUnityCoordinates(description.Position) + positionBase;

        backgroundRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources, description.BackgroundImagePath);
        backgroundRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth + 2);

        focusIdleAnimation.Setup(Vector2.zero, description.AbsoluteDepth + 1, description.FocusIdleAnimationPath);
        focusIdleAnimation.Disable();

        positionBase += backgroundRenderer.sprite.bounds.size * Vector2.down;
        currentValueIndex = 0;

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = backgroundRenderer.sprite.bounds.size;
        }

        optionText.rectTransform.sizeDelta = backgroundRenderer.sprite.bounds.size;

        string optionValue = GameSettings.GetIngameSettingValue(settingName);
        currentValueIndex = valuesAndTextValuesId.FindIndex(value => value.Item1 == optionValue);

        if (currentValueIndex == -1)
        {
            Debug.LogError(string.Format("Invalid setting value: {0}. Default to value at index 0", optionValue));
            currentValueIndex = 0;
        }

        SetupOptionDisplayText();
    }

    public void OnLeftValueTriggered()
    {
        focusIdleAnimation.Disable();

        currentValueIndex = ((currentValueIndex - 1) < 0) ? (valuesAndTextValuesId.Count - 1) : currentValueIndex - 1;
        UpdateSettingAndText();
    }

    public override void OnOptionSelected()
    {
        base.OnOptionSelected();
        selectedSequence.AppendCallback(() => focusIdleAnimation.Enable());
    }

    public override void OnOptionDeselected()
    {
        base.OnOptionDeselected();
        focusIdleAnimation.Disable();
    }

    public void OnRightValueTriggered()
    {
        focusIdleAnimation.Disable();

        currentValueIndex = (currentValueIndex + 1) % valuesAndTextValuesId.Count;
        UpdateSettingAndText();
    }
}
