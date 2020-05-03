using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRotationAttack : BossAttacks
{

    [SerializeField] int movementDirection;
    [SerializeField] float speed;

    Vector3 euler;

    private void Start()
    {
        euler = transform.rotation.eulerAngles;
    }

    public override void StartAttack()
    {
        //Do nothing
    }

    private void Update()
    {
        if (movementDirection != 0)
        {
            euler.z += movementDirection * speed * Time.deltaTime; // Mathf.Lerp(startAngle, endAngle, actualTime);

            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
