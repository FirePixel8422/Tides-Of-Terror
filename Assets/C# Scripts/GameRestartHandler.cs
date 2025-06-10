using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameRestartHandler : MonoBehaviour
{ 
    public static GameRestartHandler Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] private GameObject overlayCanvas;
    [SerializeField] private GameObject winScreen, loseScreen;

    [SerializeField] private float loseRestartTime = 3f;
    [SerializeField] private float winRegainControlTime = 3f;


    public void Win()
    {
        XRMovementController.Instance.gameObject.SetActive(false);

        overlayCanvas.SetActive(true);
        winScreen.SetActive(true);

        StartCoroutine(ContinueGameDelay());
    }
    private IEnumerator ContinueGameDelay()
    {
        yield return new WaitForSeconds(winRegainControlTime);

        XRMovementController.Instance.gameObject.SetActive(true);
        overlayCanvas.SetActive(false);
    }

    public void Lose()
    {
        XRMovementController.Instance.gameObject.SetActive(false);

        overlayCanvas.SetActive(true);
        loseScreen.SetActive(true);

        StartCoroutine(RestartGameDelay());
    }
    private IEnumerator RestartGameDelay()
    {
        yield return new WaitForSeconds(loseRestartTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
