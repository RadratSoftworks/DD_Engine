#if UNITY_EDITOR
using UnityEngine.InputSystem.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

//!!>> This script should NOT be placed in an "Editor" folder. Ideally placed in a "Plugins" folder.
namespace Invertex.UnityInputExtensions.Interactions
{
    //https://gist.github.com/Invertex
    /// <summary>
    /// Custom Hold interaction for New Input System.
    /// With this, the .performed callback will be called everytime the Input System updates. 
    /// Allowing a purely callback based approach to a button hold instead of polling it in an Update() loop and using bools
    /// .started will be called when the pressPoint threshold has been hit.
    /// .performed won't start being called until the 'duration' of holding has been met.
    /// .cancelled will be called when no-longer actuated
    /// </summary>
#if UNITY_EDITOR
    //Allow for the interaction to be utilized outside of Play Mode and so that it will actually show up as an option in the Input Manager
    [InitializeOnLoad]
#endif
    [UnityEngine.Scripting.Preserve, System.ComponentModel.DisplayName("Holding"), System.Serializable]
    public class CustomHoldingInteraction : IInputInteraction
    {
        public bool useDefaultSettingsPressPoint = false;
        public float pressPoint;

        public bool useDefaultSettingsDuration = false;
        public float duration;

        private float _heldTime = 0f;

        private float pressPointOrDefault => useDefaultSettingsPressPoint || pressPoint <= 0 ? InputSystem.settings.defaultButtonPressPoint : pressPoint;
        private float durationOrDefault => useDefaultSettingsDuration || duration <= 0 ? InputSystem.settings.defaultHoldTime : duration;

        private InputInteractionContext ctx;

        private void OnUpdate()
        {
            var isActuated = ctx.ControlIsActuated(pressPointOrDefault);
            var phase = ctx.phase;

            _heldTime += Time.deltaTime;

            //Cancel and cleanup our action if it's no-longer actuated or been externally changed to a stopped state.
            if (phase == InputActionPhase.Canceled || phase == InputActionPhase.Disabled || !ctx.action.actionMap.enabled || (!isActuated && (phase == InputActionPhase.Performed || phase == InputActionPhase.Started)))
            {
                Cancel(ref ctx);
                return;
            }

            if (_heldTime < durationOrDefault) { return; }  //Don't do anything yet, hold time not exceeded

            //We've held for long enough, start triggering the Performed state.
            if (phase == InputActionPhase.Performed || phase == InputActionPhase.Started)
            {
                ctx.PerformedAndStayPerformed();
            }
        }

        public void Process(ref InputInteractionContext context)
        {
            ctx = context; //Ensure our Update always has access to the most recently updated context

            if (ctx.phase != InputActionPhase.Performed && ctx.phase != InputActionPhase.Started && ctx.ControlIsActuated(pressPointOrDefault))
            {
                ctx.Started();

                InputSystem.onAfterUpdate -= OnUpdate; //Safeguard for duplicate registrations
                InputSystem.onAfterUpdate += OnUpdate;
            }
        }

        private void Cancel(ref InputInteractionContext context)
        {
            InputSystem.onAfterUpdate -= OnUpdate;
            _heldTime = 0f;

            if (context.phase != InputActionPhase.Canceled)
            {
                context.Canceled();
            }
        }

        public void Reset()
        {
            Cancel(ref ctx);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void RegisterInteraction()
        {
            if (InputSystem.TryGetInteraction("CustomHolding") == null)
            { //For some reason if this is called again when it already exists, it permanently removees it from the drop-down options... So have to check first
                InputSystem.RegisterInteraction<CustomHoldingInteraction>();
            }
        }

        //Constructor will be called by our Editor [InitializeOnLoad] attribute when outside Play Mode
        static CustomHoldingInteraction()
        {
            RegisterInteraction();
        }
    }

#if UNITY_EDITOR
    internal class CustomHoldInteractionEditor : InputParameterEditor<CustomHoldingInteraction>
    {
        private static GUIContent pressPointWarning, holdTimeWarning, pressPointLabel, holdTimeLabel;

        protected override void OnEnable()
        {

            pressPointLabel = new GUIContent("Press Point", "The minimum amount this input's actuation value must exceed to be considered \"held\".\n" +
            "Value less-than or equal to 0 will result in the 'Default Button Press Point' value being used from your 'Project Settings > Input System'.");

            holdTimeLabel = new GUIContent("Min Hold Time", "The minimum amount of realtime seconds before the input is considered \"held\".\n" +
            "Value less-than or equal to 0 will result in the 'Default Hold Time' value being used from your 'Project Settings > Input System'.");

            pressPointWarning = EditorGUIUtility.TrTextContent("Using \"Default Button Press Point\" set in project-wide input settings.");
            holdTimeWarning = EditorGUIUtility.TrTextContent("Using \"Default Hold Time\" set in project-wide input settings.");
        }

        public override void OnGUI()
        {
            DrawDisableIfDefault(ref target.pressPoint, ref target.useDefaultSettingsPressPoint, pressPointLabel, pressPointWarning);
            DrawDisableIfDefault(ref target.duration, ref target.useDefaultSettingsDuration, holdTimeLabel, holdTimeWarning);
        }

        private void DrawDisableIfDefault(ref float value, ref bool useDefault, GUIContent fieldName, GUIContent warningText)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(useDefault);
            value = EditorGUILayout.FloatField(fieldName, value);
            EditorGUI.EndDisabledGroup();
            useDefault = EditorGUILayout.ToggleLeft("Default", useDefault);
            EditorGUILayout.EndHorizontal();

            if (useDefault || value <= 0)
            {
                EditorGUILayout.HelpBox(warningText);
            }
        }
    }
#endif
}