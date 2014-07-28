using UnityEngine;

namespace SpineImporter
{

	public class SpineSlot : MonoBehaviour
	{

		[SerializeField] private string _defaultAttachment;
		[SerializeField] private bool _additiveBlending;
		[SerializeField] private Color _color;
		[SerializeField] private float _drawOrder;

		public string DefaultAttachment
		{
			get { return _defaultAttachment; }
			set { _defaultAttachment = value; }
		}

		public bool AdditiveBlending
		{
			get { return _additiveBlending; }
			set { _additiveBlending = value; }
		}

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public float DrawOrder
		{
			get { return _drawOrder; }
			set { _drawOrder = value; }
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
		
		public void SetAttachment(SpineAttachment attachment)
		{
			Clear();

			transform.localPosition = new Vector3(0, 0, _drawOrder);

			switch (attachment.Type)
			{
				case SpineAttachment.AttachmentType.Region:
				{
					SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = attachment.Sprite;
					transform.localPosition += new Vector3(attachment.PositionOffset.x, attachment.PositionOffset.y, 0);
					transform.localEulerAngles = new Vector3(0, 0, attachment.RotationOffset);
					transform.localScale = new Vector3(100, 100, 1);
					break;
				}
				case SpineAttachment.AttachmentType.Mesh:
				{
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = attachment.Mesh.Mesh;
					Material material = new Material(Shader.Find("Unlit/Transparent"));
					material.mainTexture = attachment.Mesh.Texture;
					meshRenderer.material = material;
					break;
				}
				case SpineAttachment.AttachmentType.SkinnedMesh:
				{
					SkinnedMeshRenderer meshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
					Material material = new Material(Shader.Find("Unlit/Transparent"));
					material.mainTexture = attachment.Mesh.Texture;
					meshRenderer.material = material;
					meshRenderer.bones = attachment.Mesh.Bones;
					meshRenderer.sharedMesh = attachment.Mesh.Mesh;
					break;
				}
			}
		}

	}

}
