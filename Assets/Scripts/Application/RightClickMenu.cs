using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script can be attached to any gameobject to display a menu when the gameobject is right-clicked. Currently
/// used on AOIs and keyframe-icons. 
/// </summary>
public class RightClickMenu : MonoBehaviour
{


    public static GameObject LastClickedObjectWithMenu;

    public Canvas canvas;

    public GameObject _menuPrefab;
    public GameObject _itemPrefab;

    public GameObject Menu { get; protected set; }
    protected Transform _contentContainer;

    public bool AddDeleteOnStart;


    private void Awake()
    {
        UnityEngine.Assertions.Assert.IsNotNull(_menuPrefab);
        UnityEngine.Assertions.Assert.IsNotNull(_itemPrefab);

        canvas = GameObject.Find("UI Canvas").GetComponent<Canvas>();

        Menu = Instantiate(_menuPrefab);
        Menu.transform.SetParent(canvas.transform);
        Menu.transform.localScale = Vector2.one;


        if (AddDeleteOnStart)
        {
            AddItem("Delete", DeleteObjectUsingRightClickMenu);
        }

        HideMenu();
    }

    /// <summary>
    /// Method to delete the object from its right click menu. deletes the menu-gameobject as well as the object itself.
    /// </summary>
    void DeleteObjectUsingRightClickMenu()
    {
        Destroy(Menu);
        Destroy(gameObject);
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1) && GotClicked() && !Menu.activeSelf)
        {        
            LastClickedObjectWithMenu = this.gameObject;
            ShowMenu();
        }

    }

    /// <summary>
    /// helper function thats not neccessary anymore. could be removed.
    /// </summary>
    /// <param name="fromGameObject"></param>
    /// <param name="withName"></param>
    /// <returns></returns>
    static public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }

    /// <summary>
    /// Checks whether or not the object got clicked. Used together with Input.GetMouseButtonDown(1) to determine click on object.
    /// Can't just used OnMouseDown because this needs to work with physics- as well as ui-objects.
    /// </summary>
    /// <returns>Returns true if at time of method-call the cursor is above the object this script is attached to.</returns>
    bool GotClicked()
    {

        if (gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                if (results.First().gameObject == this.gameObject || results.First().gameObject == Menu)
                {
                    return true;
                }
            }
            
        }
        else
        {
            // Create a new Ray instance using the current mouse position
            Ray ray = EditorCamera.Camera.ScreenPointToRay(Input.mousePosition);

            // Create a new RaycastHit instance to store the results of the raycast
            RaycastHit hit = new RaycastHit();


            // Check if the raycast hit a physics object
            if (Physics.Raycast(ray, out hit, 20f))
            {
                if (hit.transform.gameObject == this.gameObject)
                {
                    return true;
                }
            }
        }

        return false;
        
    }

    private void OnDestroy()
    {
        Destroy(Menu);
    }

    /// <summary>
    /// Adds entry to right click menu.
    /// </summary>
    /// <param name="label">Label for menu entry.</param>
    /// <param name="action">Callback for what happens when the entry is clicked.</param>
    public void AddItem(string label, UnityEngine.Events.UnityAction action)
    {
        var newitem = Instantiate(_itemPrefab);

        newitem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = label;
        //parent the item to the content container
        newitem.transform.SetParent(Menu.transform);
        //reset the item's scale -- this can get munged with UI prefabs
        newitem.transform.localScale = Vector2.one;

        newitem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(action);
    }

    /// <summary>
    /// Removes an item from the right click menu based on the label.
    /// </summary>
    /// <param name="labelToDelete">Entry to delete based on provided label.</param>
    public void RemoveItem(string labelToDelete)
    {
        TMPro.TextMeshProUGUI[] labels = Menu.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        int index = Array.FindIndex(labels,0, labels.Length, item => item.text == labelToDelete);

        if (index == -1)
        {
            return;
        }

        Destroy(labels[index].transform.parent.gameObject);
    }

    /// <summary>
    /// Shows right click menu close to cursor position.
    /// </summary>
    public void ShowMenu()
    {

        Vector2 movePos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera,
            out movePos);

        Menu.transform.position = canvas.transform.TransformPoint(movePos);

        Menu.SetActive(true);
    }

    /// <summary>
    /// Hides right click menu.
    /// </summary>
    public void HideMenu()
    {
        Menu.SetActive(false);
    }

    /// <summary>
    /// Helper function to check whether or not the cursor is currently over any UI element. For example, if the cursor is
    /// currently over the ui instead of the 360degree video a left click shouldnt spawn a new AOI etc.
    /// This helper is is used throughout multiple scripts and should probably be moved to a different location. 
    /// </summary>
    /// <returns>True of cursor is currently over any UI element.</returns>
    public static bool IsCursorOverUI()
    {

        var pointereventdata = new PointerEventData(EventSystem.current);
        //Set the Pointer Event Position to that of the mouse position
        pointereventdata.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointereventdata, results);

        if (results.Count > 0 )
        {
            return true;
        }
        else
        {
            return false;
        }

    }

}
