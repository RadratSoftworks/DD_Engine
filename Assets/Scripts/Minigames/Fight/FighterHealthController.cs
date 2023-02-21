using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FighterHealthController : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 50;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => CurrentHealth == 0;

    public event Action HealthChanged;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageCount)
    {
        if (currentHealth == 0)
        {
            return;
        }

        currentHealth = Math.Max(0, currentHealth - damageCount);
        HealthChanged?.Invoke();

        //Debug.Log("Current health points: " + currentHealth);
    }
}
