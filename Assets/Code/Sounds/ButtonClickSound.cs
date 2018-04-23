using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : EventTrigger
{
    private const string clickSound1 = "event:/UI Button";
    private const string clickSound2 = "event:/UI Button (2)";

    public override void OnPointerEnter(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(clickSound2);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(clickSound1);
    }
}
