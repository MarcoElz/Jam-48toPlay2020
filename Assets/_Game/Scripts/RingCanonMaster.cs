using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingCanonMaster : MonoBehaviour
{

    public bool IsReady { get; private set; }

    [SerializeField] GameObject canonPrefab;

    [SerializeField] float speed = 5f;
    [SerializeField] int numberOfCanons = 8;
    [SerializeField] float radius = 18.5f;
    [SerializeField] bool lookInside = false;
    [SerializeField] bool readyOnStart = false;

    private GameObject[] canons;


    float movementDecisionTimer;
    float movementDirection;

    Vector3 euler;

    private void Start()
    {
        //CreateCanons();
        euler = transform.rotation.eulerAngles;

        if(readyOnStart)
            CreateCanons();
    }

    private void Update()
    {
        if (!IsReady)
            return;

        if(movementDirection != 0)
        {
            euler.z +=  movementDirection * speed * Time.deltaTime; // Mathf.Lerp(startAngle, endAngle, actualTime);

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
        movementDecisionTimer = Random.Range(1.5f, 3f);
        movementDirection = Random.Range(-1f, 1f) < 0 ? -1 : 1;
    }

    public void CreateCanons()
    {
        StartCoroutine(CreateCanonsRoutine());
    }

    IEnumerator CreateCanonsRoutine()
    {
        canons = new GameObject[numberOfCanons];
        GameRing ring = FindObjectOfType<GameRing>();
        for (int i = 0; i < canons.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfCanons;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            GameObject canon = Instantiate(canonPrefab, this.transform);
            canon.transform.position = pos;

            if(lookInside)
                canon.transform.up = -(-canon.transform.position + Vector3.zero).normalized; //Look to center
            else
                canon.transform.up = (-canon.transform.position + Vector3.zero).normalized; //Look to outside

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
