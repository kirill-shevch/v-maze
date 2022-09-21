using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class FireScript : MonoBehaviour
{
    private GameObject player;
    private GameObject user;
    private ParticleSystem emitter1;
    private ParticleSystem emitter2;
    private Rigidbody rigidbody;

    private bool isColliding = false;

    [SerializeField] private Vector3 translation;
    [SerializeField] private float scale = 1.0f;
    [SerializeField] private float distance;

    [SerializeField] private float particlesVelocityScaling = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Torch");
        user = GameObject.Find("XR Origin");
        emitter1 = GameObject.Find("FireAdd").GetComponent<ParticleSystem>();
        emitter2 = GameObject.Find("FireMain").GetComponent<ParticleSystem>();
        rigidbody = user.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEmitter(emitter1);
        UpdateEmitter(emitter2);

        player.transform.position = user.transform.position + user.transform.forward * distance + translation;
        player.transform.localScale = Vector3.one * scale;
    }

    void UpdateEmitter(ParticleSystem emitter)
    {
        if (isColliding)
        {
            return;
        }

        // Retrieve previous state.
        var velocityPlayer = rigidbody.velocity;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[emitter.particleCount];
        int count = emitter.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            var scale = particlesVelocityScaling;
            var velocity = particles[i].velocity;
            velocity.x = velocityPlayer.x * scale;
            velocity.z = velocityPlayer.z * scale;

            particles[i].velocity = velocity;
        }

        emitter.SetParticles(particles, count);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}