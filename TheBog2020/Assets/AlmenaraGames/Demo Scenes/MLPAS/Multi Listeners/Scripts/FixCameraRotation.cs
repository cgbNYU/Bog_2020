using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames.Demo{
public class FixCameraRotation : MonoBehaviour {

	//This is only use to avoid the camera rotation

	void LateUpdate()
	{

		transform.eulerAngles = Vector3.zero;

	}
}
}