using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class GameChoicesController : MonoBehaviour
{
    public GameObject backgroundObject;
    public GameObject highlightObject;
    public GameObject choicesObject;
    public GameObject singleChoicePrefab;
    public Color highlightColor = Color.yellow;

    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer highlightRenderer;
    private RectTransform choicesTransform;

    private List<Tuple<Dialogue, DialogueSlide>> choicesToDialogueIds = new List<Tuple<Dialogue, DialogueSlide>>();
    private List<GameSingleChoiceController> singleChoiceControllers = new List<GameSingleChoiceController>();
    private List<RectTransform> singleChoiceTransforms = new List<RectTransform>();

    private int activeChoice = -1;

    public delegate void OnChoiceConfirmed(Dialogue dialogue, DialogueSlide dialogueSlide);
    public event OnChoiceConfirmed ChoiceConfirmed;

    void Awake()
    {
        backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
        choicesTransform = choicesObject.GetComponent<RectTransform>();
    }

    public void Close()
    {
        foreach (GameSingleChoiceController choiceController in singleChoiceControllers)
        {
            choiceController.Hide();
        }

        choicesToDialogueIds.Clear();
        gameObject.SetActive(false);
    }

    private void UpdateHighlight()
    {
        // Set highlight
        RectTransform childTransform = singleChoiceTransforms[activeChoice];

        highlightObject.transform.localPosition = childTransform.localPosition;
        highlightRenderer.size = childTransform.sizeDelta;

        highlightRenderer.color = highlightColor;
    }

    public void UpdateTextAndRenderLayout()
    {
        foreach (var controller in singleChoiceControllers)
        {
            controller.UpdateLayout();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesTransform);
        backgroundRenderer.size = choicesTransform.sizeDelta;

        UpdateHighlight();
    }

    private void SetActiveChoice(int choice)
    {
        if (choice >= singleChoiceControllers.Count)
        {
            throw new ArgumentOutOfRangeException("Index of hoice to select is out of range");
        }

        if (activeChoice >= 0)
        {
            GameSingleChoiceController previousController = singleChoiceControllers[activeChoice];
            previousController.SetChoosen(false);
        }

        GameSingleChoiceController controller = singleChoiceControllers[choice];
        controller.SetChoosen(true);

        activeChoice = choice;

        UpdateHighlight();
    }

    private void OnChoiceUp()
    {
        SetActiveChoice((activeChoice == 0) ? (choicesToDialogueIds.Count - 1) : (activeChoice - 1));
    }

    private void OnChoiceDown()
    {
        SetActiveChoice((activeChoice + 1) % choicesToDialogueIds.Count);
    }

    private void OnChoiceConfirm()
    {
        (Dialogue dialogue, DialogueSlide dialogueSlide) = choicesToDialogueIds[activeChoice];
        ChoiceConfirmed?.Invoke(dialogue, dialogueSlide);
    }

    public void AddChoice(string choice, Dialogue dialogue, int dialogueIdIfChoose)
    {
        DialogueSlide slide = dialogue.GetDialogueSlideWithId(dialogueIdIfChoose);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (choicesToDialogueIds.Count == 0)
        {
            activeChoice = 0;
        }

        if (choicesToDialogueIds.Count < singleChoiceControllers.Count)
        {
            GameSingleChoiceController grabController = singleChoiceControllers[choicesToDialogueIds.Count];
            grabController.SetChoiceValue(choice);

            if (choicesToDialogueIds.Count == 0)
            {
                grabController.SetChoosen(true);
            }
        } else
        {
            GameObject obj = Instantiate(singleChoicePrefab, choicesObject.transform, false);
            GameSingleChoiceController controller = obj.GetComponent<GameSingleChoiceController>(); ;

            if (controller == null)
            {
                Debug.LogError("Failed to get single choice controller");
                return;
            }

            singleChoiceControllers.Add(controller);
            singleChoiceTransforms.Add(obj.GetComponent<RectTransform>());

            controller.SetChoiceValue(choice);

            if (choicesToDialogueIds.Count == 0)
            {
                controller.SetChoosen(true);
            }
        }

        choicesToDialogueIds.Add(new Tuple<Dialogue, DialogueSlide>(dialogue, slide));
    }

    public void Setup(Vector2 canvasSize)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(canvasSize * Vector2.up);
    }
}
