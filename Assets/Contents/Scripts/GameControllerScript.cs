using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax;
}
public class GameControllerScript : MonoBehaviour
{
    public CameraController camera;
    private CharacterController characterController;
    private CapsuleCollider characterCollider;
    private ColliderTrigger characterColliderTrigger;
    public GameObject[] effetcs;
    private GameObject effetcsRoot;

    private Animator playerAnimator;
    private GameObject player;
    private float playerX;
    private float playerZ;
    private float startTime;
    private float timeEplased;
    private float playerZCheck;
    [HideInInspector]
    public float currentLevelSpeed;
    public static float laneWidth = 6;
    private float trackSpacing;
    private float trackIndexPosition;
    private float characterRotation;
    [HideInInspector]
    public float characterAngle = 45f;
    private float laneChangeTime = 0.29f;
    [HideInInspector]
    public float verticalSpeed;
    private float verticalSpeed_jumpTolerance = -30f;
    private float verticalFallSpeedLimit = -1f;
    public float gravity = 100;
    public float jumpHeight = 10f;
    [HideInInspector]
    public float obstacleCornerOffset = 18f;
    private float hitXOffset = 0.5f;
    private float hitMinOffset = 0.32f;
    private float hitMaxOffset = 0.65f;
    private float hitZOffset = 31;
    private float height = 3.5f;


    private IEnumerator changeLine; //El necessitem per cancelar lanimacio al chocar


    // Necesitem detectar on colisiona per saber si fem un cop o s'ha acabat el joc
    public enum HitLocation
    {
        Left = 0,
        XMiddle = 1,
        Right = 2,
        Upper = 3,
        YMiddle = 4,
        Lower = 5,
        Before = 6,
        ZMiddle = 7,
        After = 8
    }
    public enum StumbleLocation
    {
        LeftSide = 0,
        RightSide = 1,
        FrontLower = 2,
        FrontMiddle = 3,
        FrontUpper = 4,
        FrontLeftCorner = 5,
        FrontRightCorner = 6,
        BackLeftCorner = 7,
        BackRightCorner = 8,
        LeftTrackBorder = 9,
        RightTrackBorder = 10
    }

    //Diferents efectes que podem trobar
    public enum EffetcType
    {
        None = 0,
        LandingPuff = 1,
        DeathPuff = 3,
        StumplePuff = 4
    }

    [HideInInspector]
    public bool isRolling;
    private Coroutine endRollCoroutine;


    //Controlem l'acceleració del personatge
    public Acceleration speed = new Acceleration();
    public class Acceleration
    {
        public float start = 10;
        public float end = 100;
        public float accelerationTime = 500;
    }

    [HideInInspector]
    public Vector3 currentPosition;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 trackLeft, trackRight;
    [HideInInspector]
    public bool isGameOver;
    [HideInInspector]
    public bool playerIsRunning;
    private bool changeLaneTrig;
    [HideInInspector]
    public bool isJumping;
    [HideInInspector]
    public bool isFalling;
    private bool IsGrounded;
    public bool disableStumpleOnTrackBorders;


    [HideInInspector]
    public int numberOfTracks = 3;
    [HideInInspector]
    public int trackIndex;
    private int trackIndexTarget;
    private int trackMovement;
    private int trackMovementNext;
    private int trackMovementLast =0;
    private OVRManager ovr = null;




    void Awake()
    {
        camera = GameObject.FindWithTag("CameraController").GetComponent<CameraController>();
        player = GameObject.FindWithTag("Player");
        characterController = player.GetComponent<CharacterController>();
        playerAnimator = player.GetComponentInChildren<Animator>();
        characterCollider = player.GetComponentInChildren<CapsuleCollider>();

        effetcsRoot = GameObject.Find("EffetcsRoot");
        characterColliderTrigger = this.characterCollider.GetComponent<ColliderTrigger>();
        characterColliderTrigger.OnEnter = (ColliderTrigger.OnEnterDelegate)Delegate.Combine(this.characterColliderTrigger.OnEnter, new ColliderTrigger.OnEnterDelegate(this.OnCharacterColliderEnterTrigger));
        ovr = GameObject.FindWithTag("CameraController").GetComponent<OVRManager>();

       
        Reset();
    }
    void Start()
    {
        height = ovr.headPoseRelativeOffsetTranslation.y;
        print(height);
    }
    void FixedUpdate()
    {
        if (!isGameOver)
        {
            UpdateFunction();
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Actualitzem la volocitat
            UpdateAcceleration();

            // Mirem si s'han apretat els controls
            UpdateControls();

        }

    }

    //Posem tot per defecte a 0
    public void Reset()
    {
        playerX = 0;
        playerZ = 0;
        currentPosition = new Vector3(0, 0, 0);
        player.transform.position = new Vector3(0, 0, 0);

        startTime = Time.time;
        timeEplased = 0;
        currentLevelSpeed = this.Accelerate(Time.time - startTime);

        trackIndex = 0;
        trackIndexTarget = 0;
        trackIndexPosition = trackIndex;

        trackSpacing = 6f;
        isRolling = false;
        isJumping = false;
        isGameOver = false;

    }

    private void UpdateAcceleration()
    {
        if (playerZ > playerZCheck)
        {
            if (timeEplased > 0)
            {
                startTime += timeEplased;
                timeEplased = 0;
            }
            currentLevelSpeed = this.Accelerate(Time.time - startTime);
            playerIsRunning = true;
        }
        else
        {
            timeEplased += Time.deltaTime;
            playerIsRunning = false;
        }


        playerZCheck = playerZ;
    }

    public float Accelerate(float t)
    {

        if (t >= this.speed.accelerationTime) { return this.speed.end; }
        return t * (this.speed.end - this.speed.start) / this.speed.accelerationTime + this.speed.start;
    }
   
    private void UpdateFunction()
    {

        Vector3 peakPosition = currentPosition;

        // Eix de la z
        peakPosition = ApplyZMovement(peakPosition);

        // Eix de la x
        peakPosition = ApplyXMovement(peakPosition);

        // Eix de la y
        peakPosition = ApplyYMovement(peakPosition);

        // Apliquem el moviment al personatge
        characterController.Move(peakPosition);

        playerZ = player.transform.position.z;


    }

    // eix X
    private Vector3 ApplyXMovement(Vector3 position)
    {
        
        Vector3 currentPos = player.transform.position;
        Vector3 nextPos = Vector3.right * playerX;
        return new Vector3((nextPos - currentPos).x, position.y, position.z);
    }

   
    // eix Y
    private Vector3 ApplyYMovement(Vector3 position)
    {

        if ((verticalSpeed < 0f) && characterController.isGrounded)
        {
            verticalSpeed = 0f;
            IsGrounded = true;
            if (isJumping || isFalling)
            {
                isJumping = false;
                isFalling = false;
                IsGrounded = true;

                if (isRolling == false && !isGameOver)
                {
                    playerAnimator.Play("run", 0, 0);

                }

                //Toca a terra
                GameGlobals.Instance.PlayerPlaySound("ground");
                doAnEffect(EffetcType.LandingPuff);
            }
        }
       

        verticalSpeed -= this.gravity * Time.deltaTime;

        if ((!characterController.isGrounded && !isFalling) && ((verticalSpeed < verticalFallSpeedLimit) && !isRolling))
        {
            isFalling = true;
            IsGrounded = false;

        }
        if (isFalling && !isJumping)
        {
            if(isRolling) EndRoll();

            playerAnimator.Play("dive", 0, 0);
        }


        Vector3 zero = (Vector3)((this.verticalSpeed * Time.deltaTime) * Vector3.up);

        return new Vector3(position.x, zero.y, position.z);

    }

    // Eix Z
    private Vector3 ApplyZMovement(Vector3 position)
    {

        Vector3 currentPos = player.transform.position;
        playerZ += (currentLevelSpeed * Time.deltaTime);
        Vector3 nextPos = Vector3.forward * playerZ;

        return new Vector3(position.x, position.y, (nextPos - currentPos).z);
    }







    private Vector2 initialPos;

    void Calculate(Vector3 finalPos)
    {
        float disX = Mathf.Abs(initialPos.x - finalPos.x);
        float disY = Mathf.Abs(initialPos.y - finalPos.y);
        if (disX > 0 || disY > 0)
        {
            if (disX > disY)
            {
                if (initialPos.x > finalPos.x)
                {
                    HandleSwipe("LEFT");
                    print("LEFT");
                }
                else
                {
                    HandleSwipe("RIGHT");
                    print("RIGHT");
                }
            }
            else
            {
                if (initialPos.y > finalPos.y)
                {
                    HandleSwipe("DOWN");
                    print("DOWN");
                }
                else
                {
                    HandleSwipe("UP");
                    print("UP");
                }
            }
        }
    }

    void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Calculate(Input.mousePosition);
        }

    }


    private void UpdateControls()
    {


        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || OVRInput.Get(OVRInput.Button.Up))//Up
        {
            HandleSwipe("UP");
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || OVRInput.Get(OVRInput.Button.Down))// jump
        {
            HandleSwipe("Down");
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || OVRInput.Get(OVRInput.Button.Right))//Left
        {
            HandleSwipe("Right");
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || OVRInput.Get(OVRInput.Button.Left))//Right
        {
            HandleSwipe("Left");
        }

        TouchController.SwipeDirection direction = GameGlobals.touchController.getSwipeDirection();
        if(direction != TouchController.SwipeDirection.Null)
        {
            if(direction == TouchController.SwipeDirection.Up)
            {
                HandleSwipe("UP");
            }
            if (direction == TouchController.SwipeDirection.Down)
            {
                HandleSwipe("Down");
            }
            if (direction == TouchController.SwipeDirection.Right)
            {
                HandleSwipe("Right");
            }
            if (direction == TouchController.SwipeDirection.Left)
            {
                HandleSwipe("Left");
            }

        }

        //DetectSwipe();





    }

    private void HandleSwipe(String swipeDir)
    {

        //Si estem 'Rolling', al aplicar qualsevol moviment l'aturem.
        if (isRolling == true)
        {
            EndRoll();
            //Aqui no podem parar audio rodar perque al apretar dos cops Down, faria coses rares
        }

        switch (swipeDir.ToLower())
        {
            case "up":
                //TODO: Parar audio rodar

                doJump();
                break;

            case "down":
                doRoll();
                break;

            case "left":
                //TODO: Parar audio rodar

                doChangeLane(-1, laneChangeTime);
                break;

            case "right":
                //TODO: Parar audio rodar

                doChangeLane(1, laneChangeTime);
                break;
        }



    }




    public void doPlayerRun()
    {
        if (playerAnimator != null && !isGameOver)
        {
            playerAnimator.Play("run", 0, 0);
            GameGlobals.Instance.PlayerPlaySound("run");
        }

    }

    //Rodem, important modificar el collider per que no es mengi les valles.
    public void doRoll()
    {

        if (!this.isRolling)
        {
            camera.Rolling(true);
            this.isRolling = true;

            characterController.height = 1.4f;
            characterController.center = new Vector3(0, 0.74f, 0.04f);

            characterCollider.height = 1.4f;
            characterCollider.center = new Vector3(0, 0.74f, 0.04f);

            


            ovr.headPoseRelativeOffsetTranslation = new Vector3(0f, 0.74f, 0f);



            if (!getTransitionFromHeight())
            {
                this.verticalSpeed = -this.CalculateJumpVerticalSpeed(this.jumpHeight);
            }
            else
            {
                this.verticalSpeed = -250f;
            }


            playerAnimator.Play("roll", 0, 0);

            GameGlobals.Instance.PlayerPlaySound("roll");

            if (endRollCoroutine != null)
            {
                StopCoroutine(endRollCoroutine);
            }

            endRollCoroutine = StartCoroutine(rollDisabler());

        }

    }

    public void doJump()
    {


        bool jumpFlag = (!isJumping && (verticalSpeed <= 0f)) && (this.verticalSpeed > verticalSpeed_jumpTolerance);
        if (characterController.isGrounded || jumpFlag && !getTransitionFromHeight())
        {

            isJumping = true;
            isFalling = false;
            IsGrounded = false;

            verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);

            GameGlobals.Instance.PlayerPlaySound("jump");
            playerAnimator.Play("jump", 0, 0);
        }

    }

    public void doChangeLane(int movement, float duration)
    {

        if (changeLaneTrig == true) return;

        if (trackMovement != movement)
        {
            ForceChangeTrack(movement, duration);
        }
        else
        {
            trackMovementNext = movement;
        }
    }

    public void ForceChangeTrack(int movement, float duration)
    {
        changeLaneTrig = true;
        changeLine = ChangeTrackCoroutine(movement, duration);
        StartCoroutine(changeLine);
    }

    public float JumpLength()
    {
        return currentLevelSpeed * 2f * this.CalculateJumpVerticalSpeed(jumpHeight) / this.gravity;
    }

    public float CalculateJumpVerticalSpeed(float jumpHeight)
    {
        return Mathf.Sqrt((2f * jumpHeight) * this.gravity);
    }

    private bool getTransitionFromHeight()
    {
        if (player.transform.position.y > 5.0f)
        {
            return true;
        }

        return false;
    }

    //Pararem de rodar
    public void EndRoll()
    {

        camera.Rolling(false);
        if (this.characterController.enabled)
        {
            this.characterController.Move((Vector3)(Vector3.up * 2f));
        }

        characterController.height = 3.5f;
        characterController.center = new Vector3(0, 1.75f, 0.04f);

        characterCollider.height = 3.5f;
        characterCollider.center = new Vector3(0, 1.75f, 0.04f);

        ovr.headPoseRelativeOffsetTranslation = new Vector3(0f, height, 0f);

        if (this.characterController.enabled)
        {
            this.characterController.Move((Vector3)(Vector3.down * 2f));
        }
        this.isRolling = false;

        if (isJumping == false && !isGameOver)
        {
            playerAnimator.Play("run", 0, 0);

        }


    }

    //Pararem de rodar al cap de 1.3 segons
    IEnumerator rollDisabler()
    {
        yield return new WaitForSeconds(1.3f);
        EndRoll();
    }

    IEnumerator ChangeTrackCoroutine(int move, float duration)
    {

        int newTrackIndex = 0;
        float trackChangeIndexDistance;
        float trackIndexPositionBegin;

        float startX;
        float endX;
        float dir;



        bool doStumple = false;

        trackMovement = move;
        trackMovementLast = move;
        trackMovementNext = 0;

        newTrackIndex = trackIndexTarget + move;

        if (newTrackIndex < -1)
        {
            newTrackIndex = -1;
            doStumple = true;
        }
        else if (newTrackIndex > 1)
        {
            newTrackIndex = 1;
            doStumple = true;
        }


        if (doStumple == false)
        {

            trackChangeIndexDistance = Mathf.Abs((float)newTrackIndex - trackIndexPosition);
            trackIndexPositionBegin = trackIndexPosition;
            startX = playerX;
            endX = GetTrackX(newTrackIndex);

            dir = Mathf.Sign((float)newTrackIndex - trackIndexTarget);



            //startRotation = this.characterRotation;
            /*
            if (move == -1 && IsGrounded == true)
            {
                //playerAnimator.Play("strafeLeft", 0, 0);
            }
            else if (move == 1 && IsGrounded == true)
            {
                //playerAnimator.Play("strafeRight", 0, 0);
            }
            */

            if (move == -1)
            {
                GameGlobals.Instance.PlayerPlaySound("left");
            }
            else if (move == 1)
            {
                GameGlobals.Instance.PlayerPlaySound("right");
            }


            trackIndexTarget = newTrackIndex;
            changeLine = ShortTween.Play(trackChangeIndexDistance * duration, (float t) =>
            {
                trackIndexPosition = Mathf.Lerp(trackIndexPositionBegin, (float)newTrackIndex, t);
                playerX = Mathf.Lerp(startX, endX, t);
                //characterRotation = Bell(t) * dir * characterAngle + Mathf.Lerp(startRotation, 0f, t);
                //player.transform.localRotation = Quaternion.Euler(0f, characterRotation, 0f);

            });
            StartCoroutine(changeLine);

        }
        else
        {
            //TODO: Em de xocar.;
            //camera.Shake(); //Fem un shake a la camara per simular que xoquem

        }

        trackMovement = 0;
        trackIndex = newTrackIndex;

        if (trackMovementNext != 0)
        {

            //StartCoroutine();
            //changeLine = ChangeTrackCoroutine(trackMovementNext, duration);
            //StartCoroutine(changeLine);
        }


        changeLaneTrig = false;
        yield return 0;


    }

    public float Bell(float x)
    {
        return Mathf.SmoothStep(0f, 1f, 1f - Mathf.Abs(x - 0.5f) / 0.5f);
    }

    public float GetTrackX(int trackIndex)
    {
        switch (trackIndex)
        {
            case -1:
                return -trackSpacing;
            case 0:
                return 0;
            case 1:
                return trackSpacing;
        }

        return 0;
    }

   

    private void OnCharacterColliderEnterTrigger(Collider collider)
    {
        StartCoroutine(OnCharacterColliderEnter(collider));
    }

    private IEnumerator OnCharacterColliderEnter(Collider collider)
    {

        GameObject collidedObject = null;

        if (isGameOver == true) yield break;

        if (collider.transform.parent != null)
        {
            if (collider.transform.parent.gameObject != null)
            {
                collidedObject = collider.transform.parent.gameObject;
            }
        }

        //-------------------------------------------------------------------------------

        switch (collider.tag)
        {
        
            case "powerup":

                if (collidedObject != null)
                {
                   
                    if (collidedObject.tag == "Coin")
                    {


                        Coin currentCoin = collidedObject.gameObject.GetComponent<Coin>();
                        if (currentCoin != null)
                        {
                            currentCoin.pickUp();
                        }


                    }
                }

                break;
            case "instant-die":
                this.DoStumble(collidedObject, StumbleLocation.FrontMiddle);
                break;
            case "obstacle":

                //Debug.Log(trackMovementLast);
                //Debug.Log(collider.name);

                int lane;

                int[] hits = analizeHit(collider);

                HitLocation tx = (HitLocation)hits[0];
                HitLocation ty = (HitLocation)hits[1];
                HitLocation tz = (HitLocation)hits[2];

                float colliderCenter = (collider.bounds.min.x + collider.bounds.max.x) / 2f;
                float x = player.transform.position.x;


                if (x < colliderCenter)
                {
                    lane = 1;
                }
                else if (x > colliderCenter)
                {
                    lane = -1;
                }
                else
                {
                    lane = 0;
                }

//                Debug.Log(lane);
                bool flag = (lane == 0) || (trackMovementLast == lane);
                bool flag2 = characterCollider.bounds.center.z < collider.bounds.min.z;
                bool flag3 = ((tz == HitLocation.Before) && !flag2) && flag;

                // Debug.Log(collider.name + " - flag:" + flag + " flag2:" + flag2 + " flag3:" + flag3);



                if ((tz == HitLocation.ZMiddle) || flag3)
                {
                    // SIDES

                    if (trackMovementLast != 0) 
                    {
                        // Lane Fix
                       //StopAllCoroutines();
                        StopCoroutine(changeLine);
                       // Debug.Log(changeLine);
                        ForceChangeTrack(-this.trackMovementLast, 0);
                        //player.transform.position = new Vector3(-6, player.transform.position.y, player.transform.position.z);
                       //player.transform.localRotation = Quaternion.Euler(0f, characterRotation, 0f);
                    }

                    switch (tx)
                    {
                        case HitLocation.Left:
                        
                            this.DoStumble(collidedObject, StumbleLocation.LeftSide);

                            break;

                        case HitLocation.Right:

                            this.DoStumble(collidedObject, StumbleLocation.RightSide);
                            break;
                    }

                }
                else if ((tx == HitLocation.XMiddle) || (trackMovementLast == 0))
                {
                    if (tz == HitLocation.Before)
                    {

                        if (ty == HitLocation.Lower)
                        {
                            //Debug.Log(collider.name + " Front Lower");
                            this.DoStumble(collidedObject, StumbleLocation.FrontLower);
                        }
                        else if (ty == HitLocation.YMiddle)
                        {
                            //Debug.Log(collider.name + " Front Middle");
                            this.DoStumble(collidedObject, StumbleLocation.FrontMiddle);

                        }
                        else if (ty == HitLocation.Upper)
                        {
                            //Debug.Log(collider.name + " Front Upper");
                            this.DoStumble(collidedObject, StumbleLocation.FrontUpper);
                        }
                    }
                }
                else
                {

                    if (tx == HitLocation.Left && tz == HitLocation.Before)
                    {
                        //Debug.Log(collider.name + " Front Left Corner");
                        this.DoStumble(collidedObject, StumbleLocation.FrontLeftCorner);
                    }

                    if (tx == HitLocation.Right && tz == HitLocation.Before)
                    {
                        //Debug.Log(collider.name + " Front Right Corner");
                        this.DoStumble(collidedObject, StumbleLocation.FrontRightCorner);
                    }

                    if (tx == HitLocation.Left && tz == HitLocation.After)
                    {

                        ForceChangeTrack(-this.trackMovementLast, 0.5f);

                        //Debug.Log(collider.name + " Back Left Corner");
                        this.DoStumble(collidedObject, StumbleLocation.BackLeftCorner);
                    }

                    if (tx == HitLocation.Right && tz == HitLocation.After)
                    {

                        ForceChangeTrack(-this.trackMovementLast, 0.5f);

                        //Debug.Log(collider.name + " Back Right Corner");
                        this.DoStumble(collidedObject, StumbleLocation.BackRightCorner);

                    }

                }



                break;

        }

        yield break;
    }

   
    //Analitzem on choca per detectar si es un cop o es mor
    private int[] analizeHit(Collider collider)
    {

        int[] results = new int[3];

        int resulyX = 0;
        int resulyY = 1;
        int resulyZ = 2;

        Bounds characterBounds = characterCollider.bounds;
        Bounds obstacleBounds = collider.bounds;

        // X ---------------------------------------------------------------------------------

        float colliderXBoundsMin = Mathf.Max(characterBounds.min.x, obstacleBounds.min.x);
        float colliderXBoundsMax = Mathf.Min(characterBounds.max.x, obstacleBounds.max.x);

        float xBoundsTotal = (colliderXBoundsMin + colliderXBoundsMax) * hitXOffset;
        float xDirection = xBoundsTotal - obstacleBounds.min.x;

        if (xDirection <= obstacleBounds.size.x - laneWidth * hitMinOffset)
        {
            resulyX = (xDirection >= laneWidth * hitMinOffset ? (int)HitLocation.XMiddle : (int)HitLocation.Left);
        }
        else
        {
            resulyX = (int)HitLocation.Right;
        }


        // Y ---------------------------------------------------------------------------------

        float colliderYBoundsMin = Mathf.Max(characterBounds.min.y, obstacleBounds.min.y);
        float colliderYBoundsMax = Mathf.Min(characterBounds.max.y, obstacleBounds.max.y);

        float yBoundsTotal = (colliderYBoundsMin + colliderYBoundsMax) * hitXOffset;
        float yDirection = (yBoundsTotal - characterBounds.min.y) / characterBounds.size.y;

        if (yDirection >= hitMinOffset)
        {
            resulyY = (yDirection >= hitMaxOffset ? (int)HitLocation.Upper : (int)HitLocation.YMiddle);
        }
        else
        {
            resulyY = (int)HitLocation.Lower;
        }

        // Z ---------------------------------------------------------------------------------

        Vector3 playerPosition = player.transform.position;

        resulyZ = (int)HitLocation.ZMiddle;
        if (playerPosition.z > (obstacleBounds.max.z - (((obstacleBounds.max.z - obstacleBounds.min.z) <= hitZOffset) ? ((obstacleBounds.max.z - obstacleBounds.min.z) * hitXOffset) : obstacleCornerOffset)))
        {
            resulyZ = (int)HitLocation.After;
        }
        if (playerPosition.z < (obstacleBounds.min.z + obstacleCornerOffset))
        {
            resulyZ = (int)HitLocation.Before;
        }


        // Result Addresing

        results[0] = resulyX;
        results[1] = resulyY;
        results[2] = resulyZ;
        return results;

    }


    private void DoStumble(GameObject obstacle, StumbleLocation location)
    {
    
       

        if (disableStumpleOnTrackBorders == true)
        {
            if (location == StumbleLocation.LeftTrackBorder)
            {
                return;
            }
            if (location == StumbleLocation.RightTrackBorder)
            {
                return;
            }
        }


        camera.Shake();


        bool gameOver = false;

        if (GameGlobals.isInCriticalState == false)
        {
            StartCoroutine(GameGlobals.Instance.CriticalState(GameGlobals.Instance.criticalStateSeconds));
            GameGlobals.Instance.PlayerPlaySound("PlayerStumple");
            GameGlobals.Instance.PlayerPlaySound("ouch");
            doAnEffect(EffetcType.StumplePuff);

            print(location);
                  
            switch (location)
            {
                case StumbleLocation.FrontLower:

                    // Jump and passable objects
                    gameOver = true;

                    break;
                case StumbleLocation.FrontMiddle:
                    // -- Game Over
                    gameOver = true;
                    break;
                case StumbleLocation.FrontUpper:
                    // -- Game Over
                    gameOver = true;
                    break;
                case StumbleLocation.LeftSide:
                    gameOver = false;
                    break;
                case StumbleLocation.RightSide:
                    gameOver = false;
                    break;
                case StumbleLocation.LeftTrackBorder:
                    gameOver = false;
                    break;
                case StumbleLocation.RightTrackBorder:
                    gameOver = false;
                    break;
                case StumbleLocation.FrontLeftCorner:
                    gameOver = false;
                    break;
                case StumbleLocation.FrontRightCorner:
                    gameOver = false;
                    break;
                case StumbleLocation.BackLeftCorner:
                    gameOver = false;
                    break;
                case StumbleLocation.BackRightCorner:
                    gameOver = false;
                    break;
            }

        }
        else if (GameGlobals.isInCriticalState == true)
        {
            gameOver = true;
        }

        if (gameOver == true)
        {
            StopAllCoroutines();
            isGameOver = true;
            playerAnimator.Play("death", 0, 0);
            doAnEffect(EffetcType.DeathPuff);
            GameGlobals.Instance.PlayerPlaySound("ouch");
            GameGlobals.Instance.PlayerPlaySound("death");
            GameGlobals.Instance.GameOver();
            //doGameOver(obstacle);
        }


    }
  

    public void doAnEffect(EffetcType effetc)
    {
        GameObject effetcObject = null;
        Vector3 effetcPos = player.transform.position;

        foreach (GameObject eObject in effetcs)
        {

            EffetcController eController = eObject.GetComponent<EffetcController>();

            if (eController != null)
            {
                switch (effetc)
                {
                    case EffetcType.None:
                        break;
                    case EffetcType.LandingPuff:
                        if (eController.ID.Equals("puff"))
                        {
                            effetcObject = eObject;
                            effetcPos = new Vector3(effetcPos.x, 1.0f, effetcPos.z);
                        }
                        break;
                    case EffetcType.DeathPuff:
                        if (eController.ID.Equals("death"))
                        {
                            effetcObject = eObject;
                            effetcPos = new Vector3(effetcPos.x, effetcPos.y + 3.0f, effetcPos.z + 3.0f);
                        }
                        break;
                    case EffetcType.StumplePuff:
                        if (eController.ID.Equals("stumple"))
                        {
                            effetcObject = eObject;
                            effetcPos = new Vector3(effetcPos.x, effetcPos.y + 3.0f, effetcPos.z);
                        }
                        break;
                }
            }
        }

        if (effetcObject != null)
        {
            GameObject createdEffetc = (GameObject)GameObject.Instantiate(effetcObject, effetcPos, Quaternion.identity);
            createdEffetc.transform.parent = effetcsRoot.transform;


        }

    }
}

