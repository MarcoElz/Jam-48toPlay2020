using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRandomAI : MonoBehaviour
{

    PlayerController player;

    float timer;
    float direction;

    private void Start()
    {
        player = GameManager.Instance.CreatePlayer(101 + Random.Range(1,10000));
        GetDirection();     
    }

    private void Update()
    {
        if(timer <= 0f)
        {
            GetDirection();
        }

        timer -= Time.deltaTime;

        //Movement
        float h = direction;
        //float v = rawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");
        Vector2 input = new Vector2(h, 0f);
        player.MoveInput(input);
    }

    private void GetDirection()
    {
        timer = Random.Range(0.5f, 1.5f);
        direction = Random.Range(-1f, 1f) < 0 ? -1 : 1;
    }

}
