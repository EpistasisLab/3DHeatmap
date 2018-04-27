using UnityEngine;
using System.Collections;

[System.Serializable]






[UnityEngine.RequireComponent(typeof(CharacterController))]
public partial class Fly : MonoBehaviour
{
    public float speed;
    public int liftSpeed;
    public float flightSpeed;
    public float brakingFraction;
    public float maxVelocity;
    public float accel;
    public float sensitivityX;
    public float sensitivityY;
    public float rotateSensitivityX;
    public float rotateSensitivityY;
    public float sensitivityDX;
    public float minimumX;
    public float maximumX;
    public float minimumY;
    public float maximumY;
    public static float vertButton;
    public static float horzButton;
    public static float turnButton;
    public static float spaceButton;
    public static float sideMove;
    public static float upDownMove;
    public static float zoom;
    public static Vector3 moveDirection;
    private bool grounded;
    public static Vector3 accelDirection;
    private Camera myCamera;
    private Quaternion originalRotation;
    private static float rotateMove;
    private static float tiltMove;
    public static float rotationX;
    public static float rotationY;
    public static bool rotationChanged;
    public static Vector3 dxDirection;
    private CharacterController controller;
    private float rotationXChange;
    private float rotationYChange;
    public static bool haveTurned;  // Other classes check and clear this

    public virtual void Start()
    {
        this.myCamera = GameObject.FindWithTag("MainCamera").GetComponent("Camera") as Camera;
        //Debug.Log("Running Start in Fly on level " + Application.loadedLevelName);
        this.originalRotation = this.myCamera.transform.localRotation;
        this.controller = (CharacterController) this.GetComponent(typeof(CharacterController));
    }

    public virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Did any control field the click?
            Const.controlBusy = GUIUtility.hotControl != 0;
        }
        this.CombinedUpdate();
    }

    public virtual void CombinedUpdate()
    {
        Vector3 heightAdjust = Vector3.zero;
        //float minDx = 2f;
        
        #if UNITY_STANDALONE_WIN
            if (DX.hasController)
            {
                Fly.sideMove = DX.GetX();
                Fly.upDownMove = DX.GetZ();
                Fly.zoom = DX.GetY();
                Fly.dxDirection = new Vector3(Fly.sideMove, Fly.zoom, Fly.upDownMove);
                Fly.dxDirection = this.myCamera.transform.TransformDirection(Fly.dxDirection);
                Fly.dxDirection = Fly.dxDirection * this.speed;
            }
            else
            {
                Fly.dxDirection = Vector3.zero;
            }
        #else
            Fly.accelDirection = Vector3.zero;
        #endif

        Fly.vertButton = Input.GetAxisRaw("Vertical");
        Fly.horzButton = Input.GetAxisRaw("Horizontal");
        Fly.turnButton = Input.GetAxisRaw("Turn");
        Fly.spaceButton = Input.GetAxisRaw("Jump");
        if ((Fly.vertButton != 0f) && (Fly.turnButton == 0f))
        {
            if (Fly.spaceButton != 0f)
            {
                // uparrow becomes up rather than forward
                Fly.accelDirection = Fly.vertButton * this.myCamera.transform.up;
            }
            else
            {
                float dot = Vector3.Dot(Fly.moveDirection, this.myCamera.transform.forward);
                if ((Fly.vertButton > 0) && (dot < 0))
                {
                    dot = 0f;
                }
                Fly.moveDirection = dot * this.myCamera.transform.forward;
                Fly.accelDirection = Fly.vertButton * this.myCamera.transform.forward;
            }
        }
        if ((Fly.horzButton != 0f) && (Fly.turnButton == 0f))
        {
            Fly.accelDirection = Fly.accelDirection + (Fly.horzButton * this.myCamera.transform.right);
        }
        Fly.accelDirection = Fly.accelDirection * this.accel;
        Fly.moveDirection = Fly.moveDirection + Fly.accelDirection;

        // Stop if Fire1 is pressed
        if (!Const.controlBusy)
        {
            if (Input.GetButton("Fire1"))
            {
                Fly.moveDirection = Vector3.zero;
            }
        }
        if (Fly.moveDirection.magnitude > this.maxVelocity)
        {
            Fly.moveDirection = Fly.moveDirection.normalized * this.maxVelocity;
        }
        // ScrollWheel adjusts height
        if (!Const.menuScrolling)
        {
            heightAdjust = ((Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * this.liftSpeed) * new Vector3(0, 1, 0);
        }
        this.controller.Move(((Fly.moveDirection + Fly.dxDirection) * Time.deltaTime) + heightAdjust);

        #if UNITY_STANDALONE_WIN
        if (DX.hasController)
        {
            Fly.rotateMove = DX.GetRY();
            Fly.tiltMove = DX.GetRX();
            this.rotationXChange = (Fly.rotateMove * this.sensitivityDX) * Time.deltaTime;
            Fly.rotationX = Fly.rotationX + this.rotationXChange;
            this.rotationYChange = (Fly.tiltMove * this.sensitivityDX) * Time.deltaTime;
            Fly.rotationY = Fly.rotationY + this.rotationYChange;
        }
        #endif

        if (Fly.turnButton != 0f)
        {
            if (Fly.spaceButton != 0f)
            {
                this.rotationXChange = Fly.horzButton * 90f;
                Fly.rotationX = Fly.rotationX + this.rotationXChange;
                this.rotationYChange = Fly.vertButton * 90f;
                Fly.rotationY = Fly.rotationY + this.rotationYChange;
            }
            else
            {
                this.rotationXChange = Fly.horzButton * this.rotateSensitivityX;
                Fly.rotationX = Fly.rotationX + this.rotationXChange;
                this.rotationYChange = Fly.vertButton * this.rotateSensitivityY;
                Fly.rotationY = Fly.rotationY + this.rotationYChange;
            }
        }
        if (Input.GetButton("Fire2") || Fly.rotationChanged)
        {
            Fly.rotationChanged = false;
            // Read the mouse input axis
            this.rotationXChange = Input.GetAxis("Mouse X") * this.sensitivityX;
            Fly.rotationX = Fly.rotationX + this.rotationXChange;
            this.rotationYChange = Input.GetAxis("Mouse Y") * this.sensitivityY;
            Fly.rotationY = Fly.rotationY + this.rotationYChange;

            //rotationX = ClampAngle (rotationX, minimumX, maximumX);
            //rotationY = ClampAngle (rotationY, minimumY, maximumY);

        }
        Quaternion xQuaternion = Quaternion.AngleAxis(Fly.rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(Fly.rotationY, Vector3.left);
        this.myCamera.transform.localRotation = (this.originalRotation * xQuaternion) * yQuaternion;
        if ((this.rotationXChange != 0f) || (this.rotationYChange != 0f))
        {
            Fly.haveTurned = true; // Other classed check this
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle = angle + 360f;
        }
        if (angle > 360f)
        {
            angle = angle - 360f;
        }
        return Mathf.Clamp(angle, min, max);
    }

    public static void NewRotation(float rotX, float rotY)
    {
        Fly.rotationX = rotX;
        Fly.rotationY = rotY;
        Fly.rotationChanged = true;
    }

    public Fly()
    {
        this.speed = 30f;
        this.liftSpeed = 30;
        this.flightSpeed = 0.1f;
        this.brakingFraction = 0.9f;
        this.maxVelocity = 40f;
        this.accel = 0.4f;
        this.sensitivityX = 15f;
        this.sensitivityY = 15f;
        this.rotateSensitivityX = 0.5f;
        this.rotateSensitivityY = 0.5f;
        this.sensitivityDX = 40f;
        this.minimumX = -360f;
        this.maximumX = 360f;
        this.minimumY = -360f;
        this.maximumY = 360f;
    }

    static Fly()
    {
        Fly.moveDirection = Vector3.zero;
        Fly.accelDirection = Vector3.zero;
        Fly.dxDirection = Vector3.zero;
    }

}