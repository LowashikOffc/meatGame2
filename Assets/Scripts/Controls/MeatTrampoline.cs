using UnityEngine;

public class MeatTrampoline : MonoBehaviour
{
    [SerializeField] private int _force;
    [SerializeField] private ForceMode _forceMode;
    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        Debug.Log(rb);
        if (rb != null)
        {
            bool b = rb.transform.GetComponent<PlayerMovement>().GetClimbing();
            if (b)
            {
                Debug.Log("return");
                return;
            }
            //rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
            rb.AddForce(Vector3.up * _force, _forceMode);
        }
    }
}
