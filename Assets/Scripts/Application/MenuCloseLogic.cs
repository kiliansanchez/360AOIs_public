using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

/// <summary>
/// Tiny little script that's attached to everything that should be "closed" when the user clicks outside of it's bounds.
/// Mostly used for menus that should disappear when user clicks outside of them.
/// </summary>
public class MenuCloseLogic : MonoBehaviour
{

    void Update()
    {
        if (gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (!results.Any(r => r.gameObject == this.gameObject))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
