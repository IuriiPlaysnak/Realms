using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {

	private const float THROW_VELOCITY_MULTIPLIER = 80.0f;
	private const float BEAM_RELEASE_VELOCITY_MULTIPLIER = 8.0f;
	private const OVRInput.Button TRACTOR_BEAM_BUTTON = OVRInput.Button.One;

    public GameObject Planet;
	public List<GameObject> Planets;
    public float grabRadius;
    public bool keepUpright;
	public Vector3 initVelocity;

	[HideInInspector]
	public bool isHitByTractorBeam;

	private OVRInput.Controller _grabbedBy = OVRInput.Controller.None;
	private OVRInput.RawButton _grabbedByButton = OVRInput.RawButton.None;
	private Vector3 _localGrabOffset;
	private Quaternion _localGrabRotation;

	private SmoothedVector3 _velocitySmoothedVector;

    private Rigidbody _rigidbody;
    private SpringJoint _joint;

	private bool _isAttracted;

    private void Awake()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _joint = gameObject.GetComponent<SpringJoint>();
		_joint.minDistance = 0.55f;
		_joint.maxDistance = 1.2f;
		_joint.enableCollision = true;

		if (keepUpright)
			_rigidbody.angularDrag = 0.2f;

		_velocitySmoothedVector = new SmoothedVector3 (10);
    }

    // Use this for initialization
    void Start()
    {
		_rigidbody.AddForce (initVelocity, ForceMode.Impulse);
    }
		
    // Update is called once per frame

    void Update()
    {
		Vector3 oldPosition = transform.position;

		if (_isAttracted) {

			Vector3 controllerPosition = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
			transform.position = Vector3.Lerp(transform.position, controllerPosition, 0.1f);
			_velocitySmoothedVector.AddReading(transform.position - oldPosition);
			return;
		}

        CheckGrab(OVRInput.Controller.LTouch, OVRInput.RawButton.LHandTrigger);
        CheckGrab(OVRInput.Controller.RTouch, OVRInput.RawButton.RHandTrigger);

		if (_grabbedBy != OVRInput.Controller.None) {

			Vector3 handPos = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerPosition (_grabbedBy));
			Quaternion handRot = OVRManager.instance.transform.LocalToWorld (OVRInput.GetLocalControllerRotation (_grabbedBy));
			transform.position = handPos + handRot * _localGrabOffset;
			transform.rotation = handRot * _localGrabRotation;

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

        if (_grabbedByButton != OVRInput.RawButton.None && OVRInput.GetUp(_grabbedByButton))
        {
            // release
            _grabbedBy = OVRInput.Controller.None;
            _grabbedByButton = OVRInput.RawButton.None;

			_rigidbody.AddForce(_velocitySmoothedVector.average * THROW_VELOCITY_MULTIPLIER, ForceMode.Impulse);

			Vector3 torgue = new Vector3 (_velocitySmoothedVector.average.z, _velocitySmoothedVector.average.y, -_velocitySmoothedVector.average.x);
			_rigidbody.AddTorque (torgue, ForceMode.Impulse);

			_velocitySmoothedVector.Clear();
        }
    }

    private void FixedUpdate()
    {
		if (isHitByTractorBeam) {
			
			if (OVRInput.Get(TRACTOR_BEAM_BUTTON)) {
				
				_rigidbody.isKinematic = true;
				_isAttracted = true;

			} else {

				Vector3 controllerPosition = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));
				if (_isAttracted && Vector3.Distance (transform.position, controllerPosition) > 0.2f) {
					
					_rigidbody.AddForce(_velocitySmoothedVector.average * BEAM_RELEASE_VELOCITY_MULTIPLIER, ForceMode.Impulse);
					_velocitySmoothedVector.Clear();
				}

				_rigidbody.isKinematic = false;
				_isAttracted = false;
			}
		} else {
			
			_rigidbody.isKinematic = false;
			_isAttracted = false;
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

                _grabbedBy = hand;
                _grabbedByButton = button;
                _localGrabOffset = invTouchRot * (transform.position - touchPos);
                _localGrabRotation = invTouchRot * transform.rotation;
                _velocitySmoothedVector.Clear();
            }
        }
    }
}
