using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public string GameDataPath = "G:\\bent\\DD_RV\\opes\\";

    private ResourceFile generalResources;
    private SpriteLoader spriteLoader;

    public GameObject GUIPicturePrefab;

    private IEnumerator LoadMainChapterCoroutine()
    {
        yield return StartCoroutine(LoadDataCoroutine());

        ResourceInfo mainChapterInfo = generalResources.Resources[FilePaths.MainChapterGUIControlFileName];
        byte []mainChapterGUIData = generalResources.ReadResourceData(mainChapterInfo);

        using (MemoryStream memStream = new MemoryStream(mainChapterGUIData))
        {
            var control = new GUIControlFile(memStream);
            foreach (GUIControl ctrl in control.Controls)
            {
                if (ctrl is GUIControlPicture)
                {
                    GUIControlPicture pic = (GUIControlPicture)ctrl;
                    Sprite drawSprite = spriteLoader.Load(generalResources, pic.ImagePath);

                    GameObject obj = Instantiate(GUIPicturePrefab, pic.TopPosition, Quaternion.identity);
                    var renderer = obj.GetComponent<SpriteRenderer>();
                    renderer.sprite = drawSprite;
                }
            }
        }

        yield break;
    }

    private IEnumerator LoadDataCoroutine()
    {
        generalResources = new ResourceFile(Path.Join(GameDataPath, FilePaths.GeneralResourceFileName));
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteLoader = new SpriteLoader();

        StartCoroutine(LoadMainChapterCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
