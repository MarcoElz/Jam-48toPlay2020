using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldBar : MonoBehaviour
{
    [SerializeField] Shield shield;
    [SerializeField] Image top;
    [SerializeField] Image bot;
    [SerializeField] float deactivatedAlpha = 0.25f;


    private void Awake()
    {
        ChangeVisibility(false);
        ChangeFillValue(1.0f);
    }

    private void Start()
    {  
        shield.onUpdate += ChangeFillValue;
        shield.onActivate += ChangeVisibility;
    }

    void ChangeFillValue(float percentage)
    {
        top.fillAmount = percentage;
        bot.fillAmount = percentage;
    }

    void ChangeVisibility(bool active)
    {
        Color color = top.color;
        color.a = active ? 1.0f : deactivatedAlpha;

        top.color = color;
        bot.color = color;
    }
}
