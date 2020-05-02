using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public float HP { get; protected set; }
    public bool IsAlive { get; protected set; }
    public int DeviceId { get; protected set; }

    public event Action<float> onHPUpdate;
    public event Action<Color> onNewColor;
    public Color myColor { get; protected set; }

    //Stats
    [Header("Stats")]
    [SerializeField] protected float speed = 1f;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected float startHP = 100f;
    [SerializeField] protected float timeBetweenShoots = 0.25f;


    //Bullet
    [Header("Bullet")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletOrigin;

    [Header("Other")]
    [SerializeField] protected GameObject destroyedParticlesPrefab;

    //Render
    //[Header("Render")]
    //[SerializeField] SpriteRenderer spriteRenderer;

    [Header("Deprecated")]
    //Shield //Delete?
    [SerializeField] Shield shield;

    //Input
    protected Vector3 movement;
    protected Vector2 lookDir;

    //Cache
    protected Rigidbody2D rb;
    protected float timeOfLastShoot;
    protected bool canShoot;
    protected GameRing deadRing;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        deadRing = FindObjectOfType<GameRing>();
        canShoot = true;
        timeOfLastShoot = 0f;

        Restart();

        transform.right = (-transform.position + Vector3.zero).normalized;
    }

    public virtual void Restart()
    {
        HP = startHP;
        IsAlive = true;
        onHPUpdate?.Invoke(HP / startHP);
        onNewColor?.Invoke(myColor);
    }

    protected void OnHPUpdate()
    {
        onHPUpdate?.Invoke(HP / startHP);
    }
    protected void OnNewColor(Color color)
    {
        onNewColor?.Invoke(myColor);
    }

    public void SetColor(Color color)
    {
        myColor = color;
        onNewColor?.Invoke(color);
    }

    public void SetDeviceId(int id)
    {
        DeviceId = id;
    }

    public void MoveInput(Vector2 vector)
    {
        //Debug.Log("Movement: " + vector);
        movement = (Vector3)vector;
    }

    //Deprecated
    public void Shield(bool active)
    {
        shield.Set(active);
        Debug.Log("Shield: " + active);
        canShoot = !active;
    }

    protected virtual void Update()
    {
        if (!GameManager.Instance.IsGameActive)
            return;

        //Verify never is in the center
        if(transform.position.sqrMagnitude == 0)
        {
            float x = UnityEngine.Random.Range(-7f, 7f);
            float y = UnityEngine.Random.Range(-4f, 4f);
            transform.position = new Vector3(x, y, 0f);
        }

        //Always in the position of the circle
        Vector3 offset = transform.position - Vector3.zero;
        offset = offset.normalized;
        offset = offset * deadRing.Radius;
        transform.position = offset;

        //Rotate around the circle
        if (movement.sqrMagnitude != 0f)
        {
            float x = movement.x;
            float perimetro = 2 * Mathf.PI * deadRing.Radius;
            transform.RotateAround(Vector3.zero, Vector3.forward, x * speed  * 360/perimetro * Time.deltaTime);
        }

        //LookAt
        //transform.right = Vector2.Lerp(transform.right, lookDir, rotationSpeed);  
        transform.right = (-transform.position + Vector3.zero).normalized;

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

    public virtual void ForceCenterLook()
    {
        transform.right = (-transform.position + Vector3.zero).normalized;
    }

    protected virtual void Shoot()
    {
        ObjectPool.Instance.SpawnPooledObjectAt(bulletOrigin.transform.position, bulletOrigin.transform.rotation);
        //Instantiate(bulletPrefab, bulletOrigin.transform.position, bulletOrigin.transform.rotation);   
    }

    public virtual void Damage(float amount)
    {
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
            for(int i = 0; i < colors.Length; i++)
            {
                colors[i].Init(this);
            }
            Destroy(go, 1.5f);
        }
        else //Still alive
        {
            onHPUpdate?.Invoke(HP / startHP);

            if (colorRoutine != null) StopCoroutine(colorRoutine); //Stop if there is a routine running
            colorRoutine = StartCoroutine(ColorDamageRoutine()); //Start new one
        }     
    }

    public virtual void Heal(float amount)
    {
        HP += amount;
        HP = Mathf.Clamp(HP, 0f, startHP);
        onHPUpdate?.Invoke(HP / startHP);
    }

    protected Coroutine colorRoutine;

    protected IEnumerator ColorDamageRoutine()
    {
        //Start white
        onNewColor?.Invoke(Color.white);

        float duration = 0.4f;
        float time = 0f;
        float speedRate = 1 / duration;

        while (time < duration)
        {
            time += Time.deltaTime * speedRate;
            //Lerp
            onNewColor?.Invoke(Color.Lerp(Color.white, myColor, time));
            yield return null; //Wait frame
        }

        //End original color
        onNewColor?.Invoke(myColor);
    }
}
