using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Initializing, LastManStanding, PlayersVsLastMan, PlayersVsLastBoss  }

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject basicAIControllerPrefab;
    [SerializeField] int numberOfAI = 7;
    [SerializeField] int maxNumberOfPlayers = 8;
    [SerializeField] Color[] colors;

    //Properties
    public static GameManager Instance { get; private set; } //Singleton
    public int MaxNumberOfPlayers { get { return maxNumberOfPlayers; } }
    
    public bool IsGameActive { get; private set; }

    public GameState State { get; private set; }

    //Events
    public event Action onGameStart;
    public event Action onGameEnd;

    //Data Structures
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private List<Color> availableColors;

    //Cache
    private DeadCircle circle;

    //Monobehaviour methods like Awake, Start, Update
    #region Monobehaviour

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        availableColors = new List<Color>(colors);
        State = GameState.Initializing;
    }

    private void Start()
    {
        circle = FindObjectOfType<DeadCircle>();

        for(int i = 0; i < numberOfAI; i++)
        {
            SpawnAI(basicAIControllerPrefab);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            ForceEnd();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            StartGame();
        }
    }
    #endregion

    public void StartGame()
    {
        HealAll();

        IsGameActive = true;
        onGameStart?.Invoke();
    }

    private void ForceEnd()
    {
        //Kill all players
        foreach (var item in players)
        {
            item.Value.Damage(1000);
        }

        IsGameActive = false;
    }

    private void HealAll()
    {
        //Kill all players
        foreach (var item in players)
        {
            item.Value.Heal(1000);
            if(!item.Value.gameObject.activeSelf)
                item.Value.gameObject.SetActive(true);
        }
    }

    public void PlayerKilled()
    {
        int alive = 0;
        foreach (var item in players)
        {
            if (item.Value.IsAlive)
                alive++;
        }

        if(alive == 1)
        {
            //Game Ends
            GameEnds();
        }
    }

    void GameEnds()
    {
        IsGameActive = false;
        onGameEnd?.Invoke();
    }



    private void SpawnAI(GameObject prefab)
    {
        Instantiate(prefab);
    }

    //Player related methods
    #region Players
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
    #endregion
}
