using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabThrowScript : MonoBehaviour
{
    public float grabRadius;
    Rigidbody rbody;
    OVRInput.Controller grabbedBy = OVRInput.Controller.None;
    OVRInput.RawButton grabbedByButton = OVRInput.RawButton.None;
    Vector3 localGrabOffset;
    Quaternion localGrabRotation;
    Vector3 grabVelocity;
    SmoothedVector3 smoothedVelocity = new SmoothedVector3(10);

    // Use this for initialization
    void Start ()
    {
        rbody = GetComponent<Rigidbody>();		
	}

    // Update is called once per frame
    void Update()
    {
        CheckGrab(OVRInput.Controller.LTouch, OVRInput.RawButton.LHandTrigger);
        CheckGrab(OVRInput.Controller.RTouch, OVRInput.RawButton.RHandTrigger);

        if (grabbedByButton != OVRInput.RawButton.None && OVRInput.GetUp(grabbedByButton))
        {
            // release
            grabbedBy = OVRInput.Controller.None;
            rbody.velocity = smoothedVelocity.average / Time.fixedDeltaTime;
        }

        if(grabbedBy != OVRInput.Controller.None)
        {
            Vector3 oldPosition = transform.position;

            Vector3 handPos = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(grabbedBy));
            Quaternion handRot = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerRotation(grabbedBy));
            transform.position = handPos + handRot * localGrabOffset;
            transform.rotation = handRot * localGrabRotation;

            smoothedVelocity.AddReading( transform.position - oldPosition );
        }
    }

    void CheckGrab(OVRInput.Controller hand, OVRInput.RawButton button)
    {
        if (OVRInput.GetDown(button))
        {
            Vector3 touchPos = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(hand));
            Vector3 grabOffset = touchPos - transform.position;
            if (grabOffset.sqrMagnitude < grabRadius * grabRadius)
            {
                Quaternion invTouchRot = Quaternion.Inverse(OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerRotation(hand)));

                grabbedBy = hand;
                grabbedByButton = button;
                localGrabOffset = invTouchRot * (transform.position - touchPos);
                localGrabRotation = transform.rotation * invTouchRot;
            }
        }
    }
}
