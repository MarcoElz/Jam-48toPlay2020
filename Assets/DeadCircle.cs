using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadCircle : MonoBehaviour
{
    public float Radius { get { return transform.localScale.x * 0.5f; } }

    [SerializeField] float timeToFinish = 60f;
    [SerializeField] float damagePerSecond = 5f;
    [SerializeField] float minRadius = 5f;
    private List<PlayerController> players;

    private float completeRadius;
    private float timer;

    private void Awake()
    {
        players = new List<PlayerController>();
        completeRadius = transform.localScale.x;
        timer = timeToFinish;
    }

    private void Start()
    {
        GameManager.Instance.onGameStart += OnStartGame;
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
        timer -= Time.deltaTime;
        transform.localScale -= Vector3.one * (completeRadius / timeToFinish * Time.deltaTime);

        //Damage Players
        if(damagePerSecond > 0f)
            DamagePlayers();
    }

    void OnStartGame()
    {
        Restart();
    }

    public void Restart()
    {
        completeRadius = transform.localScale.x;
        timer = timeToFinish;
    }

    public void RegisterToList(PlayerController player)
    {
        players.Add(player);
    }

    public void UnregisterToList(PlayerController player)
    {
        players.Remove(player);
    }

    void DamagePlayers()
    {
        float radius = Radius;
        for (int i = 0; i < players.Count; i++)
        {
            float distance = Vector3.Distance(players[i].transform.position, transform.position);
            if(distance > radius)
            {
                players[i].Damage(Time.deltaTime * damagePerSecond);
            }
        }
    }

}
