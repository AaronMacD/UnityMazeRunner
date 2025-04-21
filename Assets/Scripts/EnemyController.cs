using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BearAI : MonoBehaviour
{
    enum State { WANDERING, STOPPED, SNIFFING, CHASING, WOUNDED, RETREATING, SLEEPING }
    
    [SerializeField] private GameObject[] locations;
    [SerializeField] private int health = 1000;
    
    private NavMeshAgent nav;
    private Animator animator;
    private GameObject player;
    private int currentLocation = 0;
    private State state = State.WANDERING;

    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player");
        // Targets our bear will walk between.
        locations = new GameObject[]
        {
            GameObject.Find("TargetOne"),
            GameObject.Find("TargetTwo"),
            GameObject.Find("TargetThree")
        };
        // Set first target.
        nav.SetDestination(locations[0].transform.position);
    }

    void Update()
    {
        // Get distance to player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Less than 20?  We can see them.  Go get them~
        if (distance < 20f) {
            StartChasing();
        }
        else if (distance < 30f) {
            // Less than 30 but over 20?  We sense something...
            StartStopped();
        }
        else if (distance > 50f) {
            // No detection.  Keep going.
            StartWandering();
        }
        // Near an objective?  Get the next one.
        if (state == State.WANDERING && nav.remainingDistance < 0.6f){
            nav.SetDestination(locations[++currentLocation % locations.Length].transform.position);
        }
        // If we sense someone, sniff for more info.
        if (state == State.STOPPED) {
            StartCoroutine(Sniff());
        }
        // Go after them!
        if (state == State.CHASING) {
            UpdateChasing();
        }
    }

    IEnumerator Sniff()
    {
        // Don't have a Sniff animation.  We'll use eat.
        SetState(State.SNIFFING, "Eat");
        // Tell agent to stop walking.
        nav.isStopped = true;
        // Wait 2 seconds.
        yield return new WaitForSeconds(2);
        // How close to player?
        float distance = Vector3.Distance(transform.position, player.transform.position);
        // If close, go after them.  Otherwise, resume wandering.
        if(distance < 25.0f){
            SetState(State.CHASING);
            StartChasing();
        } else {
            StartWandering();
            SetState(State.WANDERING);
            nav.isStopped = false;
        }
    }

    void StartChasing()
    {
        if (state is State.CHASING or State.WOUNDED or State.RETREATING) return;
        SetState(State.CHASING, "RunForward");
    }

    void StartStopped()
    {
        if (state is State.SNIFFING or State.CHASING or State.WOUNDED or State.RETREATING) return;
        SetState(State.STOPPED);
    }

    void StartWandering()
    {
        SetState(State.WANDERING, "Combat Idle");
        nav.speed = 3.5f;
        nav.SetDestination(locations[currentLocation].transform.position);
    }

    void UpdateChasing()
    {
        nav.isStopped = false;
        nav.SetDestination(player.transform.position);
        nav.speed = 7f;
    }

    void StartRetreating()
    {
        SetState(State.RETREATING);
        nav.SetDestination(Random.insideUnitCircle * 100f);
    }
    void StartSleeping()
    {
        SetState(State.SLEEPING);
        
    }

    void SetState(State newState, string animation = null)
    {
        state = newState;
        if (animation != null) animator.Play(animation);
        Debug.Log(state);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            health -= 100;
            if (health < 300) SetState(State.WOUNDED);
        }
    }
}
