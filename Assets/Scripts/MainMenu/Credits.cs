using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){
        EventTrigger triggers = GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry click = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick,
        };
        click.callback.AddListener((data)=> { Application.OpenURL("http://clanbomber.sourceforge.net/"); });
        triggers.triggers.Add(click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
