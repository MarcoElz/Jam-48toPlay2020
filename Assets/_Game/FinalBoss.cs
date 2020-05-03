using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour, IDamageable
{
    public float HP { get; private set; }
    public bool IsAlive { get; private set; }

    [SerializeField] float startHP = 5000f;
    [SerializeField] float speed = 90f;

    [SerializeField] Transform attacks;

    public event Action<float> onHPUpdate;

    private BossAttacks[] bossAttacks;

    private bool inDanger;

    float movementDecisionTimer;
    float movementDirection;

    Vector3 euler;

    private void Start()
    {
        transform.up = -transform.up;
        bossAttacks = attacks.GetComponentsInChildren<BossAttacks>(true);
        euler = transform.rotation.eulerAngles;
        GetDirection();
        Restart();
    }

    public void Restart()
    {
        HP = startHP;

    }

    private void Update()
    {
        //if (!GameManager.Instance.IsGameActive)
        //    return;

        if (movementDirection != 0)
        {
            euler.z += movementDirection * speed * Time.deltaTime; // Mathf.Lerp(startAngle, endAngle, actualTime);

            transform.rotation = Quaternion.Euler(euler);
        }

        if (movementDecisionTimer <= 0f)
        {
            GetDirection();
        }
        movementDecisionTimer -= Time.deltaTime;
    }

    private void GetDirection()
    {
        movementDecisionTimer = UnityEngine.Random.Range(3f, 5f);
        movementDirection = UnityEngine.Random.Range(-1f, 1f) < 0 ? -1 : 1;

        ChangeAttack();
    }

    private void ChangeAttack()
    {
        //Stop all
        for (int i = 0; i < bossAttacks.Length; i++)
        {
            bossAttacks[i].gameObject.SetActive(false);
        }

        float percentage = (HP / startHP);
        int n = 1;
        if (percentage < 0.8f) n = 2;
        if (percentage < 0.3f) n = 3;

        for (int i = 0; i < n; i++)
        {
            int random = UnityEngine.Random.Range(0, bossAttacks.Length);
            if (!bossAttacks[random].gameObject.activeInHierarchy)
            {
                bossAttacks[random].gameObject.SetActive(true);
                bossAttacks[random].StartAttack();
            }
        }

    }

    public void Damage(float amount)
    {
        if (!GameManager.Instance.IsGameActive)
            return;

        HP -= amount;
        if (HP <= 0f)
        {
            //Dead
            //IsAlive = false;
            GameManager.Instance.FinalBossKilled();
            //this.gameObject.SetActive(false);
        }
        else //Still alive
        {
            onHPUpdate?.Invoke(HP / startHP);

            if(!inDanger && HP / startHP < 0.25f)
            {
                inDanger = true;
            }
        }
    }

    public void Heal(float amount)
    {
        HP = Mathf.Clamp(HP + amount, 0f, startHP);
    }
}
