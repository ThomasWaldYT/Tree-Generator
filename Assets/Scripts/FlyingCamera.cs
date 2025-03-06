using Unity.VisualScripting;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float shiftSpeedMultiplier;

    [Header("Orbit")]
    [SerializeField] Vector3 lookTarget;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float orbitHeight;
    [SerializeField] private float orbitDistance;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isOrbiting = false;

    private bool shouldMove = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) shouldMove = !shouldMove;
        if (!shouldMove) return;

        if (Input.GetKey(KeyCode.F)) GetComponent<Camera>().fieldOfView = 30;
        else GetComponent<Camera>().fieldOfView = 60;

        if (isOrbiting)
        {
            HandleOrbit();

            if (Input.anyKey && !Input.GetKey(KeyCode.O) && !Input.GetKey(KeyCode.T) || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                ExitOrbitMode();
            }
        }
        else
        {
            //HandleCursorLock();
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.O))
            {
                EnterOrbitMode();
            }
        }
    }

    void HandleMovement()
    {
        // Mouse movement for looking around
        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // WASD movement
        float speed = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= shiftSpeedMultiplier;

        Vector3 move = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );

        transform.Translate(move * speed * Time.deltaTime, Space.Self);

        // Space/Control for Up/Down
        if (Input.GetKey(KeyCode.E))
            transform.position += Vector3.up * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            transform.position += Vector3.down * speed * Time.deltaTime;
    }

    void EnterOrbitMode()
    {
        isOrbiting = true;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z).normalized * orbitDistance
                             + lookTarget;
        transform.position = new Vector3(transform.position.x, transform.position.y + orbitHeight, transform.position.z);
        transform.LookAt(lookTarget);
    }

    void HandleOrbit()
    {
        transform.RotateAround(lookTarget, Vector3.up, orbitSpeed * Time.deltaTime);
    }

    void ExitOrbitMode()
    {
        isOrbiting = false;

        // Update rotationX and rotationY based on current camera rotation
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        rotationX = eulerAngles.y;
        rotationY = eulerAngles.x;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void HandleCursorLock()
    {
        Debug.Log(Time.time);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }
}
