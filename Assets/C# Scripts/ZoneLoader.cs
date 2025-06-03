using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneLoader : MonoBehaviour
{
    public static ZoneLoader Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }



    public List<GameObject> monsters; 

    [SerializeField] private MinMaxFloat timeBetweenEncounter;



    private void Start()
    {
        StartCoroutine(EncounterDelay());
    }


    public void PrepareNewEncounter()
    {
        StartCoroutine(EncounterDelay());
    }

    private IEnumerator EncounterDelay()
    {
        yield return new WaitForSeconds(EzRandom.Range(timeBetweenEncounter));

        StartEncounter();
    }

    private void StartEncounter()
    {
        int r = EzRandom.Range(0, monsters.Count);

        Instantiate(monsters[r]);
        BoatEngine.Instance.locked = true;
        monsters.RemoveAt(r);
    }

    public void EndEncounter()
    {
        BoatEngine.Instance.locked = false;
    }
}
