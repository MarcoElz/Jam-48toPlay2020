using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    [SerializeField] int maxNumberOfPlayers = 8;
    [SerializeField] GameObject playerPrefab;

    [SerializeField] Color[] colors;

    public int MaxNumberOfPlayers { get { return maxNumberOfPlayers; } }

    private List<Color> availableColors;
    private DeadCircle circle;


    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        availableColors = new List<Color>(colors);
    }

    private void Start()
    {
        circle = FindObjectOfType<DeadCircle>();
    }

    public PlayerController CreatePlayer(int deviceID)
    {
        //Too many players
        if (players.Count > MaxNumberOfPlayers)
            return null;

        //Get an equidistance point on circle      
        if (circle == null) circle = FindObjectOfType<DeadCircle>();
        float angle = players.Count * Mathf.PI * 2 / maxNumberOfPlayers;
        Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * circle.Radius;

        //Instantiate player prefab, store device id + player script in a dictionary
        GameObject newPlayer = Instantiate(playerPrefab, pos, transform.rotation) as GameObject;
        PlayerController playerController = newPlayer.GetComponent<PlayerController>();
        players.Add(deviceID, playerController);

        Color color = GetNextColor();

        if (color.a < 0.5f)
            return null;

        //Set position and color
        newPlayer.transform.position = pos;
        playerController.SetColor(color);

        return playerController;
    }

    public bool PlayerExists(int deviceID)
    {
        return players.ContainsKey(deviceID);
    }

    public PlayerController GetPlayer(int deviceID)
    {
        return players[deviceID];
    }

    public void RemovePlayer(int deviceID)
    {
        //Player does not exist
        if (PlayerExists(deviceID))
            return;

        //Get the player 
        PlayerController player = players[deviceID];

        //Return color to the pool
        Color color = player.myColor;
        ReturnColorToList(color);

        //Destroy that player objects
        player.RemoveFromGame();
        Destroy(player.gameObject);
        players.Remove(deviceID);
    }

    public Color GetNextColor()
    {
        if(availableColors.Count <= 1)
        {
            return new Color(0f, 0f, 0f, 0f);
        }

        //Get the first color of the list
        Color color = availableColors[0];
        availableColors.RemoveAt(0);

        return color;
    }

    public void ReturnColorToList(Color color)
    {
        //Insert it to the top of the list
        availableColors.Insert(0, color); 
    }
}
