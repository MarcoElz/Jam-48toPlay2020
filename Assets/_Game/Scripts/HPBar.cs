using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] PlayerController player;

    [SerializeField] Image bar;

    private void Start()
    {
        player.onHPUpdate += ChangeHP;
    }

    void ChangeHP(float percentage)
    {
        bar.fillAmount = percentage;
    }
}
