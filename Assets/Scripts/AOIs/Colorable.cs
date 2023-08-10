using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*

Colorable is going to handle the recoloring  of the AOI, not super useful in the current state of the program

 */

public class Colorable : AOIComponent
{

    private Renderer _renderer;


    public enum AoiColors
    {
        ACTIVE,
        INACTIVE,
        INVISIBLE
    }

    AoiColors _color;

    // Start is called before the first frame update
    protected override void Start()
    {
        _renderer = GetComponent<Renderer>();
        UnityEngine.Assertions.Assert.IsNotNull(_renderer);

        base.Start();
    }



    private void SetColor(float r, float g, float b, float a)
    {
        _renderer.material.color = new Color(r, g, b, a);
        _renderer.material.shader = Shader.Find("Transparent/Diffuse");
    }

    public void SetColor(AoiColors color)
    {
        switch (color)
        {
            case AoiColors.ACTIVE:
                SetColor(1,0,1,.3f);
                _color = AoiColors.ACTIVE;
                break;
            case AoiColors.INACTIVE:
                _color = AoiColors.INACTIVE;
                SetColor(0, 1, 0, .3f);
                break;
            case AoiColors.INVISIBLE:
                _color = AoiColors.INVISIBLE;
                SetColor(0, 0, 1, .3f);
                break;
            default:
                break;
        }
    }

    protected override void OnActivate()
    {
        SetColor(AoiColors.ACTIVE);
    }

    protected override void OnDeactivate()
    {
        SetColor(AoiColors.INACTIVE);
    }
}
