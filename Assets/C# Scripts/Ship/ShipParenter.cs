using UnityEngine;


public class ShipParenter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ship Loot") && other.isTrigger == false)
        {
            other.transform.SetParent(transform, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ship Loot") && other.isTrigger == false)
        {
            other.transform.parent = null;
        }
    }
}
