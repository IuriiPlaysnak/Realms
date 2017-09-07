using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightControls : MonoBehaviour
{
    public float speed;
    Quaternion smoothedRot;
    Vector3 smoothedVel;
    Vector3 oldHandPos;

    SmoothedVector3 handHistory = new SmoothedVector3(1);
    float spawnHoldDuration;

    SmoothedVector3 throwHistory = new SmoothedVector3(4);
    bool thrown = false;

    // Use this for initialization
    void Start ()
    {
        transform.position = new Vector3(0, 0, 1E8f);
    }

    private void Update()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 curHandPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        handHistory.AddReading(curHandPos);
        Vector3 handPos = handHistory.average;

        smoothedRot = Quaternion.Lerp(smoothedRot, OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch), 0.1f);
        Vector3 forward = (transform.position - handPos).normalized;// smoothedRot * Vector3.forward;
        Vector3 right = smoothedRot * Vector3.right;
        Vector3 shipUp = smoothedRot * Vector3.up;

        if (!thrown)
        {
            smoothedVel = Vector3.Lerp(smoothedVel, handPos - transform.position, 0.1f);
        }

        Vector2 stick = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

        if (OVRInput.Get(OVRInput.RawButton.B))
        {
            if ((transform.position - handPos).sqrMagnitude < 0.1f * 0.1f)
            {
                transform.position = handPos;
                throwHistory.AddReading(handPos);
                smoothedVel = (handPos - throwHistory.average) * 0.3f;
                thrown = (throwHistory.average.sqrMagnitude > 0.7f);
            }
            else
            {
                transform.position = handPos + (transform.position - handPos) / 2;
                throwHistory.Clear();
                thrown = false;
            }
        }
        else if(thrown)
        {
            transform.position += smoothedVel;

            if (Mathf.Abs(stick.y) > 0.1f)
            {
                smoothedVel = Quaternion.AngleAxis(stick.y, right) * smoothedVel;
            }
            if (Mathf.Abs(stick.x) > 0.1f)
            {
                smoothedVel = Quaternion.AngleAxis(stick.x, shipUp) * smoothedVel;
            }
        }
        else if (smoothedVel.magnitude > 0.05f)
        {
            transform.position += smoothedVel.normalized*0.01f;
        }
        //transform.position = Vector3.Lerp(transform.position, handPos, followDist);


        //Vector3 shipForward = forward * stick.y + right * stick.x;


//        if (stick.sqrMagnitude > 0.01f)
//        {
//            transform.position += shipForward * speed;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(smoothedVel, shipUp), 0.1f);
        //        }
        oldHandPos = handPos;
    }
}
