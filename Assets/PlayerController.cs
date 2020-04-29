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
        //GameObject lineObject = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        //line = lineObject.GetComponent<LineRenderer>();
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
        //Vector3 direction = this.transform.position - deadCircle.transform.position;
        //float distanceToCenter = direction.magnitude;
        //float distanceToCircunference = deadCircle.Radius - distanceToCenter;
        //line.SetPosition(0, this.transform.position);
        //line.SetPosition(1, this.transform.position + (direction.normalized * (distanceToCircunference)));

        //var t = distanceToCircunference / deadCircle.Radius;
        //float width = Mathf.Lerp(0.4f, 0.05f, Mathf.Abs(t));
        //line.startWidth = width;
        //line.endWidth = width;


        ////Limit
        //float radius = radiusMovement; //radius of *black circle*
        //Vector3 centerPosition = this.transform.position + (direction.normalized * (distanceToCircunference)); //center of *black circle*
        //float distance = Vector3.Distance(this.transform.position, centerPosition); //distance from ~green object~ to *black circle*

        //if (distance > radius) //If the distance is less than the radius, it is already within the circle.
        //{
        //    Vector3 fromOriginToObject = this.transform.position - centerPosition; //~GreenPosition~ - *BlackCenter*
        //    fromOriginToObject *= radius / distance; //Multiply by radius //Divide by Distance
        //    this.transform.position = centerPosition + fromOriginToObject; //*BlackCenter* + all that Math
        //}

        if (transform.position.magnitude == 0)
            transform.position = new Vector3(1f, 0f, 0f);

        Vector3 offset = transform.position - Vector3.zero;
        offset = offset.normalized;
        offset = offset * deadCircle.Radius;
        transform.position = offset;

        if (movement.sqrMagnitude != 0f)
        {
            float x = movement.x;
            float perimetro = 2 * Mathf.PI * deadCircle.Radius;

            transform.RotateAround(Vector3.zero, Vector3.forward, x * speed  * 360/perimetro * Time.deltaTime);

            //rb.velocity = Vector3.ClampMagnitude(movement, 1.0f) * speed;
        }

    }
 
    private void FixedUpdate()
    {
        
        //Movement
        if (movement.sqrMagnitude != 0f)
        {
            //float x = movement.x;
            //transform.RotateAround(Vector3.zero, Vector3.forward, x * speed * Time.fixedDeltaTime);

            //rb.velocity = Vector3.ClampMagnitude(movement, 1.0f) * speed;
        }
        else
        {
            //rb.velocity = Vector2.zero;
        }


        //LookAt
        //transform.right = Vector2.Lerp(transform.right, lookDir, rotationSpeed);  
        transform.right = (-transform.position + Vector3.zero).normalized;

        //Shoot
        if (canShoot && lookDir.magnitude > 0.2f)
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
