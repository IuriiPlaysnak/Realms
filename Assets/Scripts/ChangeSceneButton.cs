using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    public int SceneIndex;
    public float TouchRadius;
    Collider colliderX;

    // Use this for initialization
    void Start()
    {
        colliderX = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPress(OVRInput.Controller.LTouch, OVRInput.RawButton.LIndexTrigger);
        CheckPress(OVRInput.Controller.RTouch, OVRInput.RawButton.RIndexTrigger);
    }

    void CheckPress(OVRInput.Controller hand, OVRInput.RawButton button)
    {
        if (OVRInput.GetDown(button))
        {
            Vector3 touchPos = OVRManager.instance.transform.LocalToWorld(OVRInput.GetLocalControllerPosition(hand));

            if((colliderX.ClosestPointOnBounds(touchPos) - touchPos).sqrMagnitude < TouchRadius* TouchRadius)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneIndex);
            }
        }
    }
}
