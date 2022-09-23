using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    public float Density = 2;
    public GameObject[] Prefabs = new GameObject[0];
    public Vector3 StartPos = Vector3.zero;
    public Vector3 EndPos = new Vector3(100, 0, 100);
    public Texture2D HeightMap;
    public float offsetY = 0;
    public Vector3 scale = new Vector3(1f, 1f, 1f);
    public float scaleY = 1.0f;
    public Vector2 posScaleXZ = new Vector3(1.0f, 1.0f);
    public float speedScale = 1f;

    public List<GameObject> instances;

    // Start is called before the first frame update
    void Start()
    {
        int count = 0;
        Vector3 curPos = StartPos;
        while (curPos.z < EndPos.z)
        {
            curPos.z += Random.Range(Density / 2, Density * 1.5f);
            curPos.x = StartPos.x;
            while (curPos.x < EndPos.x)
            {
                curPos.x += Random.Range(Density / 5, Density * 5);
                
                int x = (int)(HeightMap.width * curPos.x / (EndPos - StartPos).x);
                int y = (int)(HeightMap.height * curPos.z / (EndPos - StartPos).x);
                if (HeightMap.GetPixel(x, y).g < 0.75f)
                {
                    continue;
                }

                float width = HeightMap.GetPixel(x, y).b;
                curPos.y = offsetY;
                var poss = new Vector3(curPos.x / posScaleXZ.x, curPos.y, curPos.z / posScaleXZ.y);

                int id = Random.Range(0, Prefabs.Length);
                GameObject cloud = Instantiate(Prefabs[id], poss, Quaternion.identity);
                
                // Assign to layer.
                int layer = LayerMask.NameToLayer("Cloud Layer");
                cloud.layer = layer;

                // Change location accordingly.
                cloud.transform.localScale = new Vector3(
                    width * scale.x,
                    scaleY * scale.y,
                    width  * scale.z
                );
                cloud.transform.parent = transform;
                
                instances.Add(cloud);
                
                count++;
            }
        }

        Debug.Log(count);
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < instances.Count; i++)
        {
            var transform = instances[i].transform;
            transform.position += Vector3.forward * Time.deltaTime * speedScale;
            float z = StartPos.z + (transform.position.z - StartPos.z) % (EndPos.z - StartPos.z);
            transform.position = new Vector3(transform.position.x, 0.5f, z);
        }
    }
}