using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Engine.UI;
using UnityEngine.Rendering.PostProcessing;

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
    [SerializeField] UIView titleView;
    [SerializeField] UIView creditsView;
    [SerializeField] UIView messageView;
    [SerializeField] UIView flashView;
    [SerializeField] TextMeshProUGUI messageLabel;

    [Header("FinalBoss")]
    [SerializeField] PostProcessProfile normalProfile;
    [SerializeField] PostProcessProfile finalProfile;
    [SerializeField] GameObject finalBossPrefab;
    [SerializeField] PostProcessVolume postProcessVolume;
    [SerializeField] UIView gameOverView;
    [SerializeField] WordByWordCounter finalBossTitle;

    [Header("Dialogues")]
    [SerializeField] DialogueView bossDialogueBox;
    [SerializeField] WordByWordCounter hahaView;

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
    private FinalBoss finalBoss;


    //Modes
    private int lastManStandingCounts;
    private int playersVsBossPlayerCounts;
    private int finalBossCounts;

    private bool playerBossKilledWin;
    private bool finalBossKilledWin;


    private bool isTitleHidden;

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
        postProcessVolume.profile = normalProfile;
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
        if (!isTitleHidden)
        {
            isTitleHidden = true;
            titleView.Hide();
            yield return new WaitForSeconds(0.4f);
        }

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
            MusicController.Instance.PlayNormalMusic(0.1f);
            State = GameState.LastManStanding;     
            //Change View
            yield return StartCoroutine(lastManStandingCounter.StartCountRoutine(null));

            MusicController.Instance.PlayClip(0.6f, 4f);

        }
        else if (State.Equals(GameState.LastManStanding) && lastManStandingCounts >= 3)
        {
            State = GameState.PlayersVsPlayerBoss;

            //Change View
            yield return StartCoroutine(playerBossCounter.StartCountRoutine(null));

            //Flash
            flashView.Show(); //0.2f to show, and 0.2f to hide
            yield return new WaitForSeconds(0.2f); //Let flash finish on all screen

        }
        else if (State.Equals(GameState.PlayersVsPlayerBoss) && (playerBossKilledWin || playersVsBossPlayerCounts >= 1))
        {
            State = GameState.PlayersVsLastBoss;

            if (bossPlayer != null)
            {
                Destroy(bossPlayer.gameObject);
                bossPlayer = null;
                players[firstBloodedPlayer.DeviceId] = firstBloodedPlayer;
                firstBloodedPlayer.gameObject.SetActive(true);
            }
            RepositionPlayers();

            MusicController.Instance.StopClip(3f);

            //Introduction dialogue
            yield return StartCoroutine(bossDialogueBox.StartRoutine("Are you having fun...", null, 0.5f));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("... Player?", null));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("LET", null, 0.5f));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("ME", null, 0.5f));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("JOIN", null, 0.5f));

            yield return new WaitForSeconds(0.5f);

            //Flash
            flashView.Show(); //0.2f to show, and 0.2f to hide
            yield return new WaitForSeconds(0.2f); //Let flash finish on all screen            
            postProcessVolume.profile = finalProfile;
        }
        else if (State.Equals(GameState.PlayersVsLastBoss) && (finalBossKilledWin || finalBossCounts >= 3))
        {
            MusicController.Instance.StopClip(0.5f);
            //Boss dialogues
            yield return StartCoroutine(bossDialogueBox.StartRoutine("Oh no. Did you really think you could WIN?", null));
            yield return StartCoroutine(hahaView.StartCountRoutine(null)); //HA HA

            finalBoss.Heal(15000);
            yield return StartCoroutine(bossDialogueBox.StartRoutine("It's my world...", null, 0.5f));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("... Player", null, 1f));
            yield return StartCoroutine(bossDialogueBox.StartRoutine("And, in my world, I do what I wish", null));

            //Kill all players
            yield return new WaitForSeconds(1f);
            foreach (var item in players)
            {
                item.Value.HardKill();
            }
            yield return new WaitForSeconds(1f);

            //HA HA HA HA HA HA
            yield return StartCoroutine(bossDialogueBox.StartRoutine("HA HA HA HA HA HA HA...", null, 0.1f));

            //Obscure screen. Ready for credits
            gameOverView.Show();
         
            State = GameState.Credits;

            postProcessVolume.profile = normalProfile;
        }

        bool willGameActivate = true;
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
           
            //Create Final Boss
            if(finalBoss == null)
            {
                GameObject finalBossGo = Instantiate(finalBossPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                finalBoss = finalBossGo.GetComponent<FinalBoss>();

                //Its show time
                yield return StartCoroutine(finalBossTitle.StartCountRoutine(null));
                MusicController.Instance.PlayBossMusic(0.75f);
            }

            finalBoss.Heal(15000);

            finalBossCounts++;
        }
        else if (State.Equals(GameState.Credits)) //Credits
        {
            //Delete boss
            if (finalBoss != null)
            {
                Destroy(finalBoss.gameObject);
                //Its show time
            }

            State = GameState.Initializing;
            lastManStandingCounts = 0;
            playersVsBossPlayerCounts = 0;
            finalBossCounts = 0;
            firstBloodedPlayer = null;

            yield return new WaitForSeconds(5f);
            gameOverView.Hide();

            creditsView.Show();//Show credits           
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(10f);//Wait
            creditsView.Hide(); //Hide
            willGameActivate = false;
            Invoke("StartGame", 1f);
        }

        if(willGameActivate)
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
        
        if (State.Equals(GameState.LastManStanding) && alive == 1)
        {
            //Game Ends
            GameEnds(lastAlive);
            return;
        }
        else if(State.Equals(GameState.PlayersVsPlayerBoss) && alive == 1)
        {
            GameEnds(null);
        }
        else if (State.Equals(GameState.PlayersVsLastBoss) && alive == 0)
        {
            GameEnds(null);
        }

        //Cache first dead
        if (firstBloodedPlayer == null)
        {
            foreach (var item in players)
            {
                if (!item.Value.IsAlive)
                {
                    firstBloodedPlayer = item.Value;
                }
            }
        }

        
        if (State.Equals(GameState.LastManStanding) && alive < 5 && lastManRingCanons == null)
        {
            lastManRingCanons = Instantiate(ringCanonMasterPrefab, Vector3.zero, Quaternion.identity).GetComponent<RingCanonMaster>();
            lastManRingCanons.CreateCanons();
        }
        
    }

    
    public void PlayerBossKilled()
    {
        playerBossKilledWin = true;
        GameEnds(bossPlayer);
    }

    public void FinalBossKilled()
    {
        finalBossKilledWin = true;
        GameEnds(null);
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


        if (!State.Equals(GameState.PlayersVsLastBoss))
        {
            messageView.Show();
            IsGameActive = false;
            onGameEnd?.Invoke();

            //TODO: Remove
            messageView.Hide(5f);
            Invoke("StartGame", 5f);
        }
        else
        {
            IsGameActive = false;
            Invoke("StartGame", 0.5f);
        }

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
