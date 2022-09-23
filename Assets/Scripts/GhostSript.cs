using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostSript : MonoBehaviour
{
    GameObject user;
    GameObject ghost;
    Vector3 target;
    public float speed = 1f;
    bool freeze = true;
    float freezeTime = 20f;
    AudioSource flying;
    bool endTriggered = false;
    float endTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("XR Origin");
        ghost = GameObject.Find("Ghost");
        var ghostPrefab = Resources.Load("Ghosts/Prefab_Ghost1");
        var position = new Vector3(WorldScript.maze_depth, WorldScript.maze_height, WorldScript.maze_width);
        ghost = (GameObject)Instantiate(ghostPrefab, position, Quaternion.identity);
        ghost.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        flying = ghost.AddComponent<AudioSource>();
        flying.clip = Resources.Load<AudioClip>("Sounds/Ghost");
        target = new Vector3(user.transform.position.x, user.transform.position.y, user.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        ghost.transform.LookAt(user.transform);
        ghost.transform.Rotate(new Vector3(-90, 0, 0));


        if (freeze)
        {
            freezeTime -= Time.deltaTime;
            if (freezeTime <= 0f)
            {
                freeze = false;
                freezeTime = 16f;
                flying.Play();
            }
        }
        else
        {
            var step = speed;// * Time.deltaTime; // calculate distance to move
            ghost.transform.position = Vector3.MoveTowards(ghost.transform.position, target, step);
        }

        // Check if the position of the cube and sphere are approximately equal.
        if (Vector3.Distance(ghost.transform.position, target) < 0.001f)
        {
            freeze = true;
            // Swap the position of the cylinder.
            target = new Vector3(user.transform.position.x, user.transform.position.y, user.transform.position.z);
        }

        if (!endTriggered && Vector3.Distance(user.transform.position, ghost.transform.position) < 1.3f)
        {
            endTriggered = true;
            flying.PlayOneShot(Resources.Load<AudioClip>("Sounds/Death"));
        }

        if (endTriggered)
        {
            endTime -= Time.deltaTime;
            if (endTime <= 0f)
            {
                SceneManager.LoadScene("MovementScene");
                endTriggered = false;
            }
        }
    }
}
