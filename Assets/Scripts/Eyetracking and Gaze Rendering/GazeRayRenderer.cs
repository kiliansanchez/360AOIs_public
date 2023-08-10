using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using static VRManager;

/// <summary>
/// Derived from SRanipal Example Scripts.
/// This script displays the current gaze of the participant as a colored ray within the experimentor-view of the application.
/// </summary>
public class GazeRayRenderer : MonoBehaviour
{

    public static GazeRayRenderer Instance;

    public int _LengthOfRay = 1000;
    [SerializeField] private LineRenderer _GazeRayRenderer;

    private Color _ray_color= Color.red;

    private GameObject _gazeSphere;
    private Renderer _gazeSphereRenderer;


    void Awake()
    {
        Instance = this;
        VRManager.OnXRStatusUpdate.AddListener(ToggleGazeRenderingOnVrStatusChange);
    }

    /// <summary>
    /// Callback for VRManagers XRStatusUpdate Event. Enables GazeRay when VR is enabled. Disables Gazeray when VR is disabled.
    /// </summary>
    /// <param name="vrStatus"></param>
    private void ToggleGazeRenderingOnVrStatusChange(VRManager.VRStatus vrStatus)
    {
        if (vrStatus == VRManager.VRStatus.Active)
        {
            _gazeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _gazeSphere.layer = VrSettingsMenu.IsGazeVisibleInVr ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("EditorOnly");
            _gazeSphere.GetComponent<Collider>().enabled = false;
            _gazeSphereRenderer = _gazeSphere.GetComponent<Renderer>();
            _gazeSphere.transform.localScale = new Vector3(.25f, .25f, .25f);

            InvokeRepeating(nameof(RenderCurrentGazeAsRay), 0, 0.004f);
        }
        else
        {
            Destroy(_gazeSphere);
            CancelInvoke(nameof(RenderCurrentGazeAsRay));
        }
    }

    

    /// <summary>
    /// Renders current Gaze as ray. Turns ray blue if gaze data is valid, gray of gaze data is invalid.
    /// </summary>
    public void RenderCurrentGazeAsRay()
    {
       


        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
        SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeRecorder.CurrentEyeData);
        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);


        //check if current eye data is invalid, if so, make ray grey -> mostly for study purposes

        ulong flag = 2; //flag for gaze direction validity bit
        //if current eyedata invalid
        if (!((flag & (EyeRecorder.CurrentEyeData.verbose_data.left.eye_data_validata_bit_mask & EyeRecorder.CurrentEyeData.verbose_data.right.eye_data_validata_bit_mask)) == flag))
        {
            _ray_color = Color.gray;
        }
        else
        {
            _ray_color = Color.blue;
        }


        _GazeRayRenderer.startColor = _ray_color;
        _GazeRayRenderer.endColor = _ray_color;


        _GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
        _GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * _LengthOfRay);

        _gazeSphere.transform.position = GazeDirectionCombined.normalized * 10;
        _gazeSphere.layer = this.gameObject.layer;
        _gazeSphereRenderer.material.color = _ray_color;

    }
}
