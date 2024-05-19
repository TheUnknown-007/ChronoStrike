using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] Transform Door;
    [SerializeField] AudioSource Audio;
    [SerializeField] Vector3 movement;
    [SerializeField] float moveSpeed;

    Vector3 initialPosition;
    Vector3 target;

    bool moving = false;

    void Start()
    {
        initialPosition = Door.transform.position;
    }

    void Update()
    {
        if(moving)
        {
            Door.transform.position = Vector3.MoveTowards(Door.transform.position, target, Time.deltaTime*moveSpeed);
            if(Door.transform.position == target) moving = false;
        }
    }
    
    void SetDestination(bool open)
    {
        Audio.Play();
        target = initialPosition + (open ? movement : Vector3.zero);
        moving = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        SetDestination(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        SetDestination(false);
    }
}
