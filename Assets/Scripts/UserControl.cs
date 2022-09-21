using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class UserControl : MonoBehaviour
{
    InputDevice rdevice;
    InputDevice head;
    Vector2 primary2DAxis;
    Quaternion headRotation;
    GameObject maze;
    GameObject user;
    bool endTriggered = false;
    float timer = 3f;

    // Start is called before the first frame update
    void Start()
    {
        maze = GameObject.Find("maze");
        user = GameObject.Find("XR Origin");
    }

    // Update is called once per frame
    void Update()
    {
        if (endTriggered)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Application.Quit();
            }
        }

        if (rdevice.isValid && head.isValid)
        {
            rdevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis);
            head.TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation);
            if (primary2DAxis.y != 0)
            {
                maze.transform.RotateAround(user.transform.position, new Vector3(
                    Mathf.Cos(headRotation.eulerAngles.y * Mathf.Deg2Rad),
                    0,
                    -Mathf.Sin(headRotation.eulerAngles.y * Mathf.Deg2Rad)),
                    primary2DAxis.y);
            }
        }
        else
        {
            if (!rdevice.isValid)
            {
                var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
                InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
                rdevice = rightHandDevices.FirstOrDefault();
            }
            if (!head.isValid)
            {
                var headDevices = new List<UnityEngine.XR.InputDevice>();
                InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.Head, headDevices);
                head = headDevices.FirstOrDefault();
            }
        }
        if (maze == null)
        {
            maze = GameObject.Find("maze");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "End")
        {
            endTriggered = true;
            other.enabled = false;
            other.gameObject.SetActive(false);
            user.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
