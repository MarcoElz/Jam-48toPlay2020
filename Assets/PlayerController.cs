using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    public float HP { get; private set; }

    public event Action<float> onHPUpdate;

    [SerializeField] Shield shield;
    [SerializeField] SpriteRenderer spriteRenderer;
    public Color myColor { get; private set; }

    public GameObject linePrefab;
    private LineRenderer line;

    public float speed = 1f;
    public float rotationSpeed = 10f;
    public float startHP = 100f;
    public float radiusMovement = 1f;

    public bool autoShoot = true;

    private Vector3 movement;
    private Vector2 lookDir;

    public GameObject bulletPrefab;
    public Transform bulletOrigin;
    private float timeOfLastShoot;
    private float timeBetweenShoots = 0.25f;

    private Rigidbody2D rb;


    private bool canShoot;

    private DeadCircle deadCircle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        canShoot = true;
        timeOfLastShoot = 0f;
        HP = startHP;

        deadCircle = FindObjectOfType<DeadCircle>();
        deadCircle.RegisterToList(this);
    }

    public void SetColor(Color color)
    {
        myColor = color;
        spriteRenderer.color = color;
    }

    public void RemoveFromGame()
    {
        deadCircle = FindObjectOfType<DeadCircle>();
        deadCircle.UnregisterToList(this);
    }

    public void MoveInput(Vector2 vector)
    {
        //Debug.Log("Movement: " + vector);
        movement = (Vector3)vector;
    }

    public void LookInput(Vector2 vector)
    {
        //Debug.Log("Look Direction: " + vector);
        lookDir = vector;
        
    }

    public void Shield(bool active)
    {
        shield.Set(active);
        Debug.Log("Shield: " + active);
        canShoot = !active;
    }

    public void Dash(bool active)
    {
        
    }

    private void Update()
    {
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
        offset = offset * deadCircle.Radius;
        transform.position = offset;

        //Rotate around the circle
        if (movement.sqrMagnitude != 0f)
        {
            float x = movement.x;
            float perimetro = 2 * Mathf.PI * deadCircle.Radius;
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

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.transform.position, bulletOrigin.transform.rotation);  
    }

    public void Damage(float amount)
    {      
        HP -= amount;
        if(HP <= 0f)
        {
            //Reset
            transform.position = Vector3.zero;
            HP = startHP;
        }
        onHPUpdate?.Invoke(HP / startHP);
    }

    public void Heal(float amount)
    {
        HP += amount;
        onHPUpdate?.Invoke(HP / startHP);
    }
}
