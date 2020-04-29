using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldColliderDamageable : MonoBehaviour, IDamageable
{

    Shield shield;

    private void Awake()
    {
        shield = transform.parent.GetComponent<Shield>();
    }

    public void Damage(float amount)
    {
        shield.Damage(amount);
    }

    public void Heal(float amount)
    {
        //No heal
    }

    


}
