using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShieldComponentsByPercentage
{
    [Range(0f, 1f)]
    public float percentage;
    public GameObject collider;
}

public class Shield : MonoBehaviour, IDamageable
{
    public float Life { get; private set; }
    public bool IsActive { get; private set; }

    public event Action<float> onUpdate;
    public event Action<bool> onActivate;

    [SerializeField] float maxLife = 100f;
    [SerializeField] float duration = 1.0f;
    [SerializeField] float healDuration = 1.0f;
    [SerializeField] ShieldComponentsByPercentage[] parts;
    [SerializeField] Collider2D playerCollider;


    private void Start()
    {
        Life = maxLife;
        onUpdate += OnLifeUpdate;
        Set(false);
    }

    public void Set(bool active)
    {
        if (IsActive == active)
            return;

        IsActive = active;
        if (IsActive)
        {
            if (Life < 0.1f)
                playerCollider.enabled = true;
            else
                playerCollider.enabled = false;
        }
        else
        {
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].collider.SetActive(false);
            }
            playerCollider.enabled = true;
        }   
        onActivate?.Invoke(IsActive);
    }

    private void Update()
    {
        if(IsActive)
        {
            Damage(Time.deltaTime * (maxLife / duration));
        }
        else
        {
            Heal(Time.deltaTime * (maxLife / healDuration));
        }
    }

    private void OnLifeUpdate(float percentage)
    {
        bool isFinished = false;
        for (int i = 0; i < parts.Length; i++)
        {
            if(percentage > parts[i].percentage && !isFinished && IsActive)
            {
                parts[i].collider.SetActive(true);
                isFinished = true;
            }
            else
            {
                parts[i].collider.SetActive(false);
            }
            
        }

        if(IsActive && percentage > 0.01f)
            playerCollider.enabled = false;
        else
            playerCollider.enabled = true;
    }

    public void Damage(float amount)
    {
        Life = Mathf.Clamp(Life - amount, 0f, maxLife);
        onUpdate?.Invoke(Life / maxLife);
    }

    public void Heal(float amount)
    {
        Life = Mathf.Clamp(Life + amount, 0f, maxLife);
        onUpdate?.Invoke(Life / maxLife);
    }
}
