using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class MazeMovement : MonoBehaviour
{
    GameObject playerCamera;
    InputDevice device;
    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GameObject.Find("Main Camera");

        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

        if(rightHandDevices.Count == 1)
        {
            device = rightHandDevices[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Mouse X") != 0) {
           gameObject.transform.RotateAround(playerCamera.transform.position, Vector3.up, 5 * Input.GetAxis("Mouse X"));
        }

        if(Input.GetAxis("Mouse Y") != 0) {
           gameObject.transform.RotateAround(playerCamera.transform.position, Vector3.left, 5 * Input.GetAxis("Mouse Y"));
        }
        Vector2 axisValue;

        if(device.TryGetFeatureValue(CommonUsages.secondary2DAxis, out axisValue)) {
            gameObject.transform.RotateAround(playerCamera.transform.position, Vector3.up, axisValue[0]);
            gameObject.transform.RotateAround(playerCamera.transform.position, Vector3.left, axisValue[1]);
        }


    }
}
