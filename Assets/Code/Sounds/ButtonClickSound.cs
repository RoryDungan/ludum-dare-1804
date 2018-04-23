using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private const string clickSound = "event:/UI Button";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(ButtonClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        RuntimeManager.PlayOneShot(clickSound);
    }
}
