using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {

	private const float VELOCITY_MULTIPLIER = 80.0f;

    public GameObject Planet;
	public List<GameObject> Planets;
    public float grabRadius;
    public bool keepUpright;

    OVRInput.Controller grabbedBy = OVRInput.Controller.None;
    OVRInput.RawButton grabbedByButton = OVRInput.RawButton.None;
    Vector3 localGrabOffset;
    Quaternion localGrabRotation;

	private SmoothedVector3 _velocitySmoothedVector = new SmoothedVector3 (10);

    private Rigidbody _rigidbody;
    private SpringJoint _joint;

	public bool debugVelocity = false;

    private void Awake()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _joint = gameObject.GetComponent<SpringJoint>();
		_joint.minDistance = 0.55f;
		_joint.maxDistance = 1.2f;
		_joint.enableCollision = true;

		if (keepUpright)
			_rigidbody.angularDrag = 0.2f;
    }

    // Use this for initialization
    void Start()
    {
		_rigidbody.AddForce (new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)), ForceMode.Impulse);
    }
		
    // Update is called once per frame

    void Update()
    {
		Vector3 oldPosition = transform.position;
		Quaternion oldRotation = transform.rotation;

        if (_attractor != null)
        {
            transform.position = Vector3.Lerp(transform.position, _attractor.position, 0.1f);
			_velocitySmoothedVector.AddReading(transform.position - oldPosition);
			return;
        }

        CheckGrab(OVRInput.Controller.LTouch, OVRInput.RawButton.LHandTrigger);
        CheckGrab(OVRInput.Controller.RTouch, OVRInput.RawButton.RHandTrigger);

		if (grabbedBy != OVRInput.Controller.None) {

			Vector3 handPos = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerPosition (grabbedBy));
			Quaternion handRot = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerRotation (grabbedBy));
			transform.position = handPos + handRot * localGrabOffset;
			transform.rotation = handRot * localGrabRotation;

			_velocitySmoothedVector.AddReading (transform.position - oldPosition);

		} else {
			
			if (keepUpright && _rigidbody.velocity.magnitude > 0) {

				Vector3 upward = transform.position - Planet.transform.position;
				Vector3 forward = Vector3.ProjectOnPlane (_rigidbody.velocity * 10, upward);

				if (forward.magnitude < 0.01f) {
					forward = transform.rotation * Vector3.forward;
				}
					
				Quaternion lookTo = Quaternion.LookRotation (forward, upward);

				transform.rotation = Quaternion.RotateTowards (transform.rotation, lookTo, Mathf.Max(_rigidbody.velocity.magnitude, 0.5f));
				if (Quaternion.Angle (transform.rotation, lookTo) <= 1f)
					transform.rotation = lookTo;
			}

			if (Planets.Count > 1) {

				float minDistance = Vector3.Distance (transform.position, Planet.transform.position);

				foreach (GameObject planet in Planets) {
					
					float distance = Vector3.Distance (transform.position, planet.transform.position);
					if (distance < minDistance) {

						minDistance = distance;
						Planet = planet;

						_joint.connectedBody = Planet.GetComponent<Rigidbody>();
					}
				}
			}
		}

        if (grabbedByButton != OVRInput.RawButton.None && OVRInput.GetUp(grabbedByButton))
        {
            // release
            grabbedBy = OVRInput.Controller.None;
            grabbedByButton = OVRInput.RawButton.None;

			_rigidbody.AddForce(_velocitySmoothedVector.average * VELOCITY_MULTIPLIER, ForceMode.Impulse);

			Vector3 torgue = new Vector3 (_velocitySmoothedVector.average.z, _velocitySmoothedVector.average.y, -_velocitySmoothedVector.average.x);
			_rigidbody.AddTorque (torgue, ForceMode.Impulse);

			_velocitySmoothedVector.Clear();
        }
    }

    private void FixedUpdate()
    {
		if (debugVelocity)
			Debug.Log (_rigidbody.velocity);
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
                _velocitySmoothedVector.Clear();
            }
        }
    }

    private Transform _attractor;
    public void Attract(bool isAttracting, Transform attractor = null)
    {
		_rigidbody.isKinematic = isAttracting;

		if (isAttracting == false) 
		{
			if (Vector3.Distance (transform.position, _attractor.position) > 0.2f) 
			{
				_rigidbody.AddForce(_velocitySmoothedVector.average * VELOCITY_MULTIPLIER / 10.0f, ForceMode.Impulse);
				_velocitySmoothedVector.Clear();
			}
		}

        _attractor = attractor;
    }
}
