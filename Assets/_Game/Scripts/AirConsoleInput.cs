using UnityEngine;
using System.Collections;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AirConsoleInput : MonoBehaviour
{


    private bool firstMasterMessage;
    
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
        GameManager.Instance.RemovePlayer(device_id);
    }

    private void AddNewPlayer(int deviceID)
    {
        //Is repeated
        if (GameManager.Instance.PlayerExists(deviceID))
            return;

        var player = GameManager.Instance.CreatePlayer(deviceID);
        
        //Can't have more players
        if (player == null)
        {
            AirConsole.instance.SetCustomDeviceStateProperty("playerColors", UpdatePlayerColorData(AirConsole.instance.GetCustomDeviceState(0), deviceID, "none"));
            return;
        }      

        //Send Color message to AirConsole
        StartCoroutine(SetViewDelayed("control", 1.5f));
        string colorString = "#" + ColorUtility.ToHtmlStringRGB(player.myColor); // "rgb("+color.r + "," + color.g + ", " + color.b  +")";
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

    void OnMessage(int device_id, JToken data)
    {
        //First message of master start game
        if(!firstMasterMessage)
        {
            int actualMaster = AirConsole.instance.GetMasterControllerDeviceId();
            if(device_id == actualMaster)
            {
                firstMasterMessage = true;
                GameManager.Instance.StartGame();
            }
        }
        
        //Check message
        if (GameManager.Instance.PlayerExists(device_id) && data["data"] != null)
        {
            //Debug.Log("message: " + data);

            string element = (string)data["element"];

            switch (element)
            {
                case "btn-left":
                    int left = (bool)data["data"]["pressed"] ? -1 : 0;
                    Vector2 vl = new Vector2(left, 0f);
                    GameManager.Instance.GetPlayer(device_id).MoveInput(vl);
                    break;
                case "btn-right":
                    int right = (bool)data["data"]["pressed"] ? 1 : 0;
                    Vector2 vr = new Vector2(right, 0f);
                    GameManager.Instance.GetPlayer(device_id).MoveInput(vr);
                    break;
            }
        }

    }


    public bool IsAirInitialized()
    {
        return AirConsole.instance != null;
    }
}