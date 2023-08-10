using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeToTextureMapper : MonoBehaviour
{

	public Vector3 direction = new Vector3(25, 0, 0);
	public int texelX;
	public int texelY;

	private void Start()
	{ 

		
	}

    private void OnDrawGizmos()
    {
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(new Vector3(0,0,0), direction);

		var uv = VectorToUV(direction);

		texelX = Mathf.FloorToInt(uv.x * 4096);
		texelY = Mathf.FloorToInt(uv.y * 2048);

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
