using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// This script manages the name of the AOI and its representation in the AOIList in the UI
/// </summary>
public class Listable : AOIComponent
{

    public string Name { get; private set; } = "";

    public static int ListableCount = 0;

    public GameObject ItemPrefab;

    private GameObject _itemInstance;

    private Transform _contentContainer;

    public static implicit operator string(Listable listable) => listable.Name;

    private TMPro.TMP_InputField _inputField;


    protected override void Start()
    {
        _contentContainer = GameObject.Find("ListContent").transform;

        ListableCount++;

        if (Name == "")
        {
            Name = "Item " + ListableCount;
        }

        _itemInstance = Instantiate(ItemPrefab);

        _inputField = _itemInstance.GetComponentInChildren<TMPro.TMP_InputField>();

        _inputField.text = Name;
        //parent the item to the content container
        _itemInstance.transform.SetParent(_contentContainer);
        //reset the item's scale -- this can get munged with UI prefabs
        _itemInstance.transform.localScale = Vector2.one;


        _inputField.readOnly = true;
        _inputField.onFocusSelectAll = false;

        _inputField.onEndEdit.AddListener(UpdateNameFromInputField);
        _itemInstance.GetComponentInChildren<EventTrigger>().triggers[0].callback.AddListener(OnListItemClick);

        _itemInstance.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 1, 1);

        base.Start();
    }

    /// <summary>
    /// Callback for UI-Inputfield. Gets called when edit is being completed. Changes name of AOI to whatever user has
    /// specified in inputfield.
    /// </summary>
    /// <param name="newname"></param>
    void UpdateNameFromInputField(string newname)
    {
        Name = newname;
    }

    /// <summary>
    /// Changes name of AOI and updates text in list
    /// </summary>
    /// <param name="newname">New name of AOI</param>
    public void SetName(string newname)
    {
        Name = newname;

        if (_inputField != null)
        {
            _inputField.text = Name;
        }
    }

    /// <summary>
    /// Callback for click event on AOI-list item. Implements single-click and double-click behavior.
    /// </summary>
    /// <param name="eventData"></param>
    void OnListItemClick(BaseEventData eventData)
    {
        var tap = ((PointerEventData)eventData).clickCount;
        if (tap >= 2)
        {
            _inputField.readOnly = false;
            _inputField.onFocusSelectAll = true;
            _inputField.ForceLabelUpdate();
        }
        else
        {
            EditorCamera.Camera.transform.LookAt(this.transform);
            AOIManager.SetActiveObject(this.gameObject);
            _itemInstance.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 1, 1);
        }
    }


    protected override void OnDestroy()
    {
        Destroy(_itemInstance);
        base.OnDestroy();
    }

    /// <summary>
    /// changes color of item in AOI list when AOI is being activated.
    /// </summary>
    protected override void OnActivate()
    {
        _itemInstance.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 1, 1);
    }

    /// <summary>
    /// changes color of item in AOI list when AOI is being de-activated.
    /// </summary>
    protected override void OnDeactivate()
    {
        _itemInstance.GetComponent<UnityEngine.UI.Image>().color = new Color(.2f, .2f, .2f, 1);
        _inputField.readOnly = true;
        _inputField.onFocusSelectAll = false;
    }
}
