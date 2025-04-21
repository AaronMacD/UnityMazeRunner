using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed;

    private NavMeshAgent nav;
    private Animator animator;
    private GameObject player;
    private GameObject floor;
    private GameObject destination;

    private bool huntPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player");
        floor = GameObject.Find("Floor_Reference");
        destination = GameObject.Find("Destination");
        // Targets our bear will walk between.

        // Set first target.
        nav.speed = patrolSpeed;
        getNewLocation();
    }

    // Update is called once per frame
    void Update()
    {
        // Get distance to player
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float currentSpeed = nav.velocity.magnitude;

        if (currentSpeed > 0 && !huntPlayer)
        {
            animator.SetBool("Forward", true);
            animator.SetBool("Run", false);
        }
        else if (currentSpeed > 0 && huntPlayer)
        {
            animator.SetBool("Forward", true);
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Forward", false);
            animator.SetBool("Run", false);
        }

        if (Vector3.Distance(transform.position, destination.transform.position) < 2)
        {
            getNewLocation();
        }

        if (huntPlayer)
        {
            navDestSet(player.transform.position);
        }

        if (distance > 10 && huntPlayer)
        {
            huntPlayer = false;
            nav.speed = patrolSpeed;
            getNewLocation();
        }
    }

    private void getNewLocation()
    {
        float x = UnityEngine.Random.Range(1, 10);
        float z = UnityEngine.Random.Range(1, 10);

        Vector3 pos = floor.transform.position;
        pos.x += x * 3;
        pos.z += z * 3;

        destination.transform.position = pos;
        navDestSet(pos);
    }
    private void navDestSet(Vector3 pos)
    {
        nav.SetDestination(pos);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * 5, Color.yellow);
        if (Physics.Raycast(transform.position, transform.forward, out hit, 5.0f))
        {
            
            if (hit.collider.CompareTag("Player"))
            {
                huntPlayer = true;
                nav.speed = chaseSpeed;
            }
        }

        
            
    }
}