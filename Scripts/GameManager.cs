using System.Collections;
using System.Collections.Generic;
using SgLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

[System.Serializable]
struct Environment
{
    public Texture pathTexture;
    public List<GameObject> rocks;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    public static int GameCount
    { 
        get { return _gameCount; } 
        private set { _gameCount = value; } 
    }

    private static int _gameCount = 0;

    [Header("Set the target frame rate for this game")]
    [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
    public int targetFrameRate = 30;

    // List of public variable for gameplay tweaking
    [Header("Gameplay Config")]

    [Range(0f, 1f)]
    [SerializeField]
    private float coinFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float stairFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float hiddenGroundFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float trapFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float obstacleFrequency = 0.1f;
    [SerializeField]
    private int minGroundNumber = 4;
    [SerializeField]
    private int maxGroundNumber = 7;
    [SerializeField]
    private int initialGround = 15;
    [SerializeField]
    private int scoreToCreateNewTrap = 10;
    public float bulletSpeed = 10f;
    public float maxDistance = 10f;
    //The maximum distance between the player and the monster


    // List of public variables referencing other objects
    [Header("Object References")]
    public PlayerController playerController;
    public GameObject firstGround;
    public GameObject groundPrefab;
    public GameObject lastGroundPrefab;
    public GameObject stairPrefab;
    public GameObject speedDownTrapPrefab;
    public GameObject thornTrapPrefab;
    public GameObject sawTrapPrefab;
    public GameObject fireTrapPrefab;
    public GameObject iceTrapPrefab;
    public GameObject torusTrapPrefab;
    public GameObject bulletPrefab;
    public GameObject coinPrefab;
    public GameObject startPoint;
    public GameObject fireBall;
    public GameObject freezingParticle;
    public GameObject firedParticle;
    public List<GameObject> listObstaclePrefab = new List<GameObject>();
    public List<GameObject> listRockPrefab = new List<GameObject>();

    [HideInInspector]
    public Vector3 groundSize { private set; get; }

    [HideInInspector]
    public Vector3 lastGroundSize { private set; get; }

    public Vector3 sawTrapSize { private set; get; }

    private List<GameObject> listGround = new List<GameObject>();
    private List<GameObject> listLastGround = new List<GameObject>();
    private List<GameObject> listStair = new List<GameObject>();
    private List<GameObject> listSpeedDownTrap = new List<GameObject>();
    private List<GameObject> listThornTrap = new List<GameObject>();
    private List<GameObject> listSawTrap = new List<GameObject>();
    private List<GameObject> listFireTrap = new List<GameObject>();
    private List<GameObject> listIceTrap = new List<GameObject>();
    private List<GameObject> listTorusTrap = new List<GameObject>();
    private List<GameObject> listBullet = new List<GameObject>();
    private List<GameObject> listObstacle = new List<GameObject>();
    private List<GameObject> listRock = new List<GameObject>();
    private List<GameObject> listCoin = new List<GameObject>();

    private Vector3 generateDirection;
    private Vector3 nextPos;

    private int groundNumberOnPath;
    private int groundCount = 0;
    private int trapCount = 0;

    private int groundNumber;
    private int lastGroundNumber;
    private int stairNumber;
    private int trapNumber;
    private int rockPackNumber;

    private bool isCreatedSawTrap = false;

    void OnEnable()
    {
        PlayerController.PlayerDied += PlayerController_PlayerDied;
    }

    void OnDisable()
    {
        PlayerController.PlayerDied -= PlayerController_PlayerDied;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Use this for initialization
    void Start()
    {
        // Initial setup
        Application.targetFrameRate = targetFrameRate;
        ScoreManager.Instance.Reset();


        groundNumber = initialGround * 2;
        stairNumber = initialGround;
        lastGroundNumber = initialGround;
        trapNumber = initialGround;
        rockPackNumber = initialGround;

        lastGroundSize = lastGroundPrefab.GetComponent<Renderer>().bounds.size;
        groundSize = groundPrefab.GetComponent<Renderer>().bounds.size;
        sawTrapSize = sawTrapPrefab.GetComponent<Renderer>().bounds.size;
        generateDirection = Vector3.forward;

        nextPos = firstGround.transform.position + generateDirection * groundSize.x;
        groundNumberOnPath = Random.Range(minGroundNumber, maxGroundNumber + 1);
        groundCount = 0;
        PrepareGame();
        InitialObjects();
        StartCoroutine(CheckAndEnableFireball());

        //Create some first ground (only ground)
        for (int i = 0; i < 5; i++)
        {
            CreateGround();
        }

        //Create path (ground, last ground, stair, trap....)
        for (int i = 0; i < initialGround; i++)
        {
            CreatePath();
        }
    }

    // Listens to the event when player dies and call GameOver
    void PlayerController_PlayerDied()
    {
        GameOver();
    }

    // Make initial setup and preparations before the game can be played
    public void PrepareGame()
    {
        GameState = GameState.Prepare;
    }

    // A new game official starts
    public void StartGame()
    {
        GameState = GameState.Playing;
        if (SoundManager.Instance.background != null)
        {
            SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
        }
        StartCoroutine(WaitAndHideStartPoint());
    }

    // Called when the player died
    public void GameOver()
    {
        if (SoundManager.Instance.background != null)
        {
            SoundManager.Instance.StopMusic();
        }

        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
        GameState = GameState.GameOver;
        GameCount++;

        // Add other game over actions here if necessary
    }

    // Start a new game
    public void RestartGame(float delay = 0)
    {
        StartCoroutine(CRRestartGame(delay));
    }

    IEnumerator CRRestartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Initial objects for pooling
    void InitialObjects()
    {
        //Initial grounds
        for (int i = 0; i < groundNumber; i++)
        {
            GameObject ground = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity);
            ground.transform.SetParent(transform);
            listGround.Add(ground);
            ground.SetActive(false);
        }
        //Initial last grounds
        for (int i = 0; i < lastGroundNumber; i++)
        {
            GameObject ground = Instantiate(lastGroundPrefab, Vector3.zero, Quaternion.identity);
            ground.transform.SetParent(transform);
            listLastGround.Add(ground);
            ground.SetActive(false);
        }

        //Initial stairs
        for (int i = 0; i < stairNumber; i++)
        {
            GameObject stair = Instantiate(stairPrefab, Vector3.zero, Quaternion.identity);
            stair.transform.SetParent(transform);
            listStair.Add(stair);
            stair.SetActive(false);
        }

        //Initial rocks
        int rockPackIndex = -1;
        for (int i = 0; i < rockPackNumber; i++)
        {
            rockPackIndex = (rockPackIndex + 1 > listRockPrefab.Count - 1) ? (0) : (rockPackIndex + 1);
            GameObject rock = Instantiate(listRockPrefab[rockPackIndex], Vector3.zero, Quaternion.identity);
            rock.transform.SetParent(transform);
            listRock.Add(rock);
            rock.SetActive(false);
        }


        //Initial obstacles
        int obstacleIndex = -1;
        for (int i = 0; i < initialGround; i++)
        {
            obstacleIndex = (obstacleIndex + 1 > listObstaclePrefab.Count - 1) ? (0) : (obstacleIndex + 1);
            GameObject obstacle = Instantiate(listObstaclePrefab[obstacleIndex], Vector3.zero, Quaternion.identity);
            obstacle.transform.SetParent(transform);
            listObstacle.Add(obstacle);
            obstacle.SetActive(false);
        }

        //Initial coins
        for (int i = 0; i < groundNumber; i++)
        {
            GameObject coin = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity);
            coin.transform.SetParent(transform);
            listCoin.Add(coin);
            coin.SetActive(false);
        }

        //Initial traps
        for (int i = 0; i < trapNumber; i++)
        {
            //Speed down trap
            GameObject sdTrap = Instantiate(speedDownTrapPrefab, Vector3.zero, Quaternion.identity);
            sdTrap.transform.SetParent(transform);
            listSpeedDownTrap.Add(sdTrap);
            sdTrap.SetActive(false);

            //Thorn trap
            GameObject thornTrap = Instantiate(thornTrapPrefab, Vector3.zero, Quaternion.identity);
            thornTrap.transform.SetParent(transform);
            listThornTrap.Add(thornTrap);
            thornTrap.SetActive(false);

            //Saw trap
            GameObject sawTrap = Instantiate(sawTrapPrefab, Vector3.zero, Quaternion.identity);
            sawTrap.transform.SetParent(transform);
            listSawTrap.Add(sawTrap);
            sawTrap.SetActive(false);

            //Fire trap
            GameObject fireTrap = Instantiate(fireTrapPrefab, Vector3.zero, Quaternion.identity);
            fireTrap.transform.SetParent(transform);
            listFireTrap.Add(fireTrap);
            fireTrap.SetActive(false);

            //Ice trap
            GameObject iceTrap = Instantiate(iceTrapPrefab, Vector3.zero, Quaternion.identity);
            iceTrap.transform.SetParent(transform);
            listIceTrap.Add(iceTrap);
            iceTrap.SetActive(false);

            //Torus trap
            GameObject torusTrap = Instantiate(torusTrapPrefab, Vector3.zero, Quaternion.identity);
            torusTrap.transform.SetParent(transform);
            listTorusTrap.Add(torusTrap);
            torusTrap.SetActive(false);

            //Bullet 
            GameObject bullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
            bullet.transform.SetParent(transform);
            listBullet.Add(bullet);
            bullet.SetActive(false);
        }
    }

    /// <summary>
    /// Get the bullet from the list
    /// </summary>
    /// <returns>The bullet object</returns>
    public GameObject GetBullet()
    {
        foreach (GameObject o in listBullet)
        {
            if (!o.activeInHierarchy)
            {
                o.transform.SetParent(null);
                o.SetActive(true);
                return o;
            }
        }
        return null;
    }

    GameObject GetObject(List<GameObject> listObject)
    {
        foreach (GameObject o in listObject)
        {
            if (!o.activeInHierarchy)
            {
                o.transform.SetParent(null);
                o.SetActive(true);
                return o;
            }
        }
        return null;
    }

    void RandomRotationObject(GameObject currentObject)
    {
        float y = (Random.Range(0, 361) / 90) * 90;
        currentObject.transform.eulerAngles = new Vector3(0, y, 0);
    }

    //Create only ground
    void CreateGround()
    {
        GameObject thisGround = GetObject(listGround);
        thisGround.transform.position = nextPos;
        RandomRotationObject(thisGround);
        nextPos = thisGround.transform.position + generateDirection * groundSize.x;
    }
    //Create path (ground, last ground , stair, traps...)
    public void CreatePath()
    {
        if (groundCount == groundNumberOnPath) //Create last ground
        {
            GameObject thisGround = GetObject(listLastGround);
            thisGround.transform.position = nextPos;
            RandomRotationObject(thisGround);

            GameObject rock = GetObject(listRock);
            rock.transform.position = thisGround.transform.position;

            if (Random.value < trapFrequency && ScoreManager.Instance.Score / scoreToCreateNewTrap >= 5)//Create torus trap
            {
                Vector3 rayPos = thisGround.transform.position + Vector3.up * (groundSize.y + 0.5f);
                RaycastHit hit;
                Ray ray = new Ray(rayPos, -generateDirection);

                if (!Physics.Raycast(ray, out hit, 20f))
                {
                    GameObject torusTrap = GetObject(listTorusTrap);
                    torusTrap.transform.position = thisGround.transform.position + generateDirection * lastGroundSize.x;
                    torusTrap.transform.position += new Vector3(0, lastGroundSize.y, 0);
                    if (generateDirection == Vector3.right)
                        torusTrap.transform.eulerAngles = new Vector3(0, 90, 0);
                    torusTrap.SetActive(true);
                }
                else
                    CreateCoin(thisGround);
            }

            generateDirection = (generateDirection == Vector3.forward) ? (Vector3.right) : (Vector3.forward);
            groundCount = 0;
            trapCount = 0;
            isCreatedSawTrap = false;
            groundNumberOnPath = Random.Range(minGroundNumber, maxGroundNumber + 1);
            nextPos = thisGround.transform.position + generateDirection * lastGroundSize.x;
        }
        else //Create ground and traps
        {
            groundCount++;
            GameObject thisObject;

            if (Random.value <= trapFrequency && groundCount > 1 && trapCount < 2 && groundCount < groundNumberOnPath) //Create trap
            {
                trapCount++;
                thisObject = PickATrap();
                thisObject.transform.position = nextPos;
                if (thisObject.CompareTag("SawTrap"))
                {
                    if (generateDirection == Vector3.forward)
                    {
                        isCreatedSawTrap = true;
                        thisObject.transform.eulerAngles = new Vector3(0, 270, 0);
                        thisObject.transform.position += new Vector3(0, 0, 1);
                    }
                    else if (!isCreatedSawTrap)
                        thisObject.transform.position += new Vector3(1, 0, 0);

                    nextPos = thisObject.transform.position + generateDirection * (sawTrapSize.x - 1);
                }
                else
                {
                    isCreatedSawTrap = false;
                    nextPos = thisObject.transform.position + generateDirection * groundSize.x;
                }
            }
            else //Create ground or stair
            {
                isCreatedSawTrap = false;
                if (Random.value < 0.5f && Random.value <= stairFrequency) //Create stair
                {
                    thisObject = GetObject(listStair);
                    thisObject.transform.position = nextPos;
                    if (generateDirection == Vector3.forward)
                        thisObject.transform.eulerAngles = new Vector3(0, 270, 0);
                    nextPos = thisObject.transform.position + generateDirection * groundSize.x + new Vector3(0, -1, 0);
                    trapCount = 0;
                }
                else //Create ground
                {
                    thisObject = GetObject(listGround);
                    thisObject.transform.position = nextPos;
                    RandomRotationObject(thisObject);
                    nextPos = thisObject.transform.position + generateDirection * groundSize.x;

                    if (Random.value <= hiddenGroundFrequency && trapCount < 2 && groundCount > 1 && groundCount < groundNumberOnPath)
                    {
                        trapCount++;
                        thisObject.SetActive(false);
                        thisObject.transform.position = Vector3.zero;
                        thisObject.transform.eulerAngles = Vector3.zero;
                        thisObject.transform.SetParent(transform);
                        CreatePath();
                    }
                    else
                    {                        
                        if (Random.value <= obstacleFrequency && trapCount < 2 && groundCount > 1 && groundCount < groundNumberOnPath)
                        {
                            trapCount++;
                            GameObject cactus = GetObject(listObstacle);
                            cactus.transform.position = thisObject.transform.position + Vector3.up * groundSize.y;
                            RandomRotationObject(cactus);
                        }
                        else
                        {
                            trapCount = 0;
                            CreateCoin(thisObject);
                        }                        
                    }
                }
            }
        }
    }

    //Create coin with given object
    void CreateCoin(GameObject positionObject)
    {
        if (Random.value <= coinFrequency)
        {
            float factor = (Random.value <= 0.5f) ? (1) : (2.5f);
            Vector3 pos = positionObject.transform.position + Vector3.up * (groundSize.y + factor);
            GameObject coin = GetObject(listCoin);
            coin.transform.position = pos;
        }  
    }

    GameObject PickATrap()
    {
        if (ScoreManager.Instance.Score >= scoreToCreateNewTrap)
        {
            int div = ScoreManager.Instance.Score / scoreToCreateNewTrap;
            if (div == 1) //Speed down trap, thorn trap
            {
                if (Random.value < 0.5)
                    return GetObject(listSpeedDownTrap);
                else
                    return GetObject(listThornTrap);
            }
            else if (div == 2) //Speed down trap, thorn trap, fire trap
            {
                int value = Random.Range(0, 3);
                if (value == 0)
                    return GetObject(listSpeedDownTrap);
                else if (value == 1)
                    return GetObject(listThornTrap);
                else
                    return GetObject(listFireTrap);
            }
            else if (div == 3) //Speed down trap, thorn trap, fire trap, ice trap
            {
                int value = Random.Range(0, 4);
                if (value == 0)
                    return GetObject(listSpeedDownTrap);
                else if (value == 1)
                    return GetObject(listThornTrap);
                else if (value == 2)
                    return GetObject(listFireTrap);
                else
                    return GetObject(listIceTrap);
            }
            else //Speed down trap, thorn trap, fire trap, ice trap, saw trap
            {
                int value = Random.Range(0, 5);
                if (value == 0)
                    return GetObject(listSpeedDownTrap);
                else if (value == 1)
                    return GetObject(listThornTrap);
                else if (value == 2)
                    return GetObject(listFireTrap);
                else if (value == 4)
                    return GetObject(listIceTrap);
                else
                    return GetObject(listSawTrap);
            }
        }
        else
        {
            return GetObject(listSpeedDownTrap);
        }
    }

    IEnumerator WaitAndHideStartPoint()
    {
        while (true)
        {
            Vector3 pos = Camera.main.WorldToViewportPoint(startPoint.transform.position);
            if (pos.y > 2f)
            {
                startPoint.SetActive(false);
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    //If the distance between player and fire is larger than maxDistance -> enable FireBall
    IEnumerator CheckAndEnableFireball()
    {
        fireBall.SetActive(false);
        while (true)
        {
            if (Vector3.Distance(fireBall.transform.position, playerController.transform.position) >= maxDistance)
            {
                fireBall.SetActive(true);
                fireBall.GetComponent<FireBallController>().Initial();
                yield break;
            }
            yield return null;
        }
    }
}
