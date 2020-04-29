using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadCircle : MonoBehaviour
{
    public float Radius { get { return transform.localScale.x / 2.0f; } }

    [SerializeField] float timeToFinish = 60f;
    [SerializeField] float damagePerSecond = 5f;
    private List<PlayerController> players;

    private float completeRadius;
    private float timer;

    private void Awake()
    {
        players = new List<PlayerController>();
        completeRadius = transform.localScale.x;
        timer = timeToFinish;
    }

    private void Update()
    {
        if(timer <= 5f)
        {
            timer = timeToFinish;
            transform.localScale = Vector3.one * completeRadius;
            return;
        }

        timer -= Time.deltaTime;
        transform.localScale -= Vector3.one * (completeRadius / timeToFinish * Time.deltaTime);
        CheckPlayers();
    }

    public void RegisterToList(PlayerController player)
    {
        players.Add(player);
    }

    void CheckPlayers()
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
