using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Engine.UI;

public enum GameState { Initializing, LastManStanding, PlayersVsPlayerBoss, PlayersVsLastBoss, Credits  }

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject basicAIControllerPrefab;
    [SerializeField] int maxNumberOfPlayers = 8;
    [SerializeField] Color[] colors;

    [Header("UI")]
    [SerializeField] Counter mainCounter;
    [SerializeField] WordByWordCounter lastManStandingCounter;
    [SerializeField] WordByWordCounter playerBossCounter;

    [Header("Other")]
    [SerializeField] GameObject playerBossPrefab;
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

    private RingCanonMaster lastManRingCanons;

    private PlayerController firstBloodedPlayer;
    private BossPlayer bossPlayer;


    //Modes
    private int lastManStandingCounts;
    private int playersVsBossPlayerCounts;
    private int finalBossCounts;

    private bool playerBossKilledWin;
    private bool finalBossKilledWin;

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
        //mainCounter = FindObjectOfType<Counter>();  
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
        if(State.Equals(GameState.LastManStanding))
        {
            if (lastManRingCanons == null && ring.Radius < ring.MinRadiusSize + 0.1f)
            {
                lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
                lastManRingCanons.CreateCanons();
            }
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


        //Change Mode
        if (State.Equals(GameState.Initializing))
        {
            State = GameState.LastManStanding;

            //Change View
            yield return StartCoroutine(lastManStandingCounter.StartCountRoutine(null));

        }
        else if (State.Equals(GameState.LastManStanding) && lastManStandingCounts >= 1)
        {
            State = GameState.PlayersVsPlayerBoss;

            //Change View
            yield return StartCoroutine(playerBossCounter.StartCountRoutine(null));

            //Flash
            flashView.Show(); //0.2f to show, and 0.2f to hide
            yield return new WaitForSeconds(0.2f); //Let flash finish on all screen

        }
        else if (State.Equals(GameState.PlayersVsPlayerBoss) && (playerBossKilledWin || playersVsBossPlayerCounts >= 2))
        {
            State = GameState.PlayersVsLastBoss;
        }
        else if (State.Equals(GameState.PlayersVsLastBoss) && (finalBossKilledWin || finalBossCounts >= 3))
        {
            State = GameState.Credits;
        }


        if (State.Equals(GameState.LastManStanding))
        {
            lastManStandingCounts++;
        }
        else if (State.Equals(GameState.PlayersVsPlayerBoss))
        {
            if (bossPlayer == null)
            {
                //Instantiate player prefab, store device id + player script in a dictionary
                GameObject bossPlayerGo = Instantiate(playerBossPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, -90f)) as GameObject;
                bossPlayer = bossPlayerGo.GetComponent<BossPlayer>();

                //Replace
                players[firstBloodedPlayer.DeviceId] = bossPlayer;

                //Set position and color
                bossPlayer.SetColor(firstBloodedPlayer.myColor);
                bossPlayer.SetDeviceId(firstBloodedPlayer.DeviceId);

                //Unactive old
                firstBloodedPlayer.gameObject.SetActive(false);
            }

            bossPlayer.ForceCenterLook();
            bossPlayer.transform.position = Vector3.zero;
            playersVsBossPlayerCounts++;
        }
        else if (State.Equals(GameState.PlayersVsLastBoss))
        {
            if(bossPlayer != null)
            {
                Destroy(bossPlayer.gameObject);
                bossPlayer = null;
                players[firstBloodedPlayer.DeviceId] = firstBloodedPlayer;
                firstBloodedPlayer.gameObject.SetActive(true);
            }
           
            RepositionPlayers();

            //Create Final Boss

            finalBossCounts++;
        }
        else if (State.Equals(GameState.Credits)) //Credits
        {
            State = GameState.PlayersVsPlayerBoss;
            //Show credits
            //Wait
            //View.Show & Hide(5f);
            Invoke("StartGame", 1f);
        }

        mainCounter.StartCount(ActivateGame);
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

        //Cache first dead
        if(firstBloodedPlayer == null)
        {
            foreach (var item in players)
            {
                if (!item.Value.IsAlive)
                {
                    firstBloodedPlayer = item.Value;
                }
            }
        }

        //if (State.Equals(GameState.LastManStanding))
        //{
        //    if (alive < 5 && lastManRingCanons == null && ring.Radius < 14.0f)
        //    {
        //        lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
        //        lastManRingCanons.CreateCanons();
        //    }
        //}
    }

    
    public void PlayerBossKilled()
    {
        playerBossKilledWin = true;
        GameEnds(bossPlayer);
    }

    void GameEnds(PlayerController player)
    {

        if(State.Equals(GameState.LastManStanding))
        {
            string hex = player != null ? ColorUtility.ToHtmlStringRGB(player.myColor) : "000000";
            messageLabel.text = "Player <color=#" + hex + ">#" + hex + "</color>\nWins!";
        }
        else if(State.Equals(GameState.PlayersVsPlayerBoss))
        {
            if(playerBossKilledWin)
            {
                messageLabel.text = "Players win";
            }
            else
            {
                messageLabel.text = "REVENGE WINS";
            }
        }
        else if (State.Equals(GameState.PlayersVsLastBoss))
        {

        }


        messageView.Show();
        IsGameActive = false;
        onGameEnd?.Invoke();

        //TODO: Remove
        messageView.Hide(5f);
        Invoke("StartGame", 5f);


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
        playerController.SetDeviceId(deviceID);

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
        //player.RemoveFromGame();
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
