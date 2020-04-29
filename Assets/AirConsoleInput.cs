using UnityEngine;
using System.Collections;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AirConsoleInput : MonoBehaviour
{

    public GameObject playerPrefab;
    public Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

    public UnityEngine.UI.Text debugText;

    void Awake()
    {
        if (AirConsole.instance == null)
        {
            this.enabled = false;
            return;
        }
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
        AirConsole.instance.onReady += OnReady;
        //AirConsole.instance.onDisconnect += OnDisconnect;
    }

    void OnReady(string code)
    {
        //Initialize Game State
        //JObject newGameState = new JObject();
        //newGameState.Add("view", new JObject());
        //newGameState.Add("playerColors", new JObject());

        //AirConsole.instance.SetCustomDeviceState(newGameState);


        //Since people might be coming to the game from the AirConsole store once the game is live, 
        //I have to check for already connected devices here and cannot rely only on the OnConnect event 
        List<int> connectedDevices = AirConsole.instance.GetControllerDeviceIds();
        foreach (int deviceID in connectedDevices)
        {
            AddNewPlayer(deviceID);
        }
    }

    void OnConnect(int device_id)
    {
        AddNewPlayer(device_id);

        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {

            if (AirConsole.instance.GetControllerDeviceIds().Count >= 2)
            {
                //StartGame();
                //GameManager.Instance.StartGame(0);
            }
            else
            {
                //uiText.text = "NEED MORE PLAYERS";
            }
        }
    }

    private void AddNewPlayer(int deviceID)
    {
        if (players.ContainsKey(deviceID))
        {
            return;
        }
        if (players.Count >= 4)
        {
            //AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, "none"));
            return;
        }

        //Color
        //PlayerColor color = availableColors[0];
        //availableColors.RemoveAt(0);
        //usedColors.Add(color);

        //Instantiate player prefab, store device id + player script in a dictionary
        Vector3 pos = transform.position;
        pos.x = Random.Range(-7f, 7f);
        pos.y = Random.Range(-4f, 4f);
        //pos.x = deviceID -2f;

        //pos.x = pos.x + ((int)color * 2);

        GameObject newPlayer = Instantiate(playerPrefab, pos, transform.rotation) as GameObject;
        PlayerController playerController = newPlayer.GetComponent<PlayerController>();
        players.Add(deviceID, playerController);


        newPlayer.transform.position = pos;



        //playerController.color = color;

        //players[deviceID].SetMaterial(playersMats[(int)color]);
        //AirConsole.instance.Message(deviceID, color.ToString().ToLower());
        //StartCoroutine(SetViewDelayed("control", 1.5f));
        //AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, color.ToString().ToLower()));
    }

    void OnMessage(int device_id, JToken data)
    {

        debugText.text = data.ToString();
        if (players.ContainsKey(device_id) && data != null)
        {
            if (data["joystick-left"] != null)
            {
                Vector2 movement = Vector2.zero;

                if(data["joystick-left"]["pressed"].Value<bool>())
                {
                    
                    movement = new Vector2(data["joystick-left"]["message"]["x"].Value<float>(), data["joystick-left"]["message"]["y"].Value<float>() * -1f);
                }
                else
                {
                    //Zero
                }
                players[device_id].MoveInput(movement);
            }

            
            if (data["look"] != null)
            {
                Vector2 lookDirection = Vector2.zero;
                if (data["look"]["pressed"].Value<bool>())
                {
                    lookDirection = new Vector2(data["look"]["message"]["x"].Value<float>(), data["look"]["message"]["y"].Value<float>() * -1f);
                    
                }
                else
                {

                    //Stay as it was.
                }
                players[device_id].LookInput(lookDirection);
            }

            if (data["shield"] != null)
            {
                bool active = data["shield"]["pressed"].Value<bool>();
                players[device_id].Shield(active);
            }

            

            //Debug.Log("message: " + data);

            //string element = (string)data["element"];

            //switch (element)
            //{
            //    case "btn-interact":
            //        players[device_id].InteractInput((bool)data["data"]["pressed"]);
            //        if (!firstMessage)
            //        {
            //            GameManager.Instance.StartGame(0);
            //            firstMessage = true;
            //        }
            //        break;
            //    case "main-arrows":
            //        players[device_id].MoveInput((string)data["data"]["key"], (bool)data["data"]["pressed"]);
            //        if (!firstMessage)
            //        {
            //            GameManager.Instance.StartGame(0);
            //            firstMessage = true;
            //        }
            //        break;
            //}

            //I forward the command to the relevant player script, assigned by device ID
        }
    }

}