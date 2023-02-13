using System;
using System.Collections.Generic;
using UnityEngine;

public class FightHealthBarController : MonoBehaviour
{
    [SerializeField]
    private FighterHealthController healthController;

    private void Start()
    {
        healthController.HealthChanged += OnHealthChange;
    }

    private void OnHealthChange()
    {

    }
}
