using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Class that was supposed to allow AOIs to be recolored from AOI List or RightClickMenu. Not implemented though, so class is mostly useless.
/// Currently only handles color of AOI based on whether or not AOI is selected by user.
/// </summary>
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


    protected override void Start()
    {
        _renderer = GetComponent<Renderer>();
        UnityEngine.Assertions.Assert.IsNotNull(_renderer);

        base.Start();
    }

    /// <summary>
    /// set color of AOI to color specified by parameters.
    /// </summary>
    /// <param name="r">red</param>
    /// <param name="g">green</param>
    /// <param name="b">blue</param>
    /// <param name="a">alpha</param>
    private void SetColor(float r, float g, float b, float a)
    {
        _renderer.material.color = new Color(r, g, b, a);
        _renderer.material.shader = Shader.Find("Transparent/Diffuse");
    }

    /// <summary>
    /// set color of AOI based to one of available AoiColors.
    /// </summary>
    /// <param name="color"></param>
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
