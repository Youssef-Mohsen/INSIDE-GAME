using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemFix : MonoBehaviour
{
    void Awake()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystems.Length > 1)
        {
            Destroy(eventSystems[0].gameObject);
        }
    }
}