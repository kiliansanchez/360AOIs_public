using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;


/*
This class does two things (meaning it could be refactored into two classes)
1. Manages AOIs based on user input
    a) Deactivates currently active AOIs and spawns new AOIs if user clicks on empty space in 360 degree video
    b) Sets an AOI as active when user clicks on AOI in 360 degree video and deactivates the previously selected AOI 

2. Manages AOI component behaviour (specifically resizing and movement of AOIs) when multiple AOIs overlap
    a) prioritizes active AOI over inactive AOIs
    b) prioritizes resizing over movement 

 */

public class AOIManager : MonoBehaviour
{
    public static AOIManager Instance;
    public static GameObject ActiveAOI = null;

    public delegate void AoiChangeDel(GameObject newActiveAOI);
    public static AoiChangeDel NewActiveAoiSubscribers;

    public GameObject AOIPrefab;

    public static bool FlipMesh = true;

    private GameObject _canvas;


    private bool _manuallyTriggeredResizeDueToOverlap = false;
    private GameObject _objectToManuallyResize = null;

    private bool _manuallyTriggeredMovementDueToOverlap = false;

    public float SpawnDistance = 10f;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _canvas = GameObject.Find("UI Canvas");
        UnityEngine.Assertions.Assert.IsNotNull(_canvas);
    }

    // Update is called once per frame
    void Update()
    {
        if (!VideoManager.IsInitialized)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !RightClickMenuManager.IsCursorOverUI())
        {
            // Create a new Ray instance using the current mouse position
            Ray ray = EditorCamera.Camera.ScreenPointToRay(Input.mousePosition);


            var results = Physics.RaycastAll(ray.origin, ray.direction, 20f);

            // no overlapping GameObjects, just one thing clicked
            if (results.Length == 1)
            {
                //if an AOI was clicked, set it active
                if (results.Any(r => r.transform.gameObject.CompareTag("AOI") || r.transform.gameObject.CompareTag("AOI_Invisible")))
                {
                    var aoi_hit = results.Where(r => r.transform.gameObject.CompareTag("AOI") || r.transform.gameObject.CompareTag("AOI_Invisible")).First().transform.gameObject;
                    if (aoi_hit != ActiveAOI)
                    {
                        SetActiveObject(aoi_hit);
                    }
                }

                
                else if (results.Any(r => r.transform.gameObject.GetComponent<HandleForResizable>() != null))
                {
                    //if a handle was clicked, do nothing, the handle can manage itself
                }
            }


            // manually handling overlapping elements
            else if (results.Length > 1)
            {
                // if theres a handle visible and being clicked always priorize resizing, since when clicking on handle thats probably what user wants to do
                if (results.Any(r => r.transform.gameObject.GetComponent<HandleForResizable>()))
                {
                  
                    _objectToManuallyResize = results.Where(r => r.transform.gameObject.GetComponent<HandleForResizable>()).First().transform.gameObject;
                    StartManualResizingDueToOverlap(_objectToManuallyResize);

                    return;
                }


                // else always prioritize currently active aoi
                else if (results.Any(r => r.transform.gameObject == ActiveAOI))
                {

                    // if the active AOI is not infront, start movement
                    Debug.Log("Manually started Movement");
                    _manuallyTriggeredMovementDueToOverlap = true;

                    if (!ActiveAOI.GetComponent<DraggableAroundCamera>().IsTryingToMove)
                    {
                        ActiveAOI.GetComponent<DraggableAroundCamera>().OnMouseDown();
                    }

                    return;
                }


                // else just target first aoi hit with raycast
                else if (results.Any(r => r.transform.gameObject.CompareTag("AOI")))
                {
                    SetActiveObject(results.Where(r => r.transform.gameObject.CompareTag("AOI")).First().transform.gameObject);
                }
            }

            // else spawn AOI if no UI clicked
            else
            {
                if (ActiveAOI == null) SpawnAOIatCursorPosition(); else SetActiveObject(null);

            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            if (_manuallyTriggeredResizeDueToOverlap)
            {
                CompleteManualResizingDueToOverlap();
            }

            if (_manuallyTriggeredMovementDueToOverlap)
            {
                _manuallyTriggeredMovementDueToOverlap = false;

                if (ActiveAOI.GetComponent<DraggableAroundCamera>().IsTryingToMove)
                {
                    ActiveAOI.GetComponent<DraggableAroundCamera>().OnMouseUp();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete) && ActiveAOI != null)
        {
            Debug.Log("delete");
            Destroy(ActiveAOI);
        }

    }

    public static void SetActiveObject(GameObject newactive)
    {
        ActiveAOI = newactive;

        NewActiveAoiSubscribers?.Invoke(ActiveAOI);
    }


    void StartManualResizingDueToOverlap(GameObject resizingObject)
    {
        Debug.Log("Manually handling Resizing");
        _manuallyTriggeredResizeDueToOverlap = true;
        _objectToManuallyResize = resizingObject;

        // check if despite overlap object has already registered resizing. only manually override if object hasnt registered resizing itself

        if (!_objectToManuallyResize.transform.root.gameObject.GetComponent<Resizable>().IsTryingToResize)
        {
            _objectToManuallyResize.GetComponent<HandleForResizable>().OnMouseDown();
        }

        if (!_objectToManuallyResize.GetComponent<DraggableAlongXYPlane>().IsTryingToMove)
        {
            _objectToManuallyResize.GetComponent<DraggableAlongXYPlane>().OnMouseDown();
        }
    }

    void CompleteManualResizingDueToOverlap()
    {
        _manuallyTriggeredResizeDueToOverlap = false;

        // check if despite overlap object has already registered resizing completion

        if (_objectToManuallyResize.transform.root.gameObject.GetComponent<Resizable>().IsTryingToResize)
        {
            _objectToManuallyResize.GetComponent<HandleForResizable>().OnMouseUp();
        }

        if (_objectToManuallyResize.GetComponent<DraggableAlongXYPlane>().IsTryingToMove)
        {
            _objectToManuallyResize.GetComponent<DraggableAlongXYPlane>().OnMouseUp();
        }

        _objectToManuallyResize = null;
    }

    void SpawnAOIatCursorPosition()
    {

        // Get the cursor position in screen space
        Vector3 cursorPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SpawnDistance);

        // Project a ray from the camera through the cursor position
        Ray ray = EditorCamera.Camera.ScreenPointToRay(cursorPosition);

        // Calculate the intersection point of the ray with the plane at the spawn distance
        Vector3 spawnPosition = ray.origin + ray.direction * SpawnDistance;

        SpawnAoi(spawnPosition);

    }

    public GameObject SpawnAoi(Vector3 spawnPosition)
    {
        GameObject newAOI = Instantiate(AOIPrefab, spawnPosition, Quaternion.identity);


        newAOI.layer = LayerMask.NameToLayer("AOILayer");

        newAOI.SetActive(true);

        // Calculate the direction vector from the camera to the object's spawn position
        Vector3 direction = spawnPosition - EditorCamera.GameObject.transform.position;

        // Push the object away from the camera towards the final position
        newAOI.transform.position = direction.normalized * SpawnDistance;

        newAOI.transform.LookAt(EditorCamera.GameObject.transform.position);

        // For quads
        if (FlipMesh)
        {
            newAOI.transform.forward *= -1;
        }

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(10f, (newAOI.transform.position - EditorCamera.GameObject.transform.position).magnitude);

        SetActiveObject(newAOI);

        return newAOI;
    }
}


