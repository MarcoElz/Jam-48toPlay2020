using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float baseDamage = 0f;
    [SerializeField] float sizeMultiplier = 0.5f;
    [SerializeField] float multiplierPerSecond = 5.0f;

    private float actualDamage;

    private float updateCount = 0;

    private float timeOfCreation;

    private Transform t;

    private Vector3 scaler;
    private Vector3 startScale;

    private void Awake()
    {
        t = transform;
        scaler = Vector3.one * sizeMultiplier;
        startScale = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = startScale;
        actualDamage = baseDamage;
        timeOfCreation = Time.time;   
    }

    private void LateUpdate()
    {

        t.position += this.t.right * Time.deltaTime * speed;

        updateCount++;
        if(updateCount < 3)
        {
            return;
        }
        updateCount = 0f;

        t.localScale += scaler * Time.deltaTime;       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if(damageable != null)
        {
            actualDamage += (Time.time - timeOfCreation) * multiplierPerSecond;

            damageable.Damage(actualDamage + baseDamage);
            ObjectPool.Instance.UsePooledParticle(transform.position, transform.rotation);

            ObjectPool.Instance.SaveObjectToPool(this.gameObject);
        }
    }
}
