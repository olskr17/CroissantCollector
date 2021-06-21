using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SgLib;


public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDied;

    [Header("Gameplay Config")]
    public float maxMovingSpeed = 5f;
    [SerializeField]
    private float minMovingSpeed = 1f;
    [SerializeField]
    private float maxJumpForce = 10f;
    [SerializeField]
    private float minJumpForce = 2f;
    [SerializeField]
    private float gravity = 14f;
    [SerializeField]
    private float smoothFixedPosTime = 0.5f;

    [Header("Object References")]
    public GameManager gameManager;
    public CameraController cameraController;
    public GameObject main;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftFoot;
    public GameObject rightFoot;
    public AnimationClip playerJump;
    public AnimationClip playerRun;
    public AnimationClip playerSanto;
    public AnimationClip playerDie;

    [HideInInspector]
    public Vector3 movingDirection { private set; get; }

    [HideInInspector]
    public float movingSpeed { private set; get; }

    [HideInInspector]
    public bool touchDisable { private set; get; }

    [HideInInspector]
    public bool isDead { private set; get; }

    private CharacterController charControl;
    private Rigidbody rigid;
    private Animator anim;
    private GameObject currentLastGround;
    private LastGroundController currentLastGroundControl;
    private List<string> listDeadTag = new List<string>();
    private float jumpForce;
    private float verticalVelocity;
    public float yCheckPos;
    private float jumpCount = 0;
    private float factor;
    private RaycastHit hit;

    void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
    }

    void Start()
    {
        //        // Uncomment to enable changing the character to the selected one
        GameObject currentCharacter = CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex];
        Material charMaterial = currentCharacter.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

        //Get the mesh
        Mesh charMainMesh = currentCharacter.transform.Find("Main").GetComponent<MeshFilter>().sharedMesh;
        Mesh charLeftHandMesh = currentCharacter.transform.Find("LeftHand").GetComponent<MeshFilter>().sharedMesh;
        Mesh charRightHandMesh = currentCharacter.transform.Find("RightHand").GetComponent<MeshFilter>().sharedMesh;
        Mesh charLeftFootMesh = currentCharacter.transform.Find("LeftFoot").GetComponent<MeshFilter>().sharedMesh;
        Mesh charRightFootMesh = currentCharacter.transform.Find("RightFoot").GetComponent<MeshFilter>().sharedMesh;

        //Change player's child mesh to the selected character
        main.GetComponent<MeshFilter>().mesh = charMainMesh;
        leftHand.GetComponent<MeshFilter>().mesh = charLeftHandMesh;
        rightHand.GetComponent<MeshFilter>().mesh = charRightHandMesh;
        leftFoot.GetComponent<MeshFilter>().mesh = charLeftFootMesh;
        rightFoot.GetComponent<MeshFilter>().mesh = charRightFootMesh;
        main.GetComponent<Renderer>().material = charMaterial;
        leftHand.GetComponent<Renderer>().material = charMaterial;
        rightHand.GetComponent<Renderer>().material = charMaterial;
        leftFoot.GetComponent<Renderer>().material = charMaterial;
        rightFoot.GetComponent<Renderer>().material = charMaterial;

        charControl = GetComponent<CharacterController>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        currentLastGroundControl = null;
        movingDirection = Vector3.forward;
        touchDisable = false;
        isDead = false;
        movingSpeed = maxMovingSpeed;
        jumpForce = maxJumpForce;
        factor = gameManager.groundPrefab.GetComponent<Renderer>().bounds.size.y - 0.2f;

        yCheckPos = gameManager.firstGround.transform.position.y + factor;

        //Add dead tag
        listDeadTag.Add("ThornTrap");
        listDeadTag.Add("Saw");
        listDeadTag.Add("IceTrap");
        listDeadTag.Add("FireTrap");
        listDeadTag.Add("Bullet");
        listDeadTag.Add("Obstacle");
    }
	
    // Update is called once per frame
    void Update()
    {     
        if (gameManager.GameState == GameState.Playing)
        {
            if (!isDead)
            {
                if (movingDirection == Vector3.right)
                    transform.position += new Vector3(movingSpeed * Time.deltaTime, 0, 0);
                else
                    transform.position += new Vector3(0, 0, movingSpeed * Time.deltaTime);

                if (charControl.isGrounded)
                {
                    anim.Play(playerRun.name);
                    jumpCount = 0;
                    verticalVelocity = -gravity * Time.deltaTime;
                    if (Input.GetMouseButtonDown(0) && !touchDisable && !cameraController.stop)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.jump);
                        jumpCount++;
                        verticalVelocity = jumpForce;
                        anim.Play(playerJump.name);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !touchDisable && jumpCount < 2 && !cameraController.stop)
                    {
                        jumpCount++;
                        verticalVelocity = jumpForce;
                        if (jumpCount == 2)
                        {
                            anim.Play(playerSanto.name);
                            SoundManager.Instance.PlaySound(SoundManager.Instance.doubleJump);
                        }
                        else
                        {
                            SoundManager.Instance.PlaySound(SoundManager.Instance.jump);
                            anim.Play(playerJump.name);
                        }                           
                    }
                    else
                        verticalVelocity -= gravity * Time.deltaTime;
                }

                Vector3 jumpVector = new Vector3(0, verticalVelocity, 0);
                charControl.Move(jumpVector * Time.deltaTime);
            }
        }

        if (!isDead && !charControl.isGrounded)
        {
            Ray rayDown = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (!Physics.Raycast(rayDown, out hit, 20f)) //Don't have any object below
            {
                if (transform.position.y <= yCheckPos)
                {
                    GameOver();
                    Vector3 fallingDir = (movingDirection + Vector3.down * 3f).normalized;
                    StartCoroutine(PlayerFall(fallingDir, maxMovingSpeed * 2f));
                }
            }
            
        }
    }

    

    // Listens to changes in game state
    void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            // Do whatever necessary when a new game starts

        }      
    }

    // Calls this when the player dies and game over
    public void Die()
    {
        // Fire event
        PlayerDied();
    }

    IEnumerator PlayerFall(Vector3 fallingDir, float fallingSpeed)
    {
        while (true)
        {
            transform.position += fallingDir * fallingSpeed * Time.deltaTime;
            yield return null;
        }
    }

    void GameOver()
    {
        isDead = true;
        charControl.enabled = false;
        gameManager.GameOver();
        Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (listDeadTag.Contains(other.tag))
        {
            GameOver();
            PushPlayerAway(other.transform.position);
        }

        if (other.CompareTag("FireBall"))
        {
            GameOver();
            StartCoroutine(HandlePlayerHitFireBall());
        }
    }

    IEnumerator HandlePlayerHitFireBall()
    {
        anim.enabled = false;
        float time = 0.3f;
        Vector3 startRot = transform.eulerAngles;
        Vector3 endRot = new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(1, 1, 0.1f);
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            transform.eulerAngles = Vector3.Lerp(startRot, endRot, factor);
            transform.localScale = Vector3.Lerp(startScale, endScale, factor);
            yield return null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Standing on last ground -> wait for touch and turn
        if (hit.collider.CompareTag("LastGround") && hit.collider.gameObject != currentLastGround)
        {
            currentLastGround = hit.collider.gameObject;
            currentLastGroundControl = hit.collider.GetComponent<LastGroundController>();
            touchDisable = true;
            StartCoroutine(WaitForTouch());
        }
        else if (hit.collider.CompareTag("SpeedDownTrap")) //Reduce moving speed
        {
            movingSpeed = minMovingSpeed;
            jumpForce = minJumpForce;
        }
        else if (listDeadTag.Contains(hit.collider.tag)) //Dead tag -> game over
        {
            if (hit.collider.CompareTag("FireTrap"))
            {
                GameObject par = Instantiate(gameManager.firedParticle, transform.position, Quaternion.identity);
                par.transform.eulerAngles = new Vector3(-90, 0, 0);
                foreach (Renderer o in transform.GetComponentsInChildren<Renderer>())
                {
                    o.material.SetFloat("_Metallic", 1);
                }
                PlayParticle(par.GetComponent<ParticleSystem>());
            }
            if (hit.collider.CompareTag("IceTrap"))
            {
                GameObject par = Instantiate(gameManager.freezingParticle, transform.position, Quaternion.identity);
                par.transform.SetParent(transform);
                anim.enabled = false;
                PlayParticle(par.transform.GetChild(0).GetComponent<ParticleSystem>());
            }
            if (hit.collider.CompareTag("ThornTrap"))
            {
                hit.gameObject.GetComponent<ThornTrapController>().GoUp();
            }
            GameOver();
            PushPlayerAway(hit.collider.transform.position);
        }
        else //Nornal ground
        {
            movingSpeed = maxMovingSpeed;
            jumpForce = maxJumpForce;
            if (hit.collider.CompareTag("Stair"))
            {
                Ray ray = new Ray(hit.transform.position, movingDirection);
                RaycastHit rayCastHit;
                if (Physics.Raycast(ray, out rayCastHit, 20f))
                    yCheckPos = rayCastHit.collider.transform.position.y + factor;
            }
            else
                yCheckPos = hit.transform.position.y + factor;
        }       
    }

    //Call this function when player hit thorns, fire trap, ice trap or bullet
    void PushPlayerAway(Vector3 hitPos)
    {
        rigid.isKinematic = false;
        Vector3 dir_1 = (movingDirection == Vector3.forward) ? (Vector3.right) : (Vector3.forward);
        Vector3 dir = (Vector3.up * 0.7f + dir_1).normalized;
        rigid.AddForce(dir * 800f);
        anim.Play(playerDie.name);
    }

    IEnumerator WaitForTouch()
    {
        while (true)
        {
            
            float distance = (movingDirection == Vector3.forward) ?
                            (currentLastGround.transform.position.z - transform.position.z) :
                            (currentLastGround.transform.position.x - transform.position.x);
            float checkSize = (movingDirection == Vector3.forward) ?
                            (gameManager.lastGroundSize.z / 2) :
                            (gameManager.lastGroundSize.x / 2);
            if (distance < 0 && Mathf.Abs(distance) >= checkSize)//Player run out of the ground -> game over
            {
                GameOver();
                Vector3 fallingDir = movingDirection + Vector3.down;
                StartCoroutine(PlayerFall(fallingDir, maxMovingSpeed * 1.5f));
                yield break;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Turn();
                    yield break;
                }
            }        
            yield return null;
        }
    }

    void Turn()
    {
        ScoreManager.Instance.AddScore(1);
        currentLastGroundControl.Change();
        touchDisable = false;

        //Change the rotation of the player and fix the position
        if (movingDirection == Vector3.forward)
        {            
            float startZ = transform.position.z;
            float endZ = currentLastGroundControl.transform.position.z;

            transform.eulerAngles += new Vector3(0, 90, 0);
            StartCoroutine(FixZPos(startZ, endZ, smoothFixedPosTime));
        }
        else
        {
            float startX = transform.position.x;
            float endX = currentLastGroundControl.transform.position.x;
            transform.eulerAngles += new Vector3(0, -90, 0);
            StartCoroutine(FixXPos(startX, endX, smoothFixedPosTime));
        }

        //Change moving direction
        movingDirection = (movingDirection == Vector3.forward) ? (Vector3.right) : (Vector3.forward);
    }


    IEnumerator FixXPos(float startX, float endX, float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            float newX = Mathf.Lerp(startX, endX, factor);
            Vector3 newPos = transform.position;
            newPos.x = newX;
            transform.position = newPos;
            yield return null;
        }
    }


    IEnumerator FixZPos(float startZ, float endZ, float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            float newZ = Mathf.Lerp(startZ, endZ, factor);
            Vector3 newPos = transform.position;
            newPos.z = newZ;
            transform.position = newPos;
            yield return null;
        }
    }

    void PlayParticle(ParticleSystem particle)
    {
        var main = particle.main;
        particle.Play();
        Destroy(particle.gameObject, main.startLifetimeMultiplier);
    }

}
