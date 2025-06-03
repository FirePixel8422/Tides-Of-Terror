using UnityEngine;


public class WaterToggler : MonoBehaviour
{
    [Header("On, Off")]
    [SerializeField] private Collider[] colls = new Collider[2];

    [SerializeField] private MeshRenderer waterRenderer;
    [SerializeField] private bool waterActive = true;


    private void Start()
    {
        waterRenderer.enabled = waterActive;

        colls[0].enabled = !waterActive;
        colls[1].enabled = waterActive;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AntiWaterTrigger"))
        {
            waterActive = !waterActive;
            waterRenderer.enabled = waterActive;

            colls[0].enabled = !waterActive;
            colls[1].enabled = waterActive;
        }
    }
}
