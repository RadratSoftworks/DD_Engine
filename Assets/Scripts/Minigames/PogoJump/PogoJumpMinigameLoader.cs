using UnityEngine;

public class PogoJumpMinigameLoader : MonoBehaviour
{
    public static PogoJumpMinigameLoader Instance;

    [SerializeField]
    private GameObject guiAnimationPrefab;

    [SerializeField]
    private GameObject guiPicturePrefab;

    [SerializeField]
    private GameObject pogoJumpScenePrefabObject;

    private void Start()
    {
        Instance = this;
    }

    public GUIControlSet Load(PogoJumpMinigameInfo info, string filename, Vector2 viewResolution)
    {
        GUIControlSet pogoJumpControlSet = new GUIControlSet(GUIControlSetFactory.Instance.container,
            pogoJumpScenePrefabObject, filename, viewResolution,
            new GUIControlSetInstantiateOptions(destroyWhenDisabled: true, preferredDpad: true));

        PogoJumpSceneController pogoJumpSceneController = pogoJumpControlSet.GameObject.GetComponent<PogoJumpSceneController>();
        if (pogoJumpSceneController != null)
        {
            pogoJumpSceneController.Setup(info, guiAnimationPrefab, guiPicturePrefab);
        }

        return pogoJumpControlSet;
    }
}
