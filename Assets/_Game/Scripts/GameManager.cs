using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Engine.UI;

public enum GameState { Initializing, LastManStanding, PlayersVsLastMan, PlayersVsLastBoss  }

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject basicAIControllerPrefab;
    [SerializeField] int maxNumberOfPlayers = 8;
    [SerializeField] Color[] colors;

    [Header("Other")]
    [SerializeField] GameObject ringCanonMasterPrefab;
    [SerializeField] UIView messageView;
    [SerializeField] UIView flashView;
    [SerializeField] TextMeshProUGUI messageLabel;

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
    private GameRing ring;
    private Counter counter;

    private RingCanonMaster lastManRingCanons;

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
        ring = FindObjectOfType<GameRing>();
        counter = FindObjectOfType<Counter>();  
    }

    private void SpawnAIs()
    {
        int numberOfAI = MaxNumberOfPlayers - players.Count;
        for (int i = 0; i < numberOfAI; i++)
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

        if(Input.GetKeyDown(KeyCode.E))
        {
            if (lastManRingCanons != null)
            {
                Destroy(lastManRingCanons.gameObject);
                lastManRingCanons = null;
            }

            lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
            lastManRingCanons.CreateCanons();
        }

        //Create rings by time
        if(lastManRingCanons == null && ring.Radius < ring.MinRadiusSize + 0.1f)
        {
            lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
            lastManRingCanons.CreateCanons();
        }
    }
    #endregion

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());   
    }

    IEnumerator StartGameRoutine()
    {
        //Flash
        flashView.Show(); //0.2f to show, and 0.2f to hide

        yield return new WaitForSeconds(0.2f); //Let flash finish on all screen

        SpawnAIs(); //SpawnAIs if needed

        //Clean
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        for(int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i].gameObject.activeInHierarchy)
                ObjectPool.Instance.SaveObjectToPool(bullets[i].gameObject);
        }

        ring.Restart();
        if (lastManRingCanons != null)
        {
            Destroy(lastManRingCanons.gameObject);
            lastManRingCanons = null;
        }

        HealAll();
        RepositionPlayers();

        counter.StartCount(ActivateGame);
    }

    private void ActivateGame()
    {
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
            item.Value.Restart();
            if(!item.Value.gameObject.activeSelf)
                item.Value.gameObject.SetActive(true);
        }
    }

    public void PlayerKilled()
    {
        int alive = 0;
        PlayerController lastAlive = null;
        foreach (var item in players)
        {
            if (item.Value.IsAlive)
            {
                lastAlive = item.Value;
                alive++;
            }
        }    

        if (alive == 1)
        {
            //Game Ends
            GameEnds(lastAlive);
            return;
        }

        if (alive < 5 && lastManRingCanons == null && ring.Radius < 14.0f)
        {
            lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
            lastManRingCanons.CreateCanons();
        }


    }

    void GameEnds(PlayerController player)
    {
        string hex = player != null ? ColorUtility.ToHtmlStringRGB(player.myColor) : "000000";
        messageLabel.text = "Player <color=#"+hex+">#" + hex + "</color>\nWins!";
        messageView.Show();
        IsGameActive = false;
        onGameEnd?.Invoke();

        //TODO: Remove
        messageView.Hide(3f);
        Invoke("StartGame", 3f);
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
        if (ring == null) ring = FindObjectOfType<GameRing>();
        float angle = players.Count * Mathf.PI * 2 / maxNumberOfPlayers;
        Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ring.Radius;

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

    private void RepositionPlayers()
    {
        if (ring == null) ring = FindObjectOfType<GameRing>();

        int count = 0;
        foreach (var item in players)
        {
            float angle = count * Mathf.PI * 2 / maxNumberOfPlayers;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ring.Radius;
            item.Value.transform.position = pos;
            item.Value.ForceCenterLook();
            count++;
        }
        
    }
    #endregion
}
