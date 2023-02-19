﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSiteSceneController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    [SerializeField]
    private SpriteRenderer signRenderer;

    [SerializeField]
    private SpriteRenderer robotAndGuyRenderer;

    [SerializeField]
    private SpriteRenderer whiskyRenderer;

    [SerializeField]
    private ConstructionSiteHostileController trapHostile;

    [SerializeField]
    private ConstructionSiteHostileController leftBirdHostile;

    [SerializeField]
    private ConstructionSiteHostileController rightBirdHostile;

    [SerializeField]
    private ConstructionSiteHostileController whiskyHostile;

    [SerializeField]
    private ConstructionSiteHostileController manHostile;

    [SerializeField]
    private ConstructionSiteHostileController winHostile;

    [SerializeField]
    private ConstructionSiteHarryController harryController;

    [SerializeField]
    private CompositeCollider2D cameraBounds;

    public void Setup(ConstructionSiteMinigameInfo info)
    {
        backgroundRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            info.BackgroundImage);
        signRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            info.SignImage, Vector2.zero);
        robotAndGuyRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            info.RobotAndGuyImage, Vector2.zero);
        whiskyRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            info.WhiskyImage, Vector2.zero);

        trapHostile.Setup(info.Trap);
        leftBirdHostile.Setup(info.LeftBird);
        rightBirdHostile.Setup(info.RightBird);
        whiskyHostile.Setup(info.Whisky);
        manHostile.Setup(info.Man);
        winHostile.Setup(info.Win);

        harryController.Setup(info.FlyPosition);

        // Switch to full view
        GameViewController.Instance.SetFullViewWithFocus(harryController.transform, cameraBounds);
    }
}