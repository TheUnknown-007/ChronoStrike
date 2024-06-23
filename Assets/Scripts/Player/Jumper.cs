using UnityEngine;

public class Jumper : MonoBehaviour
{
    [SerializeField] float jumpPower = 1400;
    [SerializeField] AudioSource audSrc;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            PlayerManager.instance.BoostJump(jumpPower);
            //other.GetComponent<Rigidbody>().AddForce(other.transform.up * jumpPower, ForceMode.Impulse);
            audSrc.Play();
        }
    }
}
