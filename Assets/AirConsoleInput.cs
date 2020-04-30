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
        AirConsole.instance.onDisconnect += OnDisconnect;
    }

    void OnReady(string code)
    {
        //Initialize Game State
        JObject newGameState = new JObject();
        newGameState.Add("view", new JObject());
        newGameState.Add("playerColors", new JObject());

        AirConsole.instance.SetCustomDeviceState(newGameState);


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
    }

    void OnDisconnect(int device_id)
    {
        RemovePlayer(device_id);
    }

    private void AddNewPlayer(int deviceID)
    {
        if (players.ContainsKey(deviceID))
        {
            return;
        }
        if (players.Count >= 9) //Max numberof players
        {
            AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, "none"));
            return;
        }

        //Color
        Color color = GameManager.Instance.GetNextColor();

        if(color.a < 0.5f)
        {
            AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, "none"));
            return;
        }

        //Get an equidistance point on circle
        DeadCircle circle = FindObjectOfType<DeadCircle>();
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8;
            pos = new Vector3(Mathf.Cos(angle) * circle.Radius, Mathf.Sin(angle) * circle.Radius, 0f);
        }

        //Instantiate player prefab, store device id + player script in a dictionary
        GameObject newPlayer = Instantiate(playerPrefab, pos, transform.rotation) as GameObject;
        PlayerController playerController = newPlayer.GetComponent<PlayerController>();
        players.Add(deviceID, playerController);

        //Set position and color
        newPlayer.transform.position = pos;
        playerController.SetColor(color);

        //Send Color message to AirConsole
        AirConsole.instance.Message(deviceID, color.ToString().ToLower());
        StartCoroutine(SetViewDelayed("control", 1.5f));
        string colorString = "#" + ColorUtility.ToHtmlStringRGB(color); // "rgb("+color.r + "," + color.g + ", " + color.b  +")";
        AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, colorString));
    }

    IEnumerator SetViewDelayed(string view, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetView(view);
    }

    public static JToken UpdatePlayerColorData(JToken oldGameState, int deviceId, string colorName)
    {

        //take out the existing playerColorData and store it as a JObject so I can modify it
        JObject playerColorData = oldGameState["playerColors"] as JObject;

        //check if the playerColorData object within the game state already has data for this device
        if (playerColorData.HasValues && playerColorData[deviceId.ToString()] != null)
        {
            //there is already color data for this device, replace it
            playerColorData[deviceId.ToString()] = colorName;
        }
        else
        {
            playerColorData.Add(deviceId.ToString(), colorName);
            //there is no color data for this device yet, create it new
        }

        //logging and returning the updated playerColorData
        //Debug.Log("AssignPlayerColor for device " + deviceId + " returning new playerColorData: " + playerColorData);

        return playerColorData;
    }

    public void SetView(string viewName)
    {
        //I don't need to replace the entire game state, I can just set the view property
        AirConsole.instance.SetCustomDeviceStateProperty("view", viewName);

        //the controller listens for the onCustomDeviceStateChanged event. See the  controller-gamestates.html file for how this is handled there. 
    }

    private void RemovePlayer(int deviceID)
    {
        if (!players.ContainsKey(deviceID))
        {
            return;
        }

        //Get the player 
        PlayerController player = players[deviceID];

        //Return color to the pool
        Color color = player.myColor;
        GameManager.Instance.ReturnColorToList(color);

        //Destroy that player objects
        player.RemoveFromGame();
        Destroy(player.gameObject);
        players.Remove(deviceID);
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


    public bool IsAirInitialized()
    {
        return AirConsole.instance != null;
    }
}