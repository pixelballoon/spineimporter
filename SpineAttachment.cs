using UnityEngine;
using System;

namespace SpineImporter
{

	[Serializable]
	public class SpineAttachment : MonoBehaviour
	{

		[SerializeField, HideInInspector] private AttachmentType _type;
		[SerializeField] private string _name;
		[SerializeField] private Sprite _sprite;
		[SerializeField] private SpineMesh _mesh;
		[SerializeField] private Vector2 _positionOffset;
		[SerializeField] private Vector2 _scaleOffset;
		[SerializeField] private float _rotationOffset;

		public SpineSlot Slot
		{
			get { return GetComponentInParent<SpineSlot>(); }
		}

		public AttachmentType Type
		{
			get { return _type; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public Sprite Sprite
		{
			get { return _sprite; }
			set { _sprite = value; }
		}

		public SpineMesh Mesh
		{
			get { return _mesh; }
			set { _mesh = value; }
		}

		public Vector2 PositionOffset
		{
			get { return _positionOffset; }
			set { _positionOffset = value; }
		}
		public Vector2 ScaleOffset
		{
			get { return _scaleOffset; }
			set { _scaleOffset = value; }
		}
		
		public float RotationOffset
		{
			get { return _rotationOffset; }
			set { _rotationOffset = value; }
		}

		public enum AttachmentType
		{
			Region,
			Mesh,
			SkinnedMesh
		}

		public SpineAttachment(string name, AttachmentType type)
		{
			_name = name;
			_type = type;
		}

		public void Clear()
		{
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = Vector3.zero;
			transform.localScale = Vector3.one;
			
			DestroyImmediate(GetComponent<SpriteRenderer>());
			DestroyImmediate(GetComponent<MeshRenderer>());
			DestroyImmediate(GetComponent<SkinnedMeshRenderer>());
			DestroyImmediate(GetComponent<MeshFilter>());
		}

		public void Refresh()
		{
			Clear();

			SpineSkeleton skeleton = GetComponentInParent<SpineSkeleton>();
			
			switch (Type)
			{
				case AttachmentType.Region:
				{
					SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = Sprite;
					if (skeleton.SpriteMaterial != null)
					{
						spriteRenderer.sharedMaterial = skeleton.SpriteMaterial;
					}
					transform.localPosition += new Vector3(PositionOffset.x, PositionOffset.y, 0);
					transform.localEulerAngles = new Vector3(0, 0, RotationOffset);
					transform.localScale = new Vector3(100 * ScaleOffset.x, 100 * ScaleOffset.y, 1);
					break;
				}
				case AttachmentType.Mesh:
				{
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = Mesh.Mesh;
					Material material = skeleton.MeshMaterial != null ? new Material(skeleton.MeshMaterial) : new Material(Shader.Find("Default/Diffuse"));
					material.mainTexture = Mesh.Texture;
					meshRenderer.sharedMaterial = material;
					break;
				}
				case AttachmentType.SkinnedMesh:
				{
					SkinnedMeshRenderer meshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
					Material material = skeleton.MeshMaterial != null ? new Material(skeleton.MeshMaterial) : new Material(Shader.Find("Default/Diffuse"));
					material.mainTexture = Mesh.Texture;
					meshRenderer.sharedMaterial = material;
					meshRenderer.bones = Mesh.Bones;
					meshRenderer.sharedMesh = Mesh.Mesh;
					break;
				}
			}
		}
	}
}
