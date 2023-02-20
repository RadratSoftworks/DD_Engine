using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private void RegOrUnregActions(bool register)
    {
        var inputMap = GameInputManager.Instance.GameChoicesActionMap;

        InputAction choiceUp = inputMap.FindAction("Choice Up");
        InputAction choiceDown = inputMap.FindAction("Choice Down");
        InputAction choiceConfirmed = inputMap.FindAction("Choice Confirm");

        if (register)
        {
            choiceUp.performed += OnChoiceUp;
            choiceDown.performed += OnChoiceDown;
            choiceConfirmed.performed += OnChoiceConfirm;
        } else
        {
            choiceUp.performed -= OnChoiceUp;
            choiceDown.performed -= OnChoiceDown;
            choiceConfirmed.performed -= OnChoiceConfirm;
        }
    }

    private void SetInputState(bool enabled = true)
    {
        var inputMap = GameInputManager.Instance.GameChoicesActionMap;
        if (inputMap.enabled == enabled)
        {
            return;
        }

        if (enabled)
        {
            inputMap.Enable();
        } else
        {
            inputMap.Disable();
        }
    }

    private void Start()
    {
        GameInputManager.Instance.SetGUIInputActionMapState(false);

        SetInputState(true);
        RegOrUnregActions(true);
    }

    private void OnDestroy()
    {
        RegOrUnregActions(false);
    }

    private void OnEnable()
    {
        SetInputState(true);
    }

    private void OnDisable()
    {
        SetInputState(false);
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

    public void OnChoiceUp(InputAction.CallbackContext context)
    {
        SetActiveChoice((activeChoice == 0) ? (choicesToDialogueIds.Count - 1) : (activeChoice - 1));
    }

    public void OnChoiceDown(InputAction.CallbackContext context)
    {
        SetActiveChoice((activeChoice + 1) % choicesToDialogueIds.Count);
    }

    public void OnChoiceConfirm(InputAction.CallbackContext context)
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
