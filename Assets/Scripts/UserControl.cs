using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class UserControl : MonoBehaviour
{
    InputDevice rdevice;
    InputDevice head;
    Vector2 primary2DAxis;
    Quaternion headRotation;
    GameObject maze;
    GameObject user;
    AudioSource rotating;
    AudioSource walking;
    bool endTriggered = false;
    float timer = 3f;

    private Rigidbody rigidbody;

    private Vector3 velocity, desiredVelocity;

    private float maxSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        maze = GameObject.Find("maze");
        user = GameObject.Find("XR Origin");
        rigidbody = user.GetComponent<Rigidbody>();
        rotating = user.AddComponent<AudioSource>();
        rotating.clip = Resources.Load<AudioClip>("Sounds/Scraping Stone Cut");
        walking = user.AddComponent<AudioSource>();
        walking.clip = Resources.Load<AudioClip>("Sounds/Stone steps");
    }


    void FixedUpdate()
    {
        // Retrieve previous state.
        velocity = rigidbody.velocity;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, 1.0f);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, 1.0f);

        rigidbody.velocity = velocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (endTriggered)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SceneManager.LoadScene("MovementScene");
                endTriggered = false;
            }
        }
        
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        if (rdevice.isValid && head.isValid)
        {
            rdevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis);
            head.TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation);
            if (primary2DAxis.y != 0)
            {
                if (!rotating.isPlaying)
                {
                    rotating.Play();
                }
                maze.transform.RotateAround(user.transform.position, new Vector3(
                        Mathf.Cos(headRotation.eulerAngles.y * Mathf.Deg2Rad),
                        0,
                        -Mathf.Sin(headRotation.eulerAngles.y * Mathf.Deg2Rad)),
                    primary2DAxis.y);
            }
            else
            {
                if (rotating.isPlaying)
                {
                    rotating.Stop();
                }
            }
            if (playerInput.x != 0 || playerInput.y != 0)
            {
                if (!walking.isPlaying)
                {
                    walking.Play();
                }
            }
            else
            {
                if (walking.isPlaying)
                {
                    walking.Stop();
                }
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
            other.gameObject.GetComponent<AudioSource>().Play();
            endTriggered = true;
            //other.enabled = false;
            //other.gameObject.SetActive(false);
            user.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
