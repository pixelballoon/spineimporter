using UnityEngine;
using System.Collections.Generic;
using System;
using Object = System.Object;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpineImporter
{

	[Serializable]
	public class SpineEvent
	{
		[SerializeField] private string _name;

		public int IntParameter;
		public float FloatParameter;
		public string StringParameter;

		public string Name
		{
			get { return _name; }
		}

		public SpineEvent(string name)
		{
			_name = name;
		}
	}
	
	[Serializable]
	public class SpineMesh
	{
		[SerializeField] private Mesh _mesh;
		[SerializeField] private Texture2D _texture;
		[SerializeField] private Transform[] _bones;
		[SerializeField] private Color _color;

		public Mesh Mesh
		{
			get { return _mesh; }
			set { _mesh = value; }
		}

		public Texture2D Texture
		{
			get { return _texture; }
			set { _texture = value; }
		}

		public Transform[] Bones
		{
			get { return _bones; }
			set { _bones = value; }
		}

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}
	}

	public class SpineSkeleton : MonoBehaviour
	{

		[SerializeField] private TextAsset _sourceData;
		[SerializeField] private float _scale = 0.01f;
		[SerializeField] private float _drawOrderOffset = -1f;
		[SerializeField] private string _imagePath;
		[SerializeField] private string _dataPath;
		[SerializeField] private string _activeSkin;
		[SerializeField] private List<SpineSkin> _skins;
		[SerializeField] private List<SpineEvent> _events;

		private Transform _skeleton;

		public TextAsset SourceData
		{
			get { return _sourceData; }
			set { _sourceData = value; }
		}

		public string ImagePath
		{
			get { return _imagePath; }
			set { _imagePath = value; }
		}

		public string DataPath
		{
			get { return _dataPath; }
			set { _dataPath = value; }
		}

		public string ActiveSkin
		{
			get { return _activeSkin; }
			set { _activeSkin = value; }
		}

		public void Refresh()
		{
			if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab)
			{
				GameObject go = Instantiate(gameObject) as GameObject;
				go.GetComponent<SpineSkeleton>().Refresh();
				string prefabPath = AssetDatabase.GetAssetPath(gameObject);

				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof (GameObject));
				PrefabUtility.ReplacePrefab(go, prefab);

				DestroyImmediate(go);

				return;
			}

			_skeleton = transform.FindChild("skeleton");

			if (!_skeleton)
			{
				_skeleton = new GameObject("skeleton").transform;
				_skeleton.parent = transform;
			}

			_skeleton.localScale = Vector3.one * _scale;
			
			using (TextReader reader = new StringReader(_sourceData.text))
			{
				var root = Json.Deserialize(reader) as Dictionary<String, Object>;
				if (root == null)
				{
					throw new Exception("Invalid JSON.");
				}

				ParseBones(root);
				ParseSlots(root);
				ParseSkins(root);
				ParseEvents(root);
				ParseAnimations(root);
			}

			SetSkin(_activeSkin);
		}

		private void SetSkin(SpineSlot[] slots, SpineSkin skin)
		{
			foreach (SpineSkin.Slot slot in skin.Slots)
			{
				for (int i = 0; i < slots.Length; i++)
				{
					if (slots[i].name == (slot.Name + " [slot]"))
					{
						SpineAttachment attachment = slot.GetAttachment(slots[i].DefaultAttachment);
						if (attachment != null)
						{
							slots[i].SetAttachment(attachment);
						}
					}
				}
			}
		}

		public void SetSkin(string name)
		{
			SpineSlot[] slots = GetComponentsInChildren<SpineSlot>();

			SpineSkin defaultSkin = GetSkin("default", false);
			SpineSkin activeSkin = GetSkin(_activeSkin, false);

			if (defaultSkin == null || activeSkin == null)
				return;

			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].Clear();
			}

			if (defaultSkin != null)
			{
				SetSkin(slots, defaultSkin);
			}

			if (activeSkin != null)
			{
				SetSkin(slots, activeSkin);
			}
		}

		private void ParseBones(Dictionary<String, Object> root)
		{
			int index = 0;

			foreach (Dictionary<String, Object> boneMap in (List<Object>)root["bones"])
			{
				string boneName = GetString(boneMap, "name", "");
				
				Transform bone = GetBone(_skeleton, boneName);
				
				Transform parent = FindChildRecursive(_skeleton.transform, GetString(boneMap, "parent", ""));
				
				if (!parent)
				{
					parent = _skeleton.transform;
				}
				
				bone.parent = parent;
				
				bone.localPosition = new Vector3(GetFloat(boneMap, "x", 0), GetFloat(boneMap, "y", 0), 0);
				bone.localScale = new Vector3(GetFloat(boneMap, "scaleX", 1), GetFloat(boneMap, "scaleY", 1), 1);
				bone.localEulerAngles = new Vector3(0, 0, GetFloat(boneMap, "rotation", 0));
				
				SpineBone spineBone = bone.GetComponent<SpineBone>();
				if (!spineBone)
				{
					spineBone = bone.gameObject.AddComponent<SpineBone>();
				}
				spineBone.Index = index;
				spineBone.Length = GetFloat(boneMap, "length", 0);

				index++;
			}
		}

		private void ParseSkins(Dictionary<String, Object> root)
		{
			bool setSkin = string.IsNullOrEmpty(_activeSkin);

			if (_skins == null)
			{
				_skins = new List<SpineSkin>();
			}
			else
			{
				_skins.Clear();
			}

			if (root.ContainsKey("skins"))
			{
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["skins"])
				{
					string skinName = entry.Key;

					SpineSkin skin = GetSkin(skinName);

					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)entry.Value)
					{
						string slotName = slotEntry.Key;

						SpineSkin.Slot slot = skin.GetSlot(slotName);

						foreach (KeyValuePair<String, Object> attachmentEntry in ((Dictionary<String, Object>)slotEntry.Value))
						{
							string attachmentName = attachmentEntry.Key;
							ParseAttachment(attachmentName, slot, (Dictionary<String, Object>)attachmentEntry.Value);
						}
					}

					if (setSkin == true && (string.IsNullOrEmpty(_activeSkin) || skinName == "default"))
					{
						_activeSkin = skinName;
					}
				}
			}
		}

		private SpineSlot GetSlot(string name)
		{
			SpineSlot[] slots = GetComponentsInChildren<SpineSlot>();

			foreach (SpineSlot slot in slots)
			{
				if (slot.name == name + " [slot]")
				{
					return slot;
				}
			}

			return null;
		}

		private SpineSkin GetSkin(string name, bool create = true)
		{
			foreach (SpineSkin skin in _skins)
			{
				if (skin.Name == name)
				{
					return skin;
				}
			}

			if (!create)
			{
				return null;
			}

			SpineSkin spineSkin = new SpineSkin(name);
			_skins.Add(spineSkin);
			return spineSkin;
		}

		private SpineEvent GetEvent(string name)
		{
			if (_events != null)
			{
				foreach (SpineEvent spineEvent in _events)
				{
					if (spineEvent.Name == name)
					{
						return spineEvent;
					}
				}
			}

			return null;
		}

		private void ParseEvents(Dictionary<String, Object> root)
		{
			if (root.ContainsKey("events"))
			{
				if (_events == null)
				{
					_events = new List<SpineEvent>();
				}

				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["events"])
				{
					var entryMap = (Dictionary<String, Object>)entry.Value;
					SpineEvent spineEvent = new SpineEvent(entry.Key);
					spineEvent.IntParameter = GetInt(entryMap, "int", 0);
					spineEvent.FloatParameter = GetFloat(entryMap, "float", 0);
					spineEvent.StringParameter = GetString(entryMap, "string", null);
					_events.Add(spineEvent);
				}
			}
		}

		private Sprite GetSprite(String name)
		{
			string assetPath = _imagePath + name + ".png";
			return (Sprite)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite));
		}

		private Texture2D GetTexture(String name)
		{
			string assetPath = _imagePath + name + ".png";
			return (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
		}

		private SpineBone FindBoneByIndex(int index)
		{
			SpineBone[] bones = _skeleton.GetComponentsInChildren<SpineBone>();

			foreach (SpineBone bone in bones)
			{
				if (bone.Index == index)
				{
					return bone;
				}
			}

			return null;
		}

		private void ParseAttachment(string name, SpineSkin.Slot skinSlot, Dictionary<String, Object> root)
		{
			switch (GetString(root, "type", "region"))
			{
				case "region":
				{
					SpineAttachment attachment = skinSlot.GetOrCreateAttachment(name, SpineAttachment.AttachmentType.Region);
					attachment.Sprite = GetSprite(GetString(root, "name", name));
					attachment.PositionOffset = new Vector2(GetFloat(root, "x", 0f), GetFloat(root, "y", 0f));
					attachment.ScaleOffset = new Vector2(GetFloat(root, "scaleX", 1f), GetFloat(root, "scaleY", 1f));
					attachment.RotationOffset = GetFloat(root, "rotation", 0f);
					break;
				}

				case "mesh":
				{
					Mesh mesh = new Mesh();
					mesh.vertices = GetVector3Array(root, "vertices", 2);
					mesh.uv = GetVector2Array(root, "uvs");
					mesh.triangles = GetIntArray(root, "triangles");

					SpineMesh spineMesh = new SpineMesh();
					spineMesh.Mesh = mesh;
					spineMesh.Color = ToColor(GetString(root, "color", "ffffffff"));
					spineMesh.Texture = GetTexture(GetString(root, "name", name));

					SpineAttachment attachment = skinSlot.GetOrCreateAttachment(name, SpineAttachment.AttachmentType.Mesh);
					attachment.Mesh = spineMesh;
					
					break;
				}

				case "skinnedmesh":
				{
					SpineSlot slot = GetSlot(name);

					Vector2[] uvs = GetVector2Array(root, "uvs");
					int[] triangles = GetIntArray(root, "triangles");
					List<Vector3> vertices = new List<Vector3>(uvs.Length);
					float[] meshData = GetFloatArray(root, "vertices", 1);
					
					List<BoneWeight> weights = new List<BoneWeight>();

					List<Transform> bones = new List<Transform>();
					
					for (int i = 0, n = meshData.Length; i < n; )
					{
						int boneCount = (int)meshData[i++];

						BoneWeight boneWeight = new BoneWeight();

						for (int j = 0; j < boneCount; j++)
						{
							int globalBoneIndex = (int)meshData[i];

							SpineBone bone = FindBoneByIndex(globalBoneIndex);

							float x = meshData[i + 1];
							float y = meshData[i + 2];
							float weight = meshData[i + 3];

							int boneIndex;
							if (bones.Contains(bone.transform))
							{
								boneIndex = bones.IndexOf(bone.transform);
							}
							else
							{
								boneIndex = bones.Count;
								bones.Add(bone.transform);
							}
							
							if (j == 0)
							{
								Vector3 position = bone.transform.TransformPoint(x, y, slot.DrawOrder);
								vertices.Add(position);
							}

							switch (j)
							{
								case 0:
								{
									boneWeight.boneIndex0 = boneIndex;
									boneWeight.weight0 = weight;
									break;
								}
								case 1:
								{
									boneWeight.boneIndex1 = boneIndex;
									boneWeight.weight1 = weight;
									break;
								}
								case 2:
								{
									boneWeight.boneIndex2 = boneIndex;
									boneWeight.weight2 = weight;
									break;
								}
								case 3:
								{
									boneWeight.boneIndex3 = boneIndex;
									boneWeight.weight3 = weight;
									break;
								}
							}
							
							i += 4;
						}

						weights.Add(boneWeight);
					}
					
					Mesh mesh = new Mesh();
					mesh.vertices = vertices.ToArray();
					mesh.uv = uvs;
					mesh.triangles = triangles;
					mesh.boneWeights = weights.ToArray();
					Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
					for (int i = 0; i < bones.Count; i++)
					{
						bindPoses[i] = bones[i].worldToLocalMatrix;
					}
					mesh.bindposes = bindPoses;

					SpineMesh spineMesh = new SpineMesh();
					spineMesh.Mesh = mesh;
					spineMesh.Color = ToColor(GetString(root, "color", "ffffffff"));
					spineMesh.Texture = GetTexture(GetString(root, "name", name));
					spineMesh.Bones = bones.ToArray();

					SpineAttachment attachment = skinSlot.GetOrCreateAttachment(name, SpineAttachment.AttachmentType.SkinnedMesh);
					attachment.Mesh = spineMesh;

					break;
				}
			}
		}

		private void ParseSlots(Dictionary<String, Object> root)
		{
			if (root.ContainsKey("slots"))
			{
				float drawOrder = 0f;

				foreach (Dictionary<String, Object> slotMap in (List<Object>)root["slots"])
				{
					var slotName = (String)slotMap["name"];
					var boneName = (String)slotMap["bone"];

					Transform bone = FindChildRecursive(_skeleton, boneName);

					SpineSlot slot = null;
					Transform slotTransform = bone.FindChild(slotName + " [slot]");
					if (!slotTransform)
					{
						GameObject slotGo = new GameObject(slotName + " [slot]");
						slot = slotGo.AddComponent<SpineSlot>();
						slotGo.transform.parent = bone;
					}
					else
					{
						slot = slotTransform.GetComponent<SpineSlot>();
					}

					slot.Clear();
					slot.DefaultAttachment = GetString(slotMap, "attachment", "");
					slot.AdditiveBlending = GetBoolean(slotMap, "additive", false);
					slot.Color = ToColor(GetString(slotMap, "color", "ffffffff"));
					slot.DrawOrder = drawOrder;

					drawOrder += _drawOrderOffset;
				}
			}
		}

		private void ParseAnimations(Dictionary<String, Object> root)
		{
			if (root.ContainsKey("animations"))
			{
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["animations"])
				{
					ParseAnimation(entry.Key, (Dictionary<String, Object>)entry.Value);
				}
			}
		}

		private void ParseAnimation(String name, Dictionary<String, Object> map)
		{
			bool isNewClip = false;

			AnimationClip clip = GetAndClearAnimationClip(name, ref isNewClip);

			if (map.ContainsKey("bones"))
			{
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["bones"])
				{
					String boneName = entry.Key;

					Transform bone = FindChildRecursive(_skeleton, boneName);

					ParseBoneTimelines(clip, bone, (Dictionary<String, Object>)entry.Value);
				}
			}

			if (map.ContainsKey("events"))
			{
				var eventsMap = (List<Object>)map["events"];

				List<AnimationEvent> animationEvents = new List<AnimationEvent>();

				foreach (Dictionary<String, Object> eventMap in eventsMap)
				{
					string eventName = (string)eventMap["name"];

					SpineEvent spineEvent = GetEvent(eventName);
					if (spineEvent == null)
					{
						continue;
					}

					AnimationEvent animationEvent = new AnimationEvent();
					animationEvent.time = (float)eventMap["time"];
					animationEvent.functionName = eventName;
					animationEvent.intParameter = GetInt(eventMap, "int", spineEvent.IntParameter);
					animationEvent.floatParameter = GetFloat(eventMap, "float", spineEvent.FloatParameter);
					animationEvent.stringParameter = GetString(eventMap, "string", spineEvent.StringParameter);

					animationEvents.Add(animationEvent);
				}

				AnimationUtility.SetAnimationEvents(clip, animationEvents.ToArray());
			}

			UpdateClipSettings(clip, isNewClip);
		}

		private string GetPath(Transform t)
		{
			return GetPathInternal(t).Substring(1);
		}

		private string GetPathInternal(Transform t)
		{
			if (t == null || t.parent == null || t.GetComponent<SpineSkeleton>() != null)
			{
				return "";
			}

			return GetPathInternal(t.parent) + "/" + t.name;
		}

		private void ParseBoneTimelines(AnimationClip clip, Transform bone, Dictionary<String, Object> timelineMap)
		{
			foreach (KeyValuePair<String, Object> timelineEntry in timelineMap)
			{
				string timelineName = timelineEntry.Key;
				var values = (List<Object>)timelineEntry.Value;

				switch (timelineName)
				{
					case "rotate":
					{
						AnimationCurve rotX = new AnimationCurve();
						AnimationCurve rotY = new AnimationCurve();
						AnimationCurve rotZ = new AnimationCurve();
						AnimationCurve rotW = new AnimationCurve();

						foreach (Dictionary<String, Object> valueMap in values)
						{
							float time = (float)valueMap["time"];
							float value = (float)valueMap["angle"];

							Quaternion quaternion = Quaternion.Euler(0, 0, value + bone.localEulerAngles.z);

							rotX.AddKey(new Keyframe(time, quaternion.x));
							rotY.AddKey(new Keyframe(time, quaternion.y));
							rotZ.AddKey(new Keyframe(time, quaternion.z));
							rotW.AddKey(new Keyframe(time, quaternion.w));
						}

						clip.SetCurve(GetPath(bone), typeof(Transform), "localRotation.x", rotX);
						clip.SetCurve(GetPath(bone), typeof(Transform), "localRotation.y", rotY);
						clip.SetCurve(GetPath(bone), typeof(Transform), "localRotation.z", rotZ);
						clip.SetCurve(GetPath(bone), typeof(Transform), "localRotation.w", rotW);

						break;
					}
					case "scale":
						goto case "translate";
					case "translate":
					{
						AnimationCurve x = new AnimationCurve();
						AnimationCurve y = new AnimationCurve();
						AnimationCurve z = new AnimationCurve();

						string propertyName;

						if (timelineName == "scale")
						{
							propertyName = "localScale";

							foreach (Dictionary<String, Object> valueMap in values)
							{
								float time = (float) valueMap["time"];

								x.AddKey(new Keyframe(time, (float) valueMap["x"] * bone.localScale.x));
								y.AddKey(new Keyframe(time, (float) valueMap["y"] * bone.localScale.y));
								z.AddKey(new Keyframe(time, bone.localScale.z));
							}
						}
						else
						{
							propertyName = "localPosition";

							foreach (Dictionary<String, Object> valueMap in values)
							{
								float time = (float)valueMap["time"];

								x.AddKey(new Keyframe(time, (float)valueMap["x"] + bone.localPosition.x));
								y.AddKey(new Keyframe(time, (float)valueMap["y"] + bone.localPosition.y));
								z.AddKey(new Keyframe(time, bone.localPosition.z));
							}
						}

						clip.SetCurve(GetPath(bone), typeof(Transform), propertyName + ".x", x);
						clip.SetCurve(GetPath(bone), typeof(Transform), propertyName + ".y", y);
						clip.SetCurve(GetPath(bone), typeof(Transform), propertyName + ".z", z);

						break;
					}
				}
			}

			clip.EnsureQuaternionContinuity();
		}

		private AnimationClip GetAndClearAnimationClip(string name, ref bool isNew)
		{
			string animPath = _dataPath + name + ".anim";

			AnimationClip clip = AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip)) as AnimationClip;

			if (!clip)
			{
				clip = new AnimationClip();

				isNew = true;

				AssetDatabase.CreateAsset(clip, animPath);
			}
			else
			{
				EditorUtility.CopySerialized(new AnimationClip(), clip);
			}

			clip.name = name;

			return clip;
		}
		
		private void UpdateClipSettings(AnimationClip clip, bool isNewClip)
		{
			clip.wrapMode = WrapMode.Loop;
			
			AnimationUtility.SetAnimationType(clip, ModelImporterAnimationType.Generic);
			
			AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
			settings.loopTime = true;
			settings.startTime = 0f;
			// If animation is a single keyframe then make sure stop time is a small value rather than 0,
			// as Unity will otherwise default to an animation length of 1 second
			settings.stopTime = Mathf.Max(clip.length, 0.01f);

			SpineUtilities.SetAnimationSettings(clip, settings);

			// Add animation clip to layer 0 of the animator controller
			UnityEditorInternal.AnimatorController controller = transform.GetComponent<Animator>().runtimeAnimatorController as UnityEditorInternal.AnimatorController;
			
			if (controller && isNewClip)
			{
				UnityEditorInternal.AnimatorController.AddAnimationClipToController(controller, clip);
			}
		}

		private static Transform GetBone(Transform skeleton, string name)
		{
			Transform bone = FindChildRecursive(skeleton, name);

			if (!bone)
			{
				bone = new GameObject(name).transform;
			}

			return bone;
		}

		private static Transform FindChildRecursive(Transform t, string name)
		{
			Transform[] transforms = t.GetComponentsInChildren<Transform>(true);
			
			foreach (Transform transform in transforms)
			{
				if (transform.gameObject.name == name)
				{
					return transform;
				}
			}
			
			return null;
		}
		
		private static Vector2[] GetVector2Array(Dictionary<String, Object> map, String name)
		{
			var list = (List<Object>)map[name];
			var values = new Vector2[list.Count / 2];
			
			for (int i = 0, n = list.Count; i < n; i+=2)
			{
				values[i/2] = new Vector2((float)list[i], 1f - (float)list[i+1]);
			}
			
			return values;
		}

		private static Vector3[] GetVector3Array(Dictionary<String, Object> map, String name, int stride)
		{
			var list = (List<Object>)map[name];
			var values = new Vector3[list.Count / stride];

			if (stride == 3)
			{
				for (int i = 0, n = list.Count; i < n; i+=3)
				{
					values[i/3] = new Vector3((float)list[i], (float)list[i+1], (float)list[i+2]);
				}
        	}
			else if (stride == 2)
			{
				for (int i = 0, n = list.Count; i < n; i+=2)
				{
					values[i/2] = new Vector3((float)list[i], (float)list[i+1], 0);
				}
			}
		
			return values;
		}
		
		public static Color ToColor(string hexString)
		{
			return new Color(ToColor(hexString, 0), ToColor(hexString, 1), ToColor(hexString, 2), ToColor(hexString, 3));
		}

		#region Helper functions taken from Spine-C# SkeletonJson.cs

		private static float[] GetFloatArray(Dictionary<String, Object> map, String name, float scale)
		{
			var list = (List<Object>) map[name];
			var values = new float[list.Count];
			if (scale == 1)
			{
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float) list[i];
			}
			else
			{
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float) list[i] * scale;
			}
			return values;
		}

		private static int[] GetIntArray(Dictionary<String, Object> map, String name)
		{
			var list = (List<Object>) map[name];
			var values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int) (float) list[i];
			return values;
		}

		private static float GetFloat(Dictionary<String, Object> map, String name, float defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float) map[name];
		}

		private static int GetInt(Dictionary<String, Object> map, String name, int defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int) (float) map[name];
		}

		private static bool GetBoolean(Dictionary<String, Object> map, String name, bool defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool) map[name];
		}

		private static String GetString(Dictionary<String, Object> map, String name, String defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (String) map[name];
		}

		public static float ToColor(String hexString, int colorIndex)
		{
			if (hexString.Length != 8)
				throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString);
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float) 255;
		}

		#endregion

	}

}
