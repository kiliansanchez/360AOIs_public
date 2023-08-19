using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Unity.VisualScripting;



/// <summary>
/// This class does 3 things
/// 1.Manages AOIs based on user input
///     a) Deactivates currently active AOI and spawns new AOIs if user clicks on empty space in 360 degree video
///     b) Sets an AOI as active when user clicks on AOI in 360 degree video and deactivates the previously selected AOI 
///
/// 2. Manages AOI component behaviour (specifically resizing and movement of AOIs) when multiple AOIs overlap
///     a) prioritizes active AOI over inactive AOIs
///     b) prioritizes resizing over movement
///     
/// 3. Disables AOIs during PreVideoStimulus TestScene so that editor doesnt accidentally edit AOIs while they're not visible.
/// </summary>
public class AOIManager : MonoBehaviour
{
    public static AOIManager Instance;
    public static GameObject ActiveAOI = null;

    public static UnityEvent<GameObject> NewActiveAOI = new();

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


    void Start()
    {
        _canvas = GameObject.Find("UI Canvas");
        UnityEngine.Assertions.Assert.IsNotNull(_canvas);
        VrSettingsMenu.OnIsPreVideoTestSceneEnabledToggled.AddListener(OnTestSceneToggled);
        VRManager.OnXRStatusUpdate.AddListener(OnVrStatusChange);
    }

    /// <summary>
    /// Callback for VRManagers status change event. When VR is being enabled or disabled it toggles
    /// AOI Colliders based on whether the TestScene is being used.
    /// </summary>
    /// <param name="status">New VR Status</param>
    private void OnVrStatusChange(VRManager.VRStatus status)
    {
        if (VrSettingsMenu.IsPreVideoTestSceneEnabled && status == VRManager.VRStatus.Active)
        {
            ToggleAOIColliders(false);
        }
        else
        {
            ToggleAOIColliders(true);
        }
    }

    /// <summary>
    /// Searches for all AOI Objects and disabled or enables their colliders based on target_collider_state parameter.
    /// </summary>
    /// <param name="target_collider_state">New target state for colliders, true = enabled, false = disabled.</param>
    public void ToggleAOIColliders(bool target_collider_state)
    {

        var visible_aois = GameObject.FindGameObjectsWithTag("AOI");
        var invisible_aois = GameObject.FindGameObjectsWithTag("AOI_Invisible");

        if (visible_aois.Count() != 0)
        {
            foreach (var aoi in visible_aois)
            {
                aoi.GetComponent<Collider>().enabled = target_collider_state;
            }
        }

        if (invisible_aois.Count() != 0)
        {
            foreach (var aoi in invisible_aois)
            {
                aoi.GetComponent<Collider>().enabled = target_collider_state;
            }
        }
        
    }

    /// <summary>
    /// Callback for VrSettingsMenu TestScene toggle. Reacts to user toggling whether or not they want to use a testscene
    /// before displaying the 360 degree video.
    /// </summary>
    /// <param name="isEnabled"></param>
    private void OnTestSceneToggled(bool isEnabled)
    {
        if (!isEnabled)
        {
            ToggleAOIColliders(true);
        }
        else
        {
            if (!EyeRecorder.IsRecording && VRManager.CurrentVRStatus == VRManager.VRStatus.Active)
            {
                ToggleAOIColliders(false);
            }
        }
    }

    /// <summary>
    /// The Update function is currently pretty complicated, because it not only handles the spawning of new AOIs, but also
    /// handles what AOI/AOI-Handle receives MouseDown events when multiple AOIs overlap.
    /// </summary>
    void Update()
    {
        if (!VideoManager.IsInitialized)
        {
            return;
        }

        if (VrSettingsMenu.IsPreVideoTestSceneEnabled && VRManager.CurrentVRStatus == VRManager.VRStatus.Active && !EyeRecorder.IsRecording)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !RightClickMenu.IsCursorOverUI())
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

                    // if the active AOI is not infront, start movement manually
                    Debug.Log("Manually started Movement");
                    _manuallyTriggeredMovementDueToOverlap = true;

                    if (!ActiveAOI.GetComponent<DraggableAroundCamera>().IsTryingToMove)
                    {
                        ActiveAOI.GetComponent<DraggableAroundCamera>().OnMouseDown();
                    }

                    return;
                }


                // if no currently active aoi, just target first aoi hit with raycast
                else if (results.Any(r => r.transform.gameObject.CompareTag("AOI")))
                {
                    SetActiveObject(results.Where(r => r.transform.gameObject.CompareTag("AOI")).First().transform.gameObject);
                }
            }

            // if nothing hit by raycast, deselct currently selected AOI. if no active AOI spawn new AOI
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

    /// <summary>
    /// Sets active AOI after use has clicked a new AOI and invokes NewActiveAOI Event.
    /// </summary>
    /// <param name="newactive">The last Object the user has clicked that should be set as active.</param>
    public static void SetActiveObject(GameObject newactive)
    {
        ActiveAOI = newactive;

        NewActiveAOI?.Invoke(ActiveAOI);
    }

    /// <summary>
    /// If objects overlap this method sends the OnMouseDown event to the correct scripts manually.
    /// </summary>
    /// <param name="resizingObject">Object of which components need to manually receive OnMouseDown-Events</param>
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

    /// <summary>
    /// After resizing is completed this method manually calls the OnMouseUp-Event on all relevant components
    /// of the last object that was resized.
    /// </summary>
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

    /// <summary>
    /// Calculates worldspace-position from cursor for spawning a new AOI.
    /// </summary>
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

    /// <summary>
    /// instantiates new AOIPrefab object at specified spawn position.
    /// </summary>
    /// <param name="spawnPosition">Position to instantiate new object at.</param>
    /// <returns></returns>
    public GameObject SpawnAoi(Vector3 spawnPosition)
    {
        GameObject newAOI = Instantiate(AOIPrefab, spawnPosition, Quaternion.identity);


        newAOI.layer = LayerMask.NameToLayer("AOILayer");

        newAOI.SetActive(true);

        // Calculate the direction vector from the camera to the object's spawn position
        Vector3 direction = spawnPosition - EditorCamera.EditorCamera_GameObject.transform.position;

        // Push the object away from the camera towards the final position
        newAOI.transform.position = direction.normalized * SpawnDistance;

        newAOI.transform.LookAt(EditorCamera.EditorCamera_GameObject.transform.position);

        // For quads
        if (FlipMesh)
        {
            newAOI.transform.forward *= -1;
        }

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(10f, (newAOI.transform.position - EditorCamera.EditorCamera_GameObject.transform.position).magnitude);

        SetActiveObject(newAOI);

        return newAOI;
    }
}


