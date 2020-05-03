using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [SerializeField] FinalBoss boss;

    [SerializeField] Image bar;

    private void Start()
    {
        boss.onHPUpdate += ChangeHP;
    }

    void ChangeHP(float percentage)
    {
        bar.fillAmount = percentage;
    }
}
