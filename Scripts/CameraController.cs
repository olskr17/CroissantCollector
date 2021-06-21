using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Object References")]
    public PlayerController playerControl;

    [Header("VerticalGradientShader Settings")]
    public Material rockMaterial;
    public float originalTopY = -5;
    public float originalBottomY = -14;

    [HideInInspector]
    public bool stop { private set; get; }

    [Header("Camera Follow Smooth-Time")]
    public float smoothTime = 0.1f;
    public float goDownSmoothTime = 0.5f;
    public float followSpeedFactor = 2f;

    [Header("Shaking Effect")]
    // How long the camera shaking.
    public float shakeDuration = 0.1f;
    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.2f;
    public float decreaseFactor = 0.3f;
    [HideInInspector]
    public Vector3 originalPos;


    private Vector3 velocity = Vector3.zero;
    private Vector3 originalDistance;
    private float currentShakeDuration;
    private float currentDistance;
    private bool stopCheck = false;
    private bool isMoving = false;

    void OnDestroy()
    {
        // Reset stuff
        rockMaterial.SetFloat("_StartY", originalTopY);
        rockMaterial.SetFloat("_EndY", originalBottomY);
    }

    void Start()
    {
        rockMaterial.SetFloat("_StartY", originalTopY);
        rockMaterial.SetFloat("_EndY", originalBottomY);
        stop = true;
    }

    void CheckAndGetOriginalDistance()
    {
        Vector2 playerPos = Camera.main.WorldToViewportPoint(playerControl.transform.position);
        if (playerPos.y <= 0.6f && playerPos.x >= 0.5f)
        {
            originalDistance = transform.position - playerControl.transform.position;
            stop = false;
            stopCheck = true;
        }    
    }

    void Update()
    {
        if (!stopCheck)
            CheckAndGetOriginalDistance();

        if (!stop && !playerControl.isDead)
        {
            Vector3 currentDistance = transform.position - playerControl.transform.position;
            Vector3 pos = transform.position;

            if (currentDistance.x < originalDistance.x)
                pos += new Vector3(playerControl.movingSpeed * followSpeedFactor * Time.deltaTime, 0, 0);

            if (currentDistance.z < originalDistance.z)
                pos += new Vector3(0, 0, playerControl.movingSpeed * followSpeedFactor * Time.deltaTime);

            transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothTime);

            if (currentDistance.y > originalDistance.y)
            {
                MoveDown(currentDistance);
            }
        }        
    }

    IEnumerator Shake()
    {
        originalPos = transform.position;
        currentShakeDuration = shakeDuration;
        while (currentShakeDuration > 0)
        {
            transform.position = originalPos + Random.insideUnitSphere * shakeAmount;
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            yield return null;
        }
        transform.position = originalPos;
    }

    void MoveDown(Vector3 currentDistance)
    {
        if (!isMoving)
        {
            float factor = currentDistance.y - originalDistance.y;
            StartCoroutine(Move(factor, goDownSmoothTime));
        }
    }

    IEnumerator Move(float factor, float time)
    {
        isMoving = true;
        float startY = transform.position.y;
        float endY = startY - factor;
        float t = 0;
        float shaderTopY = rockMaterial.GetFloat("_StartY");
        float shaderBottomY = rockMaterial.GetFloat("_EndY");
        while (t < time && !playerControl.isDead)
        {
            t += Time.deltaTime;
            float fraction = t / time;
            float newY = Mathf.Lerp(startY, endY, fraction);
            Vector3 newPos = transform.position;
            newPos.y = newY;
            transform.position = newPos;

            float minus = Mathf.Lerp(0, factor, fraction);
            rockMaterial.SetFloat("_StartY", shaderTopY - minus);
            rockMaterial.SetFloat("_EndY", shaderBottomY - minus);
            yield return null;
        }

        isMoving = false;
    }

}
