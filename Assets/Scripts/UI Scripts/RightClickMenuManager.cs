using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickMenuManager : MonoBehaviour
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

        //_contentContainer = GetChildGameObject(_menu, "Content").transform;
        //UnityEngine.Assertions.Assert.IsNotNull(_contentContainer);

        //AddItem("Close Menu", HideMenu);

        if (AddDeleteOnStart)
        {
            AddItem("Delete", DeleteObjectUsingRightClickMenu);
        }

        HideMenu();
    }

    void DeleteObjectUsingRightClickMenu()
    {
        Destroy(Menu);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1) && GotClicked() && !Menu.activeSelf)
        {        
            LastClickedObjectWithMenu = this.gameObject;
            ShowMenu();
        }

    }

    static public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }

    bool GotClicked()
    {

        if (gameObject.layer == LayerMask.NameToLayer("UI"))
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Any(r => r.gameObject == this.gameObject) || results.Any(r => r.gameObject == Menu))
            {
                return true;
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

    public void RemoveItem(string labelToDelete)
    {
        TMPro.TextMeshProUGUI[] labels = Menu.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        int index = Array.FindIndex(labels,0, labels.Length, item => item.text == labelToDelete);

        if (index == -1)
        {
            //Debug.LogWarning("Removing" + labelToDelete + " from right click menu didnt work");
            return;
        }

        Destroy(labels[index].transform.parent.gameObject);
    }

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
    public void HideMenu()
    {
        Menu.SetActive(false);
    }


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
