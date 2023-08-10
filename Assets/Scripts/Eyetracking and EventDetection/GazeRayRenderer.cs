using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class GazeRayRenderer : MonoBehaviour
{

    public int _LengthOfRay = 1000;
    [SerializeField] private LineRenderer _GazeRayRenderer;

    private Vector3 _fixationStartDirection = Vector3.zero;
    private float _fixationDuration = 0;
    private Color _color_start = Color.red, _color_end = Color.red;

    // Start is called before the first frame update
    void Awake()
    {
        VRManager.XRStatusToggled.AddListener(ToggleGazeRenderingOnVrStatusChange);
    }


    private void ToggleGazeRenderingOnVrStatusChange(bool vrStatus)
    {
        if (vrStatus)
        {
            InvokeRepeating(nameof(RenderCurrentGazeAsRay), 0, 0.004f);
        }
        else
        {
            CancelInvoke(nameof(RenderCurrentGazeAsRay));
        }
    }

    private Vector3 PrevEyeDataGazeDirection()
    {
        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
        SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeRecorder.PreviousEyeData);
        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);

        return GazeDirectionCombined;
    }

    void RenderCurrentGazeAsRay()
    {

        if (!VRManager.isXRactive)
        {
            return;
        }

        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
        SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeRecorder.CurrentEyeData);
        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);


        if (_fixationStartDirection == Vector3.zero)
        {
            _fixationStartDirection = GazeDirectionCombined;
        }
        else
        {
            var angle = Vector3.Angle(GazeDirectionCombined, _fixationStartDirection);
            var velocity = Vector3.Angle(GazeDirectionCombined, PrevEyeDataGazeDirection())
                            / (EyeRecorder.CurrentEyeData.timestamp - EyeRecorder.PreviousEyeData.timestamp) * 1000;

            if (angle <= EventDetection.FixationDispersionThresholdInDegrees && velocity <= EventDetection.FixationVelocityThresholdInDegPerSecond)
            {
                _fixationDuration += EyeRecorder.CurrentEyeData.timestamp - EyeRecorder.PreviousEyeData.timestamp;

                if (_color_start != Color.blue)
                {
                    _color_start = Color.green;
                    _color_end = Color.green;
                }

            }

            else
            {
                _fixationDuration = 0;
                _fixationStartDirection = GazeDirectionCombined;
                _color_start = Color.red;
                _color_end = Color.red;
            }

            if (_fixationDuration > EventDetection.FixationDurationThresholdInMs)
            {
                _color_start = Color.blue;
                _color_end = Color.blue;

                _fixationDuration = 0;
                _fixationStartDirection = GazeDirectionCombined;
            }

        }

        _GazeRayRenderer.startColor = _color_start;
        _GazeRayRenderer.endColor = _color_end;


        _GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
        _GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * _LengthOfRay);


        if (Physics.Raycast(Camera.main.transform.position, GazeDirectionCombined, out RaycastHit hit, 25f))
        {
            // Does the ray intersect any AOIs?
            if (hit.collider.gameObject.TryGetComponent(out Colorable eyetrackable))
            {
                eyetrackable.SetColor(Colorable.AoiColors.ACTIVE);
            }
        }
    }
}
