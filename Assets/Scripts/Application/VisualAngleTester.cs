using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAngleTester : MonoBehaviour
{

    public Vector3 TestDirection;


    public float phi = 1;
    public float distance = 10;
    public float size;

    GameObject _angle_sphere;
    public Color Color;
    private MeshRenderer _meshRenderer;
    private DraggableAroundCamera _movementComponent;


    private void Start()
    {
        _angle_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _angle_sphere.layer = LayerMask.NameToLayer("EditorOnly");
        //_angle_sphere.GetComponent<Collider>().enabled = false;
        _meshRenderer = _angle_sphere.GetComponent<MeshRenderer>();

        _angle_sphere.tag = "AOI";
        _movementComponent = _angle_sphere.AddComponent<DraggableAroundCamera>();
        _movementComponent.MovementCompleted.AddListener(OnMovementCompleted);

        Color = new(1,0,0);
        TestDirection = new(.1f,.1f,1);
    }

    private void Update()
    {

        if (!_movementComponent.IsTryingToMove)
        {
            size = ((2 * distance) * Mathf.Tan(Mathf.Deg2Rad * phi / 2));
            _angle_sphere.transform.localScale = new Vector3(size, size, size);
            _angle_sphere.transform.position = TestDirection.normalized * distance;
            _meshRenderer.material.color = Color;
        }
     
    }


    void OnMovementCompleted()
    {
        TestDirection = _angle_sphere.transform.position - EditorCamera.EditorCamera_GameObject.transform.position;
        distance = (_angle_sphere.transform.position - EditorCamera.EditorCamera_GameObject.transform.position).magnitude;     
    }
}
