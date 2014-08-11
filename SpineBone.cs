using UnityEngine;

namespace SpineImporter
{

	public class SpineBone : MonoBehaviour
	{

		[SerializeField] private int _index;
		[SerializeField] private float _length;

		private SpineSkeleton _skeleton;

		public SpineSkeleton Skeleton
		{
			get
			{
				if (_skeleton == null)
				{
					_skeleton = GetComponentInParent<SpineSkeleton>();
				}
				return _skeleton;
			}
		}

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

		void OnDrawGizmosSelected()
		{
			SpineSkeleton skeleton = Skeleton;

			if (!skeleton || !skeleton.DrawBones)
			{
				return;
			}

			Vector3 a = transform.position;
			Vector3 b = transform.TransformPoint(new Vector3(_length, 0, 0));
			Vector3 c = transform.TransformPoint(new Vector3(0.05f * _length, 0.05f * _length, 0));
			Vector3 d = transform.TransformPoint(new Vector3(0.05f * _length, - 0.05f * _length, 0));

			Gizmos.DrawLine(a, c);
			Gizmos.DrawLine(a, d);
			Gizmos.DrawLine(b, c);
			Gizmos.DrawLine(b, d);
		}

	}

}
