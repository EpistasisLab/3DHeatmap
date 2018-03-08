var speed = 30.0;
var liftSpeed = 30;
var flightSpeed = 0.1;
var brakingFraction = 0.9;
var maxVelocity : float = 40.0;
var accel : float = 0.4;
var sensitivityX : float = 15.0;
var sensitivityY : float = 15.0;
var rotateSensitivityX : float = 0.5;
var rotateSensitivityY : float = 0.5;
var sensitivityDX : float = 40.0;

var minimumX : float = -360.0;
var maximumX : float = 360.0;

var minimumY : float = -360.0;
var maximumY : float = 360.0;

static var vertButton : float = 0.0;
static var horzButton : float = 0.0;
static var turnButton : float = 0.0;
static var spaceButton : float = 0.0;

static var sideMove : float = 0.0;
static var upDownMove : float = 0.0;
static var zoom : float = 0.0;

static var moveDirection : Vector3 = Vector3.zero;
private var grounded : boolean = false;
static var accelDirection : Vector3  = Vector3.zero;
private var myCamera;
private var originalRotation : Quaternion;
private static var rotateMove : float = 0.0;
private static var tiltMove : float = 0.0;
static var rotationX : float = 0.0;
static var rotationY : float = 0.0;
static var rotationChanged : boolean = false;
static var dxDirection : Vector3 = Vector3.zero;
private var controller : CharacterController;
private var rotationXChange : float = 0;
private var rotationYChange : float = 0;
static var haveTurned : boolean = false;	// Other classes check and clear this

function Start()
{
	myCamera = GameObject.FindWithTag("MainCamera").GetComponent("Camera");
	Debug.Log("Running Start in Fly on level " + Application.loadedLevelName);
	originalRotation = myCamera.transform.localRotation;
	controller = GetComponent(CharacterController);
}

function Update()
{
	if(Input.GetMouseButtonDown(0))
	{
		// Did any control field the click?
		Const.controlBusy = (GUIUtility.hotControl != 0);
	}
	
	CombinedUpdate();
}

function CombinedUpdate()
{
	var heightAdjust : Vector3 = Vector3.zero;
	var minDx : float = 2.0;
	
	#if UNITY_STANDALONE_WIN
	if(DX.hasController)
	{
		sideMove = DX.GetX();
		upDownMove = DX.GetZ();
		zoom = DX.GetY();
		dxDirection = Vector3(sideMove, zoom, upDownMove);
		dxDirection = myCamera.transform.TransformDirection(dxDirection);
		dxDirection *= speed;
	}
	else dxDirection = Vector3.zero;
	#else
	dxDirection = Vector3.zero;
	#endif
	
	accelDirection = Vector3.zero;
	vertButton = Input.GetAxisRaw("Vertical");
	horzButton = Input.GetAxisRaw("Horizontal");
	turnButton = Input.GetAxisRaw("Turn");
	spaceButton = Input.GetAxisRaw("Jump");
	if(vertButton && !turnButton)
	{
		if(spaceButton)
		{
			// uparrow becomes up rather than forward
			accelDirection = vertButton * myCamera.transform.up;
		}
		else
		{
			var dot = Vector3.Dot(moveDirection, myCamera.transform.forward);
			if((vertButton > 0) && (dot < 0)) dot = 0.0;
			moveDirection =  dot * myCamera.transform.forward;
			accelDirection = vertButton *  myCamera.transform.forward;
		}
	}
	if(horzButton && !turnButton)
	{
		accelDirection += horzButton * myCamera.transform.right;
	}
	accelDirection *= accel;
	moveDirection += accelDirection;
	
	// Stop if Fire1 is pressed
	if(!Const.controlBusy)
	{
		if(Input.GetButton("Fire1")) moveDirection = Vector3.zero;
	}
	
	if(moveDirection.magnitude > maxVelocity) moveDirection = moveDirection.normalized * maxVelocity;
	
	// ScrollWheel adjusts height
	if(!Const.menuScrolling) heightAdjust = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * liftSpeed * Vector3(0, 1, 0);
	
	controller.Move((moveDirection + dxDirection) * Time.deltaTime + heightAdjust);
	
	#if UNITY_STANDALONE_WIN
	if(DX.hasController)
	{
		rotateMove = DX.GetRY();
		tiltMove = DX.GetRX();
		rotationXChange = rotateMove * sensitivityDX * Time.deltaTime;
		rotationX += rotationXChange;
		rotationYChange = tiltMove * sensitivityDX * Time.deltaTime;
		rotationY += rotationYChange;
	}
	#endif
	
	if(turnButton)
	{
		if(spaceButton)
		{
			rotationXChange = horzButton * 90.0;
			rotationX += rotationXChange;
			rotationYChange = vertButton * 90.0;
			rotationY += rotationYChange;
		}
		else
		{
			rotationXChange = horzButton * rotateSensitivityX;
			rotationX += rotationXChange;
			rotationYChange = vertButton * rotateSensitivityY;
			rotationY += rotationYChange;
		}
	}
	
	if(Input.GetButton("Fire2") || rotationChanged)
	{
		rotationChanged = false;
		// Read the mouse input axis
		rotationXChange = Input.GetAxis("Mouse X") * sensitivityX;
		rotationX += rotationXChange;
		rotationYChange = Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY += rotationYChange;

		//rotationX = ClampAngle (rotationX, minimumX, maximumX);
		//rotationY = ClampAngle (rotationY, minimumY, maximumY);
	}

	var xQuaternion : Quaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
	var yQuaternion : Quaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
	
	myCamera.transform.localRotation = originalRotation * xQuaternion * yQuaternion;
	if(rotationXChange || rotationYChange) haveTurned = true; // Other classed check this
}

static function ClampAngle (angle : float, min : float, max : float)
{
	if (angle < -360.0)
		angle += 360.0;
	if (angle > 360.0)
		angle -= 360.0;
	return Mathf.Clamp (angle, min, max);
}

static function NewRotation(rotX : float, rotY : float)
{
	rotationX = rotX;
	rotationY = rotY;
	rotationChanged = true;
}

@script RequireComponent(CharacterController)