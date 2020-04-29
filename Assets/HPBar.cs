using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] PlayerController player;

    [SerializeField] Image top;
    [SerializeField] Image bot;

    private void Start()
    {
        player.onHPUpdate += ChangeHP;
    }

    void ChangeHP(float percentage)
    {
        top.fillAmount = percentage;
        bot.fillAmount = percentage;
    }
}
