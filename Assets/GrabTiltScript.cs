using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static float AngleTo(this Vector3 self, Vector3 other)
    {
        return AngleTo_Raw(self.normalized, other.normalized) * Mathf.Rad2Deg;
    }

    public static float AngleTo_Raw(this Vector3 unitSelf, Vector3 unitOther)
    {
        return Mathf.Acos(Vector3.Dot(unitSelf, unitOther));
    }

    public static Vector3 SnapToAxis(this Vector3 self, Vector3 axis)
    {
        return SnapToAxis_PreNormalized(self, axis.normalized);
    }

    public static Vector3 SnapToAxis_PreNormalized(this Vector3 self, Vector3 unitAxis)
    {
        return Vector3.Dot(self, unitAxis) * unitAxis;
    }

    public static Vector3 SnapToPlane(this Vector3 self, Vector3 planeNormal)
    {
        return SnapToPlane_PreNormalized(self, planeNormal.normalized);
    }

    public static Vector3 SnapToPlane_PreNormalized(this Vector3 self, Vector3 unitPlaneNormal)
    {
        return self - (Vector3.Dot(self, unitPlaneNormal) * unitPlaneNormal);
    }

    public static float GetTwist(this Quaternion q, Vector3 axis)
    {
        axis.Normalize();

        Vector3 orthonormal = Vector3.Cross(Vector3.up, axis);
        Vector3 transformed = q * orthonormal;

        //project transformed vector onto plane
        Vector3 flattened = transformed - (Vector3.Dot(transformed, axis) * axis);
        flattened.Normalize();

        //get angle between original vector and projected transform to get angle around normal
        return orthonormal.AngleTo_Raw(flattened) * Mathf.Rad2Deg;
    }

    public static Vector3 WorldToLocal(this Transform self, Vector3 worldVector)
    {
        return self.InverseTransformPoint(worldVector);
    }

    public static Vector3 LocalToWorld(this Transform self, Vector3 worldVector)
    {
        return self.TransformPoint(worldVector);
    }

    public static Quaternion WorldToLocal(this Transform self, Quaternion worldRotation)
    {
        return Quaternion.Inverse(self.rotation) * worldRotation;
    }

    public static Quaternion LocalToWorld(this Transform self, Quaternion localRotation)
    {
        return self.rotation * localRotation;
    }

    public static string ToLongString(this Vector3 self)
    {
        return string.Format("[{0},{1},{2}]", self.x, self.y, self.z);
    }
}

public class GrabTiltScript : MonoBehaviour
{
    Rigidbody rigidBody;
    Vector3 localGrabOffsetL;
    Vector3 localGrabOffsetR;
    Vector3 localQuatGrabOffsetR;
    Vector3 grabOffsetL;
    Vector3 grabOffsetR;
    Quaternion baseRotationL;
    Quaternion baseRotationR;
    bool grabbedL;
    bool grabbedR;
    float twistTest;
    public float grabRadius;
    Quaternion avelocity;

    // Use this for initialization
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
        {
            Vector3 LTouchPos = OVRManager.instance.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
            grabOffsetL = LTouchPos - transform.position;
            if (grabOffsetL.sqrMagnitude < grabRadius * grabRadius)
            {
                grabbedL = true;
                localGrabOffsetL = transform.WorldToLocal(LTouchPos);
                baseRotationL = transform.rotation;
            }
        }
        else if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
        {
            grabbedL = false;
            if (grabbedR)
            {
                Vector3 RTouchPos = OVRManager.instance.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
                grabOffsetR = RTouchPos - transform.position;
                localGrabOffsetR = transform.WorldToLocal(RTouchPos);
                baseRotationR = transform.rotation;
            }
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
        {
            Vector3 RTouchPos = OVRManager.instance.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
            grabOffsetR = RTouchPos - transform.position;
            if (grabOffsetR.sqrMagnitude < grabRadius * grabRadius)
            {
                grabbedR = true;
                localGrabOffsetR = transform.WorldToLocal(RTouchPos);
                baseRotationR = transform.rotation;
            }
        }
        else if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger))
        {
            grabbedR = false;
            if (grabbedL)
            {
                Vector3 LTouchPos = OVRManager.instance.transform.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
                grabOffsetL = LTouchPos - transform.position;
                localGrabOffsetL = transform.WorldToLocal(LTouchPos);
                baseRotationL = transform.rotation;
            }
        }

        if (grabbedL || grabbedR)
        {
            Quaternion oldRotation = transform.rotation;

            if (grabbedL && grabbedR)
            {
                SetRotation(OVRInput.Controller.RTouch);

                //clamp the L offset to the plane of R
                OVRInput.Controller controller = OVRInput.Controller.RTouch;
                OVRInput.Controller otherController = (controller == OVRInput.Controller.LTouch) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

                Vector3 BRlocalspaceTwistOriginal = (controller == OVRInput.Controller.LTouch) ? localGrabOffsetL.SnapToPlane(localGrabOffsetR).normalized : localGrabOffsetR.SnapToPlane(localGrabOffsetL).normalized;
                Vector3 BRlocalspaceCurrentTwisterPos = transform.WorldToLocal(OVRInput.GetLocalControllerPosition(controller));
                Vector3 BRlocalspaceCurrentAxis = transform.WorldToLocal(OVRInput.GetLocalControllerPosition(otherController).normalized);
                Vector3 BRlocalspaceTwistCurrent = BRlocalspaceCurrentTwisterPos.SnapToPlane_PreNormalized(BRlocalspaceCurrentAxis).normalized;
                float BRtwistAngle = BRlocalspaceTwistCurrent.AngleTo_Raw(BRlocalspaceTwistOriginal) * Mathf.Rad2Deg;
                if (Vector3.Dot(Vector3.Cross(BRlocalspaceCurrentAxis, BRlocalspaceTwistOriginal), BRlocalspaceTwistCurrent) < 0)
                    BRtwistAngle = -BRtwistAngle;

                Vector3 localspaceTwistOriginal = localGrabOffsetL.SnapToPlane(localGrabOffsetR).normalized;
                Vector3 localspaceCurrentL = transform.WorldToLocal(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch));
                Vector3 localspaceCurrentR = transform.WorldToLocal(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch)).normalized;
                Vector3 localspaceTwistCurrent = localspaceCurrentL.SnapToPlane_PreNormalized(localspaceCurrentR).normalized;
                float twistAngle = localspaceTwistCurrent.AngleTo_Raw(localspaceTwistOriginal) * Mathf.Rad2Deg;

                if (Vector3.Dot(Vector3.Cross(localspaceCurrentR, localspaceTwistOriginal), localspaceTwistCurrent) < 0)
                    twistAngle = -twistAngle;

                if (twistAngle != BRtwistAngle)
                {
                    int breakhere = 0;
                    breakhere++;
                }

                //grabOffsetL - Vector3.Dot(grabOffsetL, grabOffsetR)
                Vector3 twistAxis = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) - transform.position;

                transform.rotation = Quaternion.AngleAxis(twistAngle, twistAxis) * transform.rotation;
                //Quaternion.Lerp(
                //    Quaternion.AngleAxis(twistAngle, twistAxis) * transform.rotation,
                //    Quaternion.AngleAxis(twistAngle, twistAxis) * transform.rotation,
                //    0.5f);
            }
            else if (grabbedL)
            {
                SetRotation(OVRInput.Controller.LTouch);
            }
            else // (grabbedR)
            {
                SetRotation(OVRInput.Controller.RTouch);
            }

            avelocity = Quaternion.Lerp(avelocity, Quaternion.Inverse(oldRotation) * transform.rotation, 0.1f);
        }
        else
        {
            grabbedL = false;
            grabbedR = false;
            transform.rotation *= Quaternion.Lerp(Quaternion.identity, avelocity, 0.6f);
        }

    }

    void SetRotation(OVRInput.Controller controller)
    {
        Vector3 currentGrabOffset = OVRInput.GetLocalControllerPosition(controller) - transform.position;

        //rigidBody.angularVelocity = new Vector3(100, 0, 0);
        //rigidBody.angularVelocity = (Quaternion.Inverse(transform.rotation) * (Quaternion.FromToRotation(grabOffset, currentGrabOffset) * baseRotation)).eulerAngles / Time.fixedDeltaTime;

        transform.rotation = Quaternion.FromToRotation(
                controller == OVRInput.Controller.LTouch? grabOffsetL: grabOffsetR,
                currentGrabOffset
            )
            * (controller == OVRInput.Controller.LTouch ? baseRotationL : baseRotationR);

        /*Quaternion newGrabRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);

        cumulativeTwist += newGrabRotation.GetTwist(currentGrabOffset) - grabRotation.GetTwist(currentGrabOffset);
        transform.rotation = Quaternion.AngleAxis(cumulativeTwist, currentGrabOffset.normalized) * transform.rotation;

        grabRotation = newGrabRotation;*/
    }
}
