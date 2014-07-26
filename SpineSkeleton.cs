using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = System.Object;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpineImporter
{

	public class SpineSkeleton : MonoBehaviour
	{

		[SerializeField] private TextAsset _sourceData;
		[SerializeField] private float _scale = 0.05f;
		[SerializeField] private float _drawOrderOffset = -0.01f;
		[SerializeField] private string _imagePath;
		[SerializeField] private string _dataPath;
		[SerializeField] private string _activeSkin;
		[SerializeField] private List<SpineSkin> _skins;

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
				spineBone.Length = GetFloat(boneMap, "length", 0);
			}
		}

		private void ParseSkins(Dictionary<String, Object> root)
		{
			bool setSkin = string.IsNullOrEmpty(_activeSkin);

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

		private SpineSkin GetSkin(string name, bool create = true)
		{
			if (_skins == null)
			{
				_skins = new List<SpineSkin>();
			}

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

		private Sprite GetSprite(String name)
		{
			string assetPath = _imagePath + name + ".png";
			return (Sprite)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite));
		}

		private void ParseAttachment(string name, SpineSkin.Slot slot, Dictionary<String, Object> root)
		{
			switch (GetString(root, "type", "region"))
			{
				case "region":
				{
					SpineAttachment attachment = slot.GetOrCreateAttachment(name, SpineAttachment.AttachmentType.Region);
					attachment.Sprite = GetSprite(GetString(root, "name", name));
					attachment.PositionOffset = new Vector2(GetFloat(root, "x", 0f), GetFloat(root, "y", 0f));
					attachment.RotationOffset = GetFloat(root, "rotation", 0f);
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

					slot.transform.localPosition = Vector3.zero;
					slot.transform.localScale = new Vector3(100, 100, 1);

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
			AnimationClip clip = GetAndClearAnimationClip(name);

			if (map.ContainsKey("bones"))
			{
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["bones"])
				{
					String boneName = entry.Key;

					Transform bone = FindChildRecursive(_skeleton, boneName);

					ParseBoneTimelines(clip, bone, (Dictionary<String, Object>)entry.Value);
				}
			}

			UpdateClipSettings(clip);
		}

		private string GetPath(Transform t)
		{
			return GetPathInternal(t).Substring(1);
		}

		private string GetPathInternal(Transform t)
		{
			if (t == null || t.parent == null)
			{
				return "";
			}

			return GetPathInternal(t.parent) + "/" + t.name;
		}

		private void ParseBoneTimelines(AnimationClip clip, Transform bone, Dictionary<String, Object> timelineMap)
		{
			foreach (KeyValuePair<String, Object> timelineEntry in timelineMap)
			{
				var timelineName = (String)timelineEntry.Key;
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
						
						Vector3 offset = (timelineName == "translate") ? bone.localPosition : bone.localScale;
						
						foreach (Dictionary<String, Object> valueMap in values)
						{
							float time = (float)valueMap["time"];
							
							x.AddKey(new Keyframe(time, (float)valueMap["x"] + offset.x));
							y.AddKey(new Keyframe(time, (float)valueMap["y"] + offset.y));
						}
						
						string propertyName = "local" + (timelineName == "translate" ? "Position" : "Scale");
						
						clip.SetCurve(GetPath(bone), typeof(Transform), propertyName + ".x", x);
						clip.SetCurve(GetPath(bone), typeof(Transform), propertyName + ".y", y);

						break;
					}
				}
			}

			clip.EnsureQuaternionContinuity();
		}

		private AnimationClip GetAndClearAnimationClip(string name)
		{
			string animPath = _dataPath + name + ".anim";

			AnimationClip clip = AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip)) as AnimationClip;

			if (!clip)
			{
				clip = new AnimationClip();

				AssetDatabase.CreateAsset(clip, animPath);
			}
			else
			{
				EditorUtility.CopySerialized(new AnimationClip(), clip);
			}

			clip.name = name;

			return clip;
		}


		
		private void UpdateClipSettings(AnimationClip clip)
		{
			clip.wrapMode = WrapMode.Loop;
			
			AnimationUtility.SetAnimationType(clip, ModelImporterAnimationType.Generic);
			
			AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
			settings.loopTime = true;
			settings.stopTime = clip.length;

			SpineUtilities.SetAnimationSettings(clip, settings);

			// Add animation clip to layer 0 of the animator controller
			UnityEditorInternal.AnimatorController controller = transform.GetComponent<Animator>().runtimeAnimatorController as UnityEditorInternal.AnimatorController;
			
			if (controller)
			{
				UnityEditorInternal.StateMachine stateMachine = controller.GetLayer(0).stateMachine;
				
				bool hasClip = false;
				
				for (int i = 0; i < stateMachine.stateCount; i++)
				{
					if (stateMachine.GetState(i).GetMotion() == clip)
					{
						hasClip = true;
					}
				}
				
				if (!hasClip)
				{
					UnityEditorInternal.AnimatorController.AddAnimationClipToController(controller, clip);
				}
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
		
		// Helper functions taken from Spine-C# SkeletonJson.cs
		
		private static float[] GetFloatArray(Dictionary<String, Object> map, String name, float scale)
		{
			var list = (List<Object>)map[name];
			var values = new float[list.Count];
			if (scale == 1)
			{
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i];
			}
			else
			{
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i] * scale;
			}
			return values;
		}
		
		private static int[] GetIntArray(Dictionary<String, Object> map, String name)
		{
			var list = (List<Object>)map[name];
			var values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int)(float)list[i];
			return values;
		}
		
		private static float GetFloat(Dictionary<String, Object> map, String name, float defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float)map[name];
		}
		
		private static int GetInt(Dictionary<String, Object> map, String name, int defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int)(float)map[name];
		}
		
		private static bool GetBoolean(Dictionary<String, Object> map, String name, bool defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool)map[name];
		}
		
		private static String GetString(Dictionary<String, Object> map, String name, String defaultValue)
		{
			if (!map.ContainsKey(name))
				return defaultValue;
			return (String)map[name];
		}

		public static Color ToColor(string hexString)
		{
			return new Color(ToColor(hexString, 0), ToColor(hexString, 1), ToColor(hexString, 2), ToColor(hexString, 3));
		}
		
		public static float ToColor(String hexString, int colorIndex)
		{
			if (hexString.Length != 8)
				throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString);
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}

	}

}
