using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float baseDamage = 0f;
    [SerializeField] float sizeMultiplier = 0.5f;
    [SerializeField] float multiplierPerSecond = 5.0f;
    [SerializeField] GameObject particlesPrefab;
    private float actualDamage;

    void Start()
    {
        this.GetComponent<Rigidbody2D>().velocity = this.transform.right * speed;
        actualDamage = baseDamage;
        //Destroy(this.gameObject, 10f);
    }

    private void Update()
    {
        transform.localScale += Vector3.one * Time.deltaTime * sizeMultiplier;
        actualDamage += Time.deltaTime * multiplierPerSecond;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if(damageable != null)
        {
            damageable.Damage(actualDamage);
            Destroy(this.gameObject);

            GameObject go = Instantiate(particlesPrefab, transform.position, transform.rotation);
            Destroy(go, 1f);
        }

    }
}
