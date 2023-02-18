using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    LineRenderer line;
    Vector3 grapplePoint;
    SpringJoint joint;
    Quaternion desiredRotation;


    [Header("References")]
    [SerializeField] MeshRenderer gunMesh;
    [SerializeField] Transform gunModel;
    [SerializeField] LayerMask grappleMask;
    [SerializeField] Transform gunTip;
    [SerializeField] Transform cam;
    [SerializeField] Transform player;
    [SerializeField] float maxDistance = 100f;

    [Header("Spring Parameters")]
    [SerializeField] float maxMultiplier = 0.8f;
    [SerializeField] float minMultiplier = 0.25f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float springForce = 4.5f;
    [SerializeField] float damper = 7f;
    [SerializeField] float massScale = 4.5f;


    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if(Input.GetButtonDown("Grapple")) StartGrapple();
        if(Input.GetButtonUp("Grapple")) StopGrapple();
    }

    void LateUpdate()
    {
        if(!joint)
        {
            desiredRotation = transform.parent.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime*rotationSpeed);
            return;
        }
        line.SetPosition(0, gunTip.position);
        line.SetPosition(1, grapplePoint);
        desiredRotation = Quaternion.LookRotation(grapplePoint - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime*rotationSpeed);
    }

    void StartGrapple()
    {
        gunMesh.enabled = true;
        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, grappleMask))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distranceFromPoint = Vector3.Distance(player.position, grapplePoint);
            joint.maxDistance = distranceFromPoint*maxMultiplier;
            joint.minDistance = distranceFromPoint*minMultiplier;

            joint.spring = 5f;
            line.positionCount = 2;
        }
    }

    void StopGrapple()
    {
        gunMesh.enabled = false;
        line.positionCount = 0;
        Destroy(joint);
    }
}
