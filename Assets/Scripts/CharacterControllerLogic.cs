﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerLogic : MonoBehaviour
{
    [SerializeField]
    private float directionDampTime = .25f; //delay direction
    [SerializeField]
    private float directionSpeed = 3.0f;// how fast u want the character to turn

    private float speedDampTime = .05f;

    //ref to game camera
    [SerializeField]
    private ThirdPersonCamera gamecam;

    //movement vars
    public float speed;
    private float direction = 0.0f;
    private float char_angle = 0.0f;

    private float horizontal = 0.0f;
    private float vertical = 0.0f;
    [SerializeField]
    private float rotationDegreePerSecond = 120f; //how many sec it takes for char to rotate in full 360 if holding r /l

    //Animator state machine vars
    [SerializeField]
    private Animator animator;

    private AnimatorStateInfo stateInfo;
    private AnimatorTransitionInfo transInfo;

    public float LocomotionThreshold { get { return 0.2f; } }
    private int m_LocomotionId = 0; 

    private int m_LocomotionPivotLId = 0;
    private int m_LocomotionPivotRId = 0;
    private int m_LocomotionPivotLTransId = 0;
    private int m_LocomotionPivotRTransId = 0;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        m_LocomotionId = Animator.StringToHash("Locomotion");

        m_LocomotionPivotLId = Animator.StringToHash("LocomotionPivotLeft");
        m_LocomotionPivotRId = Animator.StringToHash("LocomotionPivotRight");
        m_LocomotionPivotLTransId = Animator.StringToHash("Locomotion -> LocomotionPivotLeft");
        m_LocomotionPivotRTransId = Animator.StringToHash("Locomotion -> LocomotionPivotRight");

        speed = 0.0f;//init speed

        //todo: look what it does
        if (animator.layerCount >= 2);
        {
            animator.SetLayerWeight(1, 1);
        }


    }

    // Update is called once per frame
    void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0); //set state info in each update

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        char_angle = 0f;
        direction = 0f;

        stickToCamSpace(this.transform, gamecam.transform, ref direction, ref speed, ref char_angle, isInPivot()); //pseudo returning two values

        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
        animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);

        if (speed > LocomotionThreshold)    // Dead zone
        {
            if (!isInPivot())
            {
                animator.SetFloat("Angle", char_angle);
            }
        }
        if (speed < LocomotionThreshold && Mathf.Abs(horizontal) < 0.05f)    // Dead zone
        {
            animator.SetFloat("Direction", 0f);
            animator.SetFloat("Angle", 0f);
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //Debug.Log("dir "+ direction+ "hor " + horizontal);

        //rotatin char model
        if (isInLocomotion() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0)))
        {
            //rot around y axis based on stick hor movement (rotate faster if stick is pushed all the way down)
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime); //turns eulerangle vec to quaternion and achieve frame rate indep
            this.transform.rotation = (this.transform.rotation * deltaRotation);
        }

    }


    public void stickToCamSpace(Transform root, Transform camera, ref float directionOut, ref float speedOut, ref float angleOut, bool isInPivot)
    {
        Vector3 rootDirection = root.forward;
        Vector3 stickDirection = new Vector3(horizontal, 0, vertical);

        speedOut = stickDirection.sqrMagnitude;

        //get camera dir
        Vector3 cameraDirection = camera.forward;
        cameraDirection.y = 0.0f; //kill y value

        //angle angle the camera is rotated proportional to the global forward vector (its initial dir)
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, cameraDirection);

        //convert joystick input to worldspace coordinate
        Vector3 moveDirection = referentialShift * stickDirection;

        //detects if stick is to the left or right of character's forward vector using the cross product (left-hand rule)
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

    
        float angleRootToMove = Vector3.Angle(rootDirection, moveDirection) * (axisSign == Vector3.zero ? 0 : -Mathf.Sign(axisSign.y));

        if(!isInPivot)
        angleOut = angleRootToMove; //retrieve angle val

        angleRootToMove /= 180f; //normalize the angle value ( to have it between 1 and -1)


        directionOut = angleRootToMove * directionSpeed; //haviing a speed to our direction (will make turn faster)

    }


    public bool isInLocomotion()
    {

        return stateInfo.shortNameHash == m_LocomotionId;//query against against the state machine and look for a hash id
    }


    public bool isInPivot()
    {
        return stateInfo.shortNameHash == m_LocomotionPivotLId ||
            stateInfo.shortNameHash == m_LocomotionPivotRId ||
            transInfo.nameHash == m_LocomotionPivotLTransId ||
            transInfo.nameHash == m_LocomotionPivotRTransId;
    }


}
