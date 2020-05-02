using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRing : MonoBehaviour
{
    public float Radius { get { return transform.localScale.x * 0.5f; } }
    public float MaxRadiusSize { get { return completeRadius * 0.5f; } }
    public float MinRadiusSize {  get { return minRadius; } }

    [SerializeField] float timeToFinish = 60f;
    [SerializeField] float minRadius = 5f;


    private float completeRadius;

    private float reductionRate;

    private void Awake()
    {
        completeRadius = transform.localScale.x;
    }

    private void Start()
    {
        GameManager.Instance.onGameStart += OnStartGame;

        reductionRate = (completeRadius - MinRadiusSize * 2f) / timeToFinish;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameActive)
            return;

        if(Radius < minRadius)
        {
            //Stop
            return;
        }

        //Reduce Area
        transform.localScale -= Vector3.one * (reductionRate * Time.deltaTime);

    }

    void OnStartGame()
    {
        Restart();
    }

    public void Restart()
    {
        transform.localScale = Vector3.one * completeRadius;
    }

}
