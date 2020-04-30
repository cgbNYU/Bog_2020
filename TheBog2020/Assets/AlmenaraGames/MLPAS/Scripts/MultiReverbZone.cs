using UnityEngine;
using System.Collections;
using AlmenaraGames;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmenaraGames{
[HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MultiReverbZone")]
[AddComponentMenu("Almenara Games/MLPAS/Multi Reverb Zone",1)]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class MultiReverbZone : MonoBehaviour {

	[Tooltip("The reverb preset applied to the listeners inside this Multi Reverb Zone")]
	/// <summary>
	/// The reverb preset applied to the listeners inside this <see cref="MultiReverbZone"/>.
	/// </summary>
	public AudioReverbPreset ReverbPreset=AudioReverbPreset.Generic;
	[Tooltip("The shape of the collider that this Multi Reverb Zone will going to use")]
	/// <summary>
	/// The shape of the collider that this <see cref="MultiReverbZone"/>  will going to use.
	/// </summary>
	public ZoneType shape=ZoneType.Sphere;
	[Tooltip("The order of this Multi Reverb Zone, useful when placing a Multi Reverb Zone inside of another (Higher values stays on top)")]
	/// <summary>
	/// The order of this <see cref="MultiReverbZone"/>, useful when placing a <see cref="MultiReverbZone"/>  inside of another (Higher values stays on top).
	/// </summary>
	public int order=0;


	public enum ZoneType
	{
		Sphere,
		Box
	}

	[HideInInspector]
	public Collider col;
	
	BoxCollider boxCol;
	SphereCollider sphereCol;

		Vector3 center;
		Vector3 size;
		float radius;

		#if UNITY_EDITOR
		bool added=false;
		#endif

	private bool isApplicationQuitting = false;

	void Awake()
	{
			
			if (Application.isPlaying) {

				col = GetComponent<Collider> ();

				boxCol = col as BoxCollider;
				sphereCol = col as SphereCollider;

				if (shape == ZoneType.Box) {
					size = boxCol.size;
					center = boxCol.center;
					boxCol.size = Vector3.zero;
					boxCol.center = new Vector3(-9999,9999,-9999);
				}

				if (shape == ZoneType.Sphere) {
					radius = sphereCol.radius;
					center = sphereCol.center;
					sphereCol.radius = 0f;
					sphereCol.center = new Vector3(-9999,9999,-9999);
				}

				if (col != null) {
					col.isTrigger = true;
					col.enabled = true;
				}
			} else {

				#if UNITY_EDITOR
				if (!added && GetComponent<Collider> () != null && Selection.activeObject==gameObject && GetComponents<MultiReverbZone> ().Length==1) {
				
					Event e = Event.current;

					if(e.commandName == "Duplicate"){
						return;
					}

					if (!EditorUtility.DisplayDialog("Are you sure to Add this Component?",
						"The Multi Reverb Zone Component will remove any Collider that exists in this Game Object", "Add", "Cancel"))
					{
						DestroyImmediate (this);
						return;
					}

				}

				added = true;
				#endif
				UpdateCollider ();
			}
		
	}

	void OnApplicationQuit () {

		isApplicationQuitting = true;
	}

	void OnDestroy()
	{

			if (isApplicationQuitting || Application.isPlaying)
				return;

	}

	void OnDisable()
	{
			if (!Application.isPlaying)
				return;

		if (isApplicationQuitting)
			return;

			if (MultiPoolAudioSystem.audioManager != null && MultiPoolAudioSystem.audioManager.reverbZones != null) {
				MultiPoolAudioSystem.audioManager.reverbZones.Remove (this);
				MultiPoolAudioSystem.audioManager.reverbZonesCount--;
			}

			MultiAudioManager.Instance.reverbZones.Sort((p1,p2)=>p1.order.CompareTo(p2.order));

	}

	void OnEnable()
	{
			if (!Application.isPlaying)
				return;

			if (MultiPoolAudioSystem.audioManager != null) {
				MultiPoolAudioSystem.audioManager.reverbZones.Add (this);
				MultiPoolAudioSystem.audioManager.reverbZonesCount++;
			} else {
				MultiAudioManager.Instance.reverbZones.Add (this);
				MultiPoolAudioSystem.audioManager.reverbZonesCount++;
			}

			MultiAudioManager.Instance.reverbZones.Sort((p1,p2)=>p1.order.CompareTo(p2.order));

	}

		void UpdateCollider()
		{

			if (shape == ZoneType.Box && (GetComponent<BoxCollider> () == null || GetComponents<Collider>().Length>1)) {
				foreach (var item in GetComponents<Collider>()) {
					DestroyImmediate (item);
				}
				col = gameObject.AddComponent<BoxCollider> ();
			}
			if (shape == ZoneType.Sphere && (GetComponent<SphereCollider> () == null || GetComponents<Collider>().Length>1)) {
				foreach (var item in GetComponents<Collider>()) {
					DestroyImmediate (item);
				}
				col = gameObject.AddComponent<SphereCollider> ();
			}

			boxCol = col as BoxCollider;
			sphereCol = col as SphereCollider;

		}


	void Update()
	{

			if (Application.isPlaying)
				return;

			#if UNITY_2017_1_OR_NEWER

			UpdateCollider ();

			#else

			shape=ZoneType.Sphere;

			UpdateCollider();

			#endif

			if (shape == ZoneType.Box && boxCol!=null) {
				size = boxCol.size;
				center = boxCol.center;
			}

			if (shape == ZoneType.Sphere && sphereCol!=null) {
				radius = sphereCol.radius;
				center = sphereCol.center;
			}

			if (col != null) {
				col.isTrigger = true;
				col.enabled = true;
			}

			if (ReverbPreset == AudioReverbPreset.User)
				ReverbPreset = AudioReverbPreset.Generic;

			if (col == null) {
				col = GetComponent<Collider> ();
			}

	}

		public bool IsInside(Vector3 position)
		{

			if (shape == ZoneType.Box) {
				boxCol.size = size;
				boxCol.center = center;
			}

			if (shape == ZoneType.Sphere) {
				sphereCol.radius = radius;
				sphereCol.center = center;
			}
				

			#if UNITY_2017_1_OR_NEWER

			Vector3 closestPoint = col.ClosestPoint(position);

			#else

			Vector3 thisPosition = transform.position+transform.TransformVector(center);

			Vector3 directionToPosition = new Vector3 (position.x - thisPosition.x, position.y - thisPosition.y, position.z - thisPosition.z);
			float distance = directionToPosition.magnitude;

			#endif

			if (shape == ZoneType.Box) {
				size = boxCol.size;
				center = boxCol.center;
				boxCol.size = Vector3.zero;
				boxCol.center = new Vector3(-9999,9999,-9999);
			}

			if (shape == ZoneType.Sphere) {
				radius = sphereCol.radius;
				center = sphereCol.center;
				sphereCol.radius = 0f;
				sphereCol.center = new Vector3(-9999,9999,-9999);
			}

			#if UNITY_2017_1_OR_NEWER

			if (closestPoint==position) {

				return true;

			}
			
			#else

			if (distance<=radius*GetSphereSize(transform.localScale)) {

			return true;

			}

			#endif

			return false;

		}

		void OnDrawGizmos()
		{

			Gizmos.DrawIcon (transform.position+transform.TransformVector(center), "AlmenaraGames/MLPAS/ReverbZoneNoCenterIco");

			if (!Application.isEditor || col==null)
				return;

			#if UNITY_EDITOR
			if (Selection.activeGameObject == gameObject && !Application.isPlaying)
				return;
			#endif

			Color blueGizmos = new Color (0.69f, 0.89f, 1f, 1);

			Gizmos.color = blueGizmos;

			#if UNITY_EDITOR
			if (shape == ZoneType.Sphere) {
				//SphereCollider volume = col as SphereCollider;
				Matrix4x4 oldHandleMatrix = Handles.matrix;
				Color oldHandleColor = Handles.color;
				//Matrix4x4 localMatrix = Matrix4x4.TRS(volume.transform.position,volume.transform.rotation,GetSphereSize(volume.transform.localScale)*Vector3.one);
				//Handles.matrix = localMatrix;
				blueGizmos.a = 0.5f;
				Handles.color = blueGizmos;
				Vector3 position = transform.position+transform.TransformVector(center);

				if (Camera.current.orthographic) {
					Vector3 normal = position - Handles.inverseMatrix.MultiplyVector (Camera.current.transform.forward);
					float sqrMagnitude = normal.sqrMagnitude;
					float num0 = radius*GetSphereSize(transform.localScale) * radius*GetSphereSize(transform.localScale);
					Handles.DrawWireDisc (position - num0 * normal / sqrMagnitude, normal, radius*GetSphereSize(transform.localScale));
				} else {
					Vector3 normal = position - Handles.inverseMatrix.MultiplyPoint (Camera.current.transform.position);
					float sqrMagnitude = normal.sqrMagnitude;
					float num0 = radius*GetSphereSize(transform.localScale) * radius*GetSphereSize(transform.localScale);
					float num1 = num0 * num0 / sqrMagnitude;
					float num2 = Mathf.Sqrt (num0 - num1);
					Handles.DrawWireDisc (position - num0 * normal / sqrMagnitude, normal, num2);
				}

				Handles.matrix=oldHandleMatrix;
				Handles.color=oldHandleColor;
			}
			#endif

			if (shape == ZoneType.Box && boxCol!=null) {
	

				Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation, transform.localScale);
				Gizmos.DrawWireCube (center, size);
					}

			if (shape == ZoneType.Sphere && sphereCol!=null) {

				//Gizmos.matrix = Matrix4x4.TRS(transform.position,transform.rotation,transform.localScale);
				Gizmos.DrawWireSphere (transform.position+transform.TransformVector(center), radius*GetSphereSize(transform.localScale));


			}



		}

		float GetSphereSize(Vector3 size)
		{

			Vector3 absSize = new Vector3 (Mathf.Abs(size.x),Mathf.Abs(size.y),Mathf.Abs(size.z));

			if (absSize.x > absSize.y && absSize.x > absSize.z)
				return size.x;
			else if (absSize.y > absSize.x && absSize.y > absSize.z)
				return size.y;
			else if (absSize.z > absSize.y && absSize.z > absSize.x)
				return size.z;
			else
				return size.x;

		}

		void OnDrawGizmosSelected()
		{
			Gizmos.DrawIcon (transform.position, "AlmenaraGames/MLPAS/ReverbZoneNoCenterIco");
			Gizmos.DrawIcon (transform.position+transform.TransformVector(center), "AlmenaraGames/MLPAS/ReverbZoneIco");

		}

}


	#if UNITY_EDITOR

	[CustomEditor(typeof(MultiReverbZone)), CanEditMultipleObjects]
	public class ReverbZoneEditor : Editor
	{

		SerializedObject reverbZoneObj;

		private static readonly string[] _dontIncludeMePlaying = new string[]{"m_Script","shape","order"};

	#if UNITY_2017_1_OR_NEWER
		private static readonly string[] _dontIncludeMe = new string[]{"m_Script"};
	#else
		private static readonly string[] _dontIncludeMeOld = new string[]{"m_Script","shape"};
	#endif

		void OnEnable()
		{
			reverbZoneObj = new SerializedObject (targets);
		}

		public override void OnInspectorGUI()
		{
			reverbZoneObj.Update();

	#if UNITY_2017_1_OR_NEWER
			EditorGUILayout.HelpBox("The Reverb Zone Component uses the collider's size and position to know whether a listener is inside of the reverb zone.", MessageType.Info);
	#else
			EditorGUILayout.HelpBox("The Reverb Zone Component uses the collider's size and position to know whether a listener is inside of the reverb zone.", MessageType.Info);
			EditorGUILayout.HelpBox("Box Shape is only available for Unity 2017.1 and newer versions.", MessageType.Info);
	#endif

			if ((target as MultiReverbZone).ReverbPreset == AudioReverbPreset.User) {
				(target as MultiReverbZone).ReverbPreset = AudioReverbPreset.Off;
			}

			if (!Application.isPlaying) {
				#if UNITY_2017_1_OR_NEWER
				DrawPropertiesExcluding (reverbZoneObj, _dontIncludeMe);
				#else
				DrawPropertiesExcluding (reverbZoneObj, _dontIncludeMeOld);
				EditorGUILayout.LabelField ("Shape", (target as MultiReverbZone).shape.ToString ());
				#endif
			} else {
				DrawPropertiesExcluding (reverbZoneObj, _dontIncludeMePlaying);
				EditorGUILayout.LabelField ("Shape", (target as MultiReverbZone).shape.ToString ());
				EditorGUILayout.LabelField ("Order", (target as MultiReverbZone).order.ToString ());
			}

			reverbZoneObj.ApplyModifiedProperties();

		}


	}
	#endif

}
