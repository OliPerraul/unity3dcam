using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{

    [SerializeField]
    private float distanceAway; //fixed distance from the back of the character where we want to position the camera
    [SerializeField]
    private float distanceUp; //fixed distance up from the character to where we want to set the cam

    [SerializeField]
    private Transform followXForm; //transform of a marker for the character
     
    [SerializeField] ///a way to look at the top of the char instead of lookin at its feet
    private Vector3 height_offset; // height from the top of the char that we want to look at


    //smooth damp vars
    //
    [SerializeField]
    private float camSmoothDampTime = 0.1f;
    private Vector3 velocityCamSmooth = Vector3.zero;

    [SerializeField]
    private float lookDirDampTime = 0.1f;
    private Vector3 lookDirVelocity = Vector3.zero;
    //
    
    [SerializeField]
    private GameObject beta;
    private CharacterControllerLogic char_logic; //ref to character logic

    private Vector3 curr_lookDir;//dir were currentyl looking into
    private Vector3 lookDir; //direction we want to look into
    private Vector3 targetPosition;

    //inputs
    private float horizontal = 0.0f;
    private float vertical = 0.0f;

    //default cam state
    private CamStates camState = CamStates.BEHIND;

    //camera states
    public enum CamStates
    {
        BEHIND, FIRSTPERSON, TARGET, FREE
    }



    // Use this for initialization
    void Start()
    {
        followXForm = GameObject.FindWithTag("Player").transform;
        char_logic = beta.GetComponent<CharacterControllerLogic>();

        //init cam movement vars
        curr_lookDir = followXForm.forward;
        
    }

    // Update is called once per frame
    void Update()
    {
        //get inputs
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void LateUpdate()
    {
        Vector3 characterOffset = followXForm.position + height_offset; //distance from char follow transform + some height

        //determine cam state
        if (Input.GetAxis("Target") > 0.01)
        {
            camState = CamStates.TARGET;

        }
        else
        {
            camState = CamStates.BEHIND;

        }
        
        switch (camState)
        {
            //case CamStates.BEHIND:
            //    calc dir from camera to player, kill y and normalize to give a valid direction with unit magnitude
            //    lookDir = characterOffset - this.transform.position;
            //    lookDir.y = 0; //kill y val
            //    lookDir.Normalize();

            //    targetPosition = characterOffset - lookDir * distanceAway;
            //    break;

            case CamStates.BEHIND:

                if (char_logic.speed > char_logic.LocomotionThreshold && char_logic.isInLocomotion() && !char_logic.isInPivot())
                {
                    //http://chortle.ccsu.edu/vectorlessons/vch09/vch09_6.html the range of the dot product of 2 unit vector is between 0 and 1
                    lookDir = Vector3.Lerp(followXForm.right * (horizontal == 0 ? 0 : -Mathf.Sign(horizontal)), followXForm.forward * (vertical == 0 ? 0 : Mathf.Sign(vertical)), Mathf.Abs(Vector3.Dot(this.transform.forward, followXForm.forward)));

                    // Calculate direction from camera to player, kill Y, and normalize to give a valid direction with unit magnitude
                    curr_lookDir = Vector3.Normalize(characterOffset - this.transform.position);
                    curr_lookDir.y = 0;

                    // Damping makes it so we don't update targetPosition while pivoting; camera shouldn't rotate around player
                    curr_lookDir = Vector3.SmoothDamp(curr_lookDir, lookDir, ref lookDirVelocity, lookDirDampTime);
                }

                targetPosition = characterOffset + followXForm.up * distanceUp - Vector3.Normalize(curr_lookDir) * distanceAway;

                break;


            case CamStates.TARGET:

                //calc dir from camera to player, kill y and normalize to give a valid direction with unit magnitude
                lookDir = followXForm.forward;
                lookDir.y = 0; //kill y val
                lookDir.Normalize();

                targetPosition = characterOffset - lookDir * distanceAway;

                break;

        }
            
        //do regardless of state
        //compensateForWalls(characterOffset, ref targetPosition);
        smoothPosition(transform.position, targetPosition);//smooth
        //make sure cam faces character
        transform.LookAt(followXForm);

    }



    /// <summary>  
    /// making a smooth transition between cam curr pos and pos it wants to be in
    /// </summary>  
    void smoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        //making a smooth transition between cam curr pos and pos it wants to be in
        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    /// <summary>  
    ///  Collide with walls to prevent clipping
    /// </summary>  
    void compensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        Debug.DrawLine(fromObject, toTarget, Color.cyan);
        //compensate for walls behind cam
        RaycastHit wallHit = new RaycastHit();

        // Returns true if there is any collider intersecting the line between start and end.
        if (Physics.Linecast(fromObject, toTarget, out wallHit))
        {
          //  Debug.DrawRay(wallHit.point, Vector3.left, Color.red);

            //change the targetPos if needed to the impact point in world space where the ray hit the collider.
            toTarget = new Vector3(wallHit.point.x, wallHit.point.y, wallHit.point.z);
        }

    }



}
