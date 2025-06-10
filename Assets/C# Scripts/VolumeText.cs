using TMPro;
using UnityEngine;


public class VolumeText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObj;


    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        textObj.text = VolumeHandler.Volume.ToString();
    }
}
