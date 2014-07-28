using UnityEngine;

namespace SpineImporter
{

	public class SpineBone : MonoBehaviour
	{

		[SerializeField] private int _index;
		[SerializeField] private float _length;

		public int Index
		{
			get { return _index; }
			set { _index = value; }
		}

		public float Length
		{
			get { return _length; }
			set { _length = value; }
		}

		void OnDrawGizmos()
		{
			Gizmos.DrawLine(transform.position, transform.TransformPoint(new Vector3(_length, 0, 0)));
		}

	}

}
