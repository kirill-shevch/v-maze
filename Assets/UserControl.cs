using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class UserControl : MonoBehaviour
{
    InputDevice rdevice;
    Vector2 primary2DAxis;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rdevice.isValid)
        {
            
            rdevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis);
            Debug.Log(string.Format("x:{0}, y:{1}", primary2DAxis.x, primary2DAxis.y));
        }
        else
        {
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
            rdevice = rightHandDevices.FirstOrDefault();
        }
    }
}
