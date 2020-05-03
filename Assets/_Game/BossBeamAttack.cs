using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBeamAttack : BossAttacks
{

    [SerializeField] float damage = 5f;

    public override void StartAttack()
    {
        //Wait and change opacity

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.Damage(damage);
        }
    }
}
