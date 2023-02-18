using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;
    private PlayerInput playerInput;

    private InputActionMap guiLocationActionMap;
    private InputActionMap guiMenuActionMap;
    private InputActionMap gameChoicesActionMap;
    private InputActionMap dialogueBalloonActionMap;
    private InputActionMap continueGameActionMap;
    private InputActionMap fightMinigameActionMap;
    private InputActionMap itemSwitchMinigameActionMap;
    private InputActionMap flyMinigameActionMap;
    private InputActionMap pogojumpMinigameActionMap;

    public InputActionMap GUILocationActionMap => guiLocationActionMap;
    public InputActionMap GUIMenuActionMap => guiMenuActionMap;
    public InputActionMap GameChoicesActionMap => gameChoicesActionMap;
    public InputActionMap DialogueBalloonActionMap => dialogueBalloonActionMap;
    public InputActionMap ContinueGameActionMap => continueGameActionMap;
    public InputActionMap FightMinigameActionMap => fightMinigameActionMap;
    public InputActionMap ItemSwitchMinigameActionMap => itemSwitchMinigameActionMap;
    public InputActionMap FlyMinigameActionMap => flyMinigameActionMap;
    public InputActionMap PogoJumpMinigameActionMap => pogojumpMinigameActionMap;

    private void Start()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();

        InitializeActionMapFromActionSet(playerInput.actions);
    }

    private void InitializeActionMapFromActionSet(InputActionAsset assets)
    {
        guiLocationActionMap = assets.FindActionMap("GUI Location");
        guiMenuActionMap = assets.FindActionMap("GUI Menu");
        gameChoicesActionMap = assets.FindActionMap("Game Choices");
        dialogueBalloonActionMap = assets.FindActionMap("Dialogue Balloon");
        continueGameActionMap = assets.FindActionMap("Continue Game");
        fightMinigameActionMap = assets.FindActionMap("Fight Minigame");
        itemSwitchMinigameActionMap = assets.FindActionMap("Item Switch Minigame");
        flyMinigameActionMap = assets.FindActionMap("Fly Minigame");
        pogojumpMinigameActionMap = assets.FindActionMap("Pogo Jump Minigame");
    }

    public void SetGUIInputActionMapState(bool enabled)
    {
        if (enabled)
        {
            guiLocationActionMap.Enable();
            guiMenuActionMap.Enable();
        } else
        {
            guiLocationActionMap.Disable();
            guiMenuActionMap.Disable();
        }
    }
}
