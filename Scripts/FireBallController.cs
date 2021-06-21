using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallController : MonoBehaviour {

    [Header("Gameplay Config")]
    public float oneStepJumpForce = 15f;
    public float twoStepJumpForce = 22f;
    public float gravity = 60f;

    [Header("Object References")]
    public GameManager gameManager;
    public PlayerController playerController;

    private GameObject currentLastGround;
    private GameObject currentObject;
    private CharacterController charControl;
    private Vector3 movingDir;
    private Vector3 rollingDir;
    private float jumpForce;
    private float verticalVelocity;
    private float speed;
    private float rollingSpeed;
    private bool jump = false;


    public void Initial()
    {
        charControl = GetComponent<CharacterController>();

        movingDir = Vector3.forward;
        rollingDir = Vector3.right;
        currentLastGround = null;
        currentObject = null;
        StartCoroutine(DoRotate());
        StartCoroutine(MoveDown());
    }

    // Update is called once per frame
    void Update () {

        float distance = Vector3.Distance(transform.position, playerController.transform.position);
        if (distance < gameManager.maxDistance)
        {
            speed = 0.9f * playerController.maxMovingSpeed;
        }
        else if (distance > gameManager.maxDistance)
        {
            speed = 1.2f * playerController.maxMovingSpeed;
        }
        else
            speed = playerController.maxMovingSpeed;

        transform.position += movingDir * speed * Time.deltaTime;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("LastGround") && hit.collider.gameObject != currentLastGround)
        {
            currentLastGround = hit.collider.gameObject;
            StartCoroutine(CheckAndTurn(currentLastGround.transform.position.z, currentLastGround.transform.position.x));
        }

        if (!hit.collider.CompareTag("LastGround") && hit.collider.gameObject != currentObject)
        {
            currentObject = hit.collider.gameObject;
            string tag = hit.collider.tag;
            float checkLength;
            if (tag.Equals("SawTrap"))
                checkLength = gameManager.sawTrapSize.x;
            else
                checkLength = gameManager.groundSize.x;
            Vector3 rayPos = currentObject.transform.position + new Vector3(0, 0.5f, 0);
            Ray ray = new Ray(rayPos, movingDir);
            RaycastHit raycastHit;
            if (!Physics.Raycast(ray, out raycastHit, checkLength))
            {
                Vector3 newRayPos = (rayPos + movingDir * checkLength);
                Ray newRay = new Ray(newRayPos, movingDir);
                bool check = Physics.Raycast(newRay, out raycastHit, gameManager.groundSize.x);
                if (!check)
                    jumpForce = twoStepJumpForce;
                else
                    jumpForce = oneStepJumpForce;
                StartCoroutine(CheckAndEnableJump(currentObject.transform.position.x,
                                                  currentObject.transform.position.z));
            }
        }
    }

    IEnumerator DoRotate()
    {
        while (true)
        {
            rollingSpeed = (speed * 2f) * 10f;
            transform.RotateAround(transform.position, rollingDir, rollingSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator MoveDown()
    {
        while (true)
        {
            if (charControl.isGrounded)
            {
                verticalVelocity = -gravity * Time.deltaTime;
            }
            else
            {
                if (jump)
                {
                    verticalVelocity = jumpForce;
                    jump = false;
                }
                else
                    verticalVelocity -= gravity * Time.deltaTime;
            }
            Vector3 verticalVector = new Vector3(0, verticalVelocity, 0);
            charControl.Move(verticalVector * Time.deltaTime);

            yield return null;
        }

    }

    IEnumerator CheckAndTurn(float z, float x)
    {
        while (true)
        {
            if (movingDir == Vector3.forward)
            {
                if (transform.position.z >= z)
                {
                    transform.position = new Vector3(transform.position.x,
                        transform.position.y, z);
                    movingDir = Vector3.right;
                    rollingDir = Vector3.back;
                    yield break;
                }
            }
            else
            {
                if (transform.position.x >= x)
                {
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                    movingDir = Vector3.forward;
                    rollingDir = Vector3.right;
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator CheckAndEnableJump(float checkX, float checkZ)
    {
        if (movingDir == Vector3.forward)
        {
            while (true)
            {
                if (transform.position.z >= checkZ)
                {
                    jump = true;
                    yield break;
                }
                yield return null;
            }
        }
        else
        {
            while (true)
            {
                if (transform.position.x >= checkX)
                {
                    jump = true;
                    yield break;
                }
                yield return null;
            }
        }
    }

}
