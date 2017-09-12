using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTrack : MonoBehaviour
{
    public OVRInput.Controller controller;
    public OVRInput.RawButton button;
    public OVRInput.RawButton tractorBeamButton;
    public Material gripMaterial;
    Material originalMaterial;
    LineRenderer lineRenderer;

	// Use this for initialization
	void Start ()
    {
        originalMaterial = GetComponent<MeshRenderer>().material;
        lineRenderer = GetComponent<LineRenderer>();
	}

    // Update is called once per frame
	Grabable _attractableObject;
	RotatableWithTheBim _rotatableObject;
	Vector3 _rotationOrigin;
	void Update ()
    {
        transform.localPosition = OVRInput.GetLocalControllerPosition(controller);
        transform.localRotation = OVRInput.GetLocalControllerRotation(controller);

        if(gripMaterial != null && OVRInput.GetDown(button))
        {
            GetComponent<MeshRenderer>().material = gripMaterial;
        }
        else if (gripMaterial != null && OVRInput.GetUp(button))
        {
            GetComponent<MeshRenderer>().material = originalMaterial;
        }

        Vector3 lineEnd = transform.position + transform.forward * 100;
        if (OVRInput.Get(tractorBeamButton))
        {
            RaycastHit hitInfo;
            if (Physics.Linecast(transform.position, lineEnd, out hitInfo))
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { transform.position, hitInfo.point });
                GameObject hitObject = hitInfo.collider.gameObject;
			
				if(_attractableObject == null && _rotatableObject == null)
                {
                    _attractableObject = hitObject.GetComponent<Grabable>();
                    if (_attractableObject != null)
                    {
                        _attractableObject.Attract(true, transform);
                    }
                }

				if (_rotatableObject == null) {

					_rotatableObject = hitObject.GetComponent<RotatableWithTheBim> ();
					if (_rotatableObject != null) {
						_rotationOrigin = hitInfo.point;
					}
				} else {

					Vector3 rotation = Vector3.ProjectOnPlane(transform.position - hitInfo.point, transform.position - hitObject.transform.position);
					_rotatableObject.UpdateRotation(rotation);
				}


            }
            else
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { transform.position, lineEnd });
            }
        }
        else
        {
            if (_attractableObject != null)
                _attractableObject.Attract(false);

            _attractableObject = null;

			_rotatableObject = null;

            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[] { });
        }
    }
}
