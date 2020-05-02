using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPlayer : PlayerController
{
    protected override void Start()
    {
        deadRing = FindObjectOfType<GameRing>();
        canShoot = true;
        timeOfLastShoot = 0f;

        Restart();

        //transform.right = (-transform.position + Vector3.zero).normalized;
        transform.right = transform.up;
    }

    public override void Restart()
    {
        HP = startHP;
        IsAlive = true;
        OnHPUpdate();
        OnNewColor(myColor);
    }

    protected override void Update()
    {
        if (!GameManager.Instance.IsGameActive)
            return;

        //Always in the position of the circle
        Vector3 offset = transform.position - Vector3.zero;
        offset = offset.normalized;
        offset = offset * deadRing.Radius;
        transform.position = Vector3.zero;

        //Rotate around the circle
        if (movement.sqrMagnitude != 0f)
        {
            float x = movement.x;
            //float perimetro = 2 * Mathf.PI * deadRing.Radius;
            transform.Rotate(x * speed * Time.deltaTime * Vector3.forward);
            //transform.RotateAround(Vector3.zero, Vector3.forward, x * speed * 360 / perimetro * Time.deltaTime);
        }

        //LookAt
        //transform.right = Vector2.Lerp(transform.right, lookDir, rotationSpeed);  
        //transform.right = (-transform.position + Vector3.zero).normalized;

        //Shoot
        if (canShoot)// && lookDir.magnitude > 0.2f)
        {
            if (Time.time > timeOfLastShoot + timeBetweenShoots)
            {
                Shoot();
                timeOfLastShoot = Time.time;
            }

        }

    }

    public override void ForceCenterLook()
    {
        transform.right = transform.up;
    }

    public override void Damage(float amount)
    {
        //Damage must be by other method. A weak point
        return;

        if (!GameManager.Instance.IsGameActive)
            return;

        HP -= amount;
        if (HP <= 0f)
        {
            //Dead
            IsAlive = false;
            GameManager.Instance.PlayerKilled();
            this.gameObject.SetActive(false);
            //Spawn particles
            GameObject go = Instantiate(destroyedParticlesPrefab, transform.position, transform.rotation);
            ParticleColorChanger[] colors = go.GetComponentsInChildren<ParticleColorChanger>();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].Init(this);
            }
            Destroy(go, 1.5f);
        }
        else //Still alive
        {
            OnHPUpdate();

            if (colorRoutine != null) StopCoroutine(colorRoutine); //Stop if there is a routine running
            colorRoutine = StartCoroutine(ColorDamageRoutine()); //Start new one
        }
    }

    public override void Heal(float amount)
    {
        HP += amount;
        HP = Mathf.Clamp(HP, 0f, startHP);
        OnHPUpdate();
    }


    public void WeakPointDamage(float amount)
    {
        if (!GameManager.Instance.IsGameActive)
            return;

        HP -= amount;
        if (HP <= 0f)
        {
            //Dead
            IsAlive = false;
            GameManager.Instance.PlayerBossKilled();
            this.gameObject.SetActive(false);
            //Spawn particles
            GameObject go = Instantiate(destroyedParticlesPrefab, transform.position, transform.rotation);
            ParticleColorChanger[] colors = go.GetComponentsInChildren<ParticleColorChanger>();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].Init(this);
            }
            Destroy(go, 1.5f);
        }
        else //Still alive
        {
            OnHPUpdate();

            if (colorRoutine != null) StopCoroutine(colorRoutine); //Stop if there is a routine running
            colorRoutine = StartCoroutine(ColorDamageRoutine()); //Start new one
        }

        
    }
}
