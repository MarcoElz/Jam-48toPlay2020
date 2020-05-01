using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnBecameInvisible : MonoBehaviour
{
    [SerializeField] GameObject toDestroy;

    private void OnBecameInvisible()
    {
        Destroy(toDestroy);
    }
}
