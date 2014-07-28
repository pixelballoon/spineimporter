using UnityEngine;
using System;

namespace SpineImporter
{

	[Serializable]
	public class SpineAttachment
	{

		[SerializeField, HideInInspector] private AttachmentType _type;
		[SerializeField] private string _name;
		[SerializeField] private Sprite _sprite;
		[SerializeField] private SpineMesh _mesh;
		[SerializeField] private Vector2 _positionOffset;
		[SerializeField] private float _rotationOffset;

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

	}

}
