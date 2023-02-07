using System;
using UnityEngine;

public class GameContinueConfirmatorController : MonoBehaviour
{
    public bool Confirmed { get; set; } = false;

    public void StartHearing()
    {
        Confirmed = false;
        gameObject.SetActive(true);
    }

    public void OnContinueConfirmed()
    {
        Confirmed = true;
        gameObject.SetActive(false);
    }
}
