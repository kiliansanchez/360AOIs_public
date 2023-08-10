using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proof of concept class. Derived from shader code of Unity PanoramicShader.
/// Allows to map the gaze direction onto pixel coordinates of the skybox-texture. Can be used in the future 
/// to create different types of visualizations.
/// </summary>
public class GazeToTextureMapper : MonoBehaviour
{

	public Vector3 direction = new Vector3(25, 0, 0);

	//hardcoded values should be replaced. 
	public int VideoWidth = 4096;
	public int VideoHeight = 2048;

	//currently calculated in OnDrawGizmos. X and Y coordinates in pixels of current gaze on skybox texture.
	public int texelX;
	public int texelY;


    private void OnDrawGizmos()
    {
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(new Vector3(0,0,0), direction);

		var uv = VectorToUV(direction);

		texelX = Mathf.FloorToInt(uv.x * VideoWidth);
		texelY = Mathf.FloorToInt(uv.y * VideoHeight);

		//Debug.Log(texelX + " x " + texelY);
	}

    Vector2 VectorToUV(Vector3 coords)
	{
		Vector3 normalizedCoords = coords.normalized;
		float latitude = Mathf.Acos(normalizedCoords.y);
		float longitude = Mathf.Atan2(normalizedCoords.z, normalizedCoords.x);
		Vector2 sphereCoords = new Vector2(longitude, latitude) * new Vector2(0.5f / Mathf.PI, 1.0f / Mathf.PI);
		return new Vector2(0.5f, 1.0f) - sphereCoords;
	}

}
