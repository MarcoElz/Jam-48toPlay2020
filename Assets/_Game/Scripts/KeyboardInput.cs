using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] bool rawInput;

    PlayerController player;

    private void Start()
    {
        player = GameManager.Instance.CreatePlayer(101);
    }

    private void Update()
    {
        //Movement
        float h = rawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
        //float v = rawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");
        Vector2 input = new Vector2(h, 0f);
        GameManager.Instance.GetPlayer(101).MoveInput(input);
        //player.MoveInput(input);


        //Look
        //Vector3 mousePos = Vector3.zero;
        //mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        //Vector3 lookPos = Camera.main.ScreenToWorldPoint(mousePos);
        //lookPos = lookPos - player.transform.position;
        //if(!Input.GetMouseButton(0))
        //    lookPos = lookPos.normalized / 2f;
        //player.LookInput(lookPos);


        //Shield
        //if (Input.GetKeyDown(KeyCode.Space))
        //    player.Shield(true);
        //else if (Input.GetKeyUp(KeyCode.Space))
        //    player.Shield(false);

    }

#endif
}
