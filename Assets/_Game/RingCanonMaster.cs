using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingCanonMaster : MonoBehaviour
{

    public bool IsReady { get; private set; }

    [SerializeField] GameObject canonPrefab;

    [SerializeField] float speed = 5f;
    [SerializeField] int numberOfCanons = 8;

    private GameObject[] canons;


    float timer;
    float direction;

    Vector3 euler;

    private void Start()
    {
        //CreateCanons();
        euler = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (!IsReady)
            return;

        if(direction != 0)
        {
            euler.z +=  direction * speed * Time.deltaTime; // Mathf.Lerp(startAngle, endAngle, actualTime);

            transform.rotation = Quaternion.Euler(euler);
        }

        if (timer <= 0f)
        {
            GetDirection();
        }   
        timer -= Time.deltaTime;
    }


    private void GetDirection()
    {
        timer = Random.Range(1.5f, 3f);
        direction = Random.Range(-1f, 1f) < 0 ? -1 : 1;
    }

    public void CreateCanons()
    {
        StartCoroutine(CreateCanonsRoutine());
    }

    IEnumerator CreateCanonsRoutine()
    {
        canons = new GameObject[numberOfCanons];
        GameRing ring = FindObjectOfType<GameRing>();
        float radius = ring.MaxRingSize - 1.5f;
        for (int i = 0; i < canons.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfCanons;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            GameObject canon = Instantiate(canonPrefab, this.transform);
            canon.transform.position = pos;
            canons[i] = canon;
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < canons.Length; i++)
        {
            canons[i].GetComponent<AutoShooter>().Activate(true);
        }

        IsReady = true;
    }

}
