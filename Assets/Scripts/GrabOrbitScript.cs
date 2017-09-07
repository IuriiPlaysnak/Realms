using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabOrbitScript : MonoBehaviour
{
    public GameObject Planet;
    public float grabRadius;
    public bool keepUpright;
    public float initialSpeed;

    OVRInput.Controller grabbedBy = OVRInput.Controller.None;
    OVRInput.RawButton grabbedByButton = OVRInput.RawButton.None;
    Vector3 localGrabOffset;
    Quaternion localGrabRotation;
    SmoothedVector3 smoothedGrabVelocity = new SmoothedVector3(10);
    Quaternion orbitRotation = Quaternion.identity;
    Quaternion spin = Quaternion.identity;
    float unfreezeTimestamp;
    public Vector3 tractorBeamVelocity;

    // Use this for initialization
    void Start()
    {
        orbitRotation = Quaternion.AngleAxis(initialSpeed, Vector3.Cross(transform.forward, (transform.position - Planet.transform.position)));
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
            grabbedByButton = OVRInput.RawButton.None;

            float orbitRadius = (transform.position - Planet.transform.position).magnitude;
            Vector3 axis = Vector3.Cross(transform.position - Planet.transform.position, transform.position + smoothedGrabVelocity.average - Planet.transform.position);
            orbitRotation = Quaternion.AngleAxis(smoothedGrabVelocity.average.magnitude / (orbitRadius*Time.fixedDeltaTime), axis);
        }

        if (grabbedBy != OVRInput.Controller.None)
        {
            Vector3 oldPosition = transform.position;
            Quaternion oldRotation = transform.rotation;

            Vector3 handPos = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(grabbedBy));
            Quaternion handRot = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerRotation(grabbedBy));
            transform.position = handPos + handRot * localGrabOffset;
            transform.rotation = handRot * localGrabRotation;

            spin = transform.rotation * Quaternion.Inverse(oldRotation);

            smoothedGrabVelocity.AddReading(transform.position - oldPosition);
        }
        else if (unfreezeTimestamp != 0 && unfreezeTimestamp < Time.time)
        {
            unfreezeTimestamp = 0;
            float orbitRadius = (transform.position - Planet.transform.position).magnitude;
            Vector3 axis = Vector3.Cross(transform.position - Planet.transform.position, transform.position + tractorBeamVelocity - Planet.transform.position);
            orbitRotation = Quaternion.AngleAxis(tractorBeamVelocity.magnitude / (orbitRadius * Time.fixedDeltaTime), axis);
        }
    }

    private void FixedUpdate()
    {
        if (grabbedBy == OVRInput.Controller.None)
        {
            Vector3 oldPosition = transform.position;
            if (unfreezeTimestamp < Time.time)
            {
                transform.position = Planet.transform.position + orbitRotation * (transform.position - Planet.transform.position);
                if (keepUpright)
                    transform.rotation = Quaternion.LookRotation(transform.position - oldPosition, transform.position - Planet.transform.position);
                else
                    transform.rotation = spin * transform.rotation;
            }
            else
            {
                transform.position += tractorBeamVelocity;
                if (keepUpright)
                    transform.rotation = Quaternion.LookRotation(tractorBeamVelocity, transform.position - Planet.transform.position);
                else
                    transform.rotation = spin * transform.rotation;
            }

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
                localGrabRotation = invTouchRot * transform.rotation;
                smoothedGrabVelocity.Clear();
            }
        }
    }

    public void FreezeOrbitUntil(float UnfreezeTimestamp)
    {
        unfreezeTimestamp = UnfreezeTimestamp;
    }
}
