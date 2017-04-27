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
       
    [SerializeField]
    private float camSmoothDampTime = 0.1f;
    private Vector3 velocityCamSmooth = Vector3.zero;

    private Vector3 lookDir; //dir well be lookin in (to calculate fixed dist from the player no matter what way he is facing)
    private Vector3 targetPosition;


    // Use this for initialization
    void Start ()
    {
        followXForm = GameObject.FindWithTag("Player").transform;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void LateUpdate()
    {
        Vector3 characterOffset = followXForm.position + height_offset; //distance from char follow transform + some height

        //calc dir from camera to player, kill y and normalize to give a valid direction with unit magnitude
        lookDir = characterOffset - this.transform.position;
        lookDir.y = 0; //kill y val
        lookDir.Normalize();
        Debug.DrawRay(this.transform.position, lookDir * distanceAway, Color.green); //draw ray from cam

        targetPosition = characterOffset - lookDir*distanceAway;
        smoothPosition(transform.position, targetPosition);//smooth transition between current pos and target pos
  
        //make sure cam faces character
        transform.LookAt(followXForm);

    }

    void smoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        //making a smooth transition between cam curr pos and pos it wants to be in
        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }
    
    

}
