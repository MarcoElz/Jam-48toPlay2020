using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAutoShooter : MonoBehaviour
{
    //Bullet
    [SerializeField] Transform bulletOrigin;
    [SerializeField] float timeBetweenShoots = 0.25f;
    [SerializeField] bool activateOnStart = true;

    private bool isReady;

    private float timeOfLastShoot;

    private void Start()
    {
        timeOfLastShoot = 0f;
        isReady = activateOnStart;
    }

    public void Activate(bool value)
    {
        isReady = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (isReady && GameManager.Instance.IsGameActive)
        {
            //Shoot
            if (Time.time > timeOfLastShoot + timeBetweenShoots)
            {
                Shoot();
                timeOfLastShoot = Time.time;
            }
        }
    }

    void Shoot()
    {
        BossPool.Instance.SpawnPooledObjectAt(bulletOrigin.transform.position, bulletOrigin.transform.rotation);
        //Instantiate(bulletPrefab, bulletOrigin.transform.position, bulletOrigin.transform.rotation);
    }
}
