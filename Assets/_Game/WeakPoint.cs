using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour, IDamageable
{

    [SerializeField] BossPlayer bossPlayer;

    public void Damage(float amount)
    {
        bossPlayer.WeakPointDamage(amount);
    }

    public void Heal(float amount)
    {
        
    }
}
