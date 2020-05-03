using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnBecameInvisible : MonoBehaviour
{
    [SerializeField] GameObject toDestroy;

    [SerializeField] bool isBossBullet = false;

    private void OnBecameInvisible()
    {
        if(isBossBullet)
            BossPool.Instance.SaveObjectToPool(toDestroy);
        else
            ObjectPool.Instance.SaveObjectToPool(toDestroy);
    }
}
