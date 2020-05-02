using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public Transform transform;
    public ParticleSystem particleSystem;

    public Particle(Transform transform, ParticleSystem particleSystem)
    {
        this.transform = transform;
        this.particleSystem = particleSystem;
    }
}

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab = default;
    [SerializeField] int startBulletAmount = 200;

    [SerializeField] GameObject bulletParticlePrefab = default;
    [SerializeField] int startParticleAmount = 200;

    public static ObjectPool Instance { get; private set; }

    private Queue<GameObject> pooledObjects;
    private List<Particle> pooledParticlesObjects;

    private int lastParticleIndex;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        pooledObjects = new Queue<GameObject>();
        for (int i = 0; i < startBulletAmount; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pooledObjects.Enqueue(obj);
        }

        pooledParticlesObjects = new List<Particle>();
        for (int i = 0; i < startParticleAmount; i++)
        {
            GameObject obj = Instantiate(bulletParticlePrefab);
            Particle particle = new Particle(obj.transform, obj.GetComponent<ParticleSystem>());
            pooledParticlesObjects.Add(particle);
        }
    }

    public GameObject GetPooledObject()
    {
        if(pooledObjects.Count > 0)
        {
            GameObject go = pooledObjects.Dequeue();
            go.SetActive(true);
            //pooledObjects.RemoveAt(0);

            if(go != null)
                return go;
        }

        return Instantiate(bulletPrefab);
    }

    public GameObject SpawnPooledObjectAt(Vector3 position, Quaternion rotation)
    {
        if (pooledObjects.Count > 0)
        {
            GameObject go = pooledObjects.Dequeue();
            if (go != null && !go.activeInHierarchy)
            {
                go.transform.SetPositionAndRotation(position, rotation);
                go.SetActive(true);
                //pooledObjects.RemoveAt(0);
                return go;
            }
                
        }
        GameObject goj = Instantiate(bulletPrefab, position, rotation);
        return goj;
    }

    public void SaveObjectToPool(GameObject go)
    {
        pooledObjects.Enqueue(go);
        go.SetActive(false);
    }

    public void UsePooledParticle(Vector3 position, Quaternion rotation)
    {
        if (lastParticleIndex > pooledParticlesObjects.Count - 1)
            lastParticleIndex = 0;

        pooledParticlesObjects[lastParticleIndex].transform.SetPositionAndRotation(position, rotation);
        pooledParticlesObjects[lastParticleIndex].particleSystem.Play();

        lastParticleIndex++;
    }


}
