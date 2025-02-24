using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Receiver : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        string receivedData = PlayerPrefs.GetString("PassedData");
        text.text = receivedData;
    }

}