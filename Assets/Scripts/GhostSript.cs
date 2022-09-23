using UnityEngine;

public class GhostSript : MonoBehaviour
{
    GameObject user;
    GameObject ghost;
    // Start is called before the first frame update
    void Start()
    {
        user = GameObject.Find("XR Origin");
        ghost = GameObject.Find("Ghost");
        var ghostPrefab = Resources.Load("Ghosts/Prefab_Ghost1");
        var position = new Vector3(WorldScript.maze_depth, WorldScript.maze_height, WorldScript.maze_width);
        ghost = (GameObject)Instantiate(ghostPrefab, user.transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        ghost.transform.LookAt(user.transform);
    }
}
