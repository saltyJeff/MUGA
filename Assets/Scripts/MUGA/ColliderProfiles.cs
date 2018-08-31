using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MUGA {
	//OOP isn't particularly known for being terse
	/// <summary>
	/// Represents being able to restore a represented state to a collider
	/// </summary>
	/// <typeparam name="T">A collider</typeparam>
	public interface IRestoreableToCollider<T> where T: Collider {
		void RestoreSelfToCollider(T c);
	}

	//every collilder must inherit from transform profile
	/// <summary>
	/// Stores the state of a Transform including its name, tag, and physics layer
	/// </summary>
	/// <seealso cref="MUGA.IRestoreableToCollider{UnityEngine.Collider}" />
	[MessagePackObject(keyAsPropertyName: true)]
	public class TransformProfile: IRestoreableToCollider<Collider> {
		public string name;
		public string tag;
		public int layer;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
		public Vector3 localScale;

		public TransformProfile() { }
		public TransformProfile (Collider c): this(c.gameObject) { }

		public TransformProfile(GameObject g) {
			name = g.name;
			tag = g.tag;
			layer = g.layer;
			position = g.transform.position;
			rotation = g.transform.rotation;
			scale = g.transform.lossyScale;
			localScale = g.transform.localScale;
		}
		/// <summary>
		/// Creates a transform profile from a collider
		/// </summary>
		/// <param name="c">The collider to save</param>
		/// <returns>A transform profile</returns>
		public static TransformProfile ProfileFromCollider(Collider c) {
			CapsuleCollider maybeCapsule = c as CapsuleCollider;
			if(maybeCapsule != null) {
				return new CapsuleColliderProfile(maybeCapsule);
			}
			BoxCollider maybeBox = c as BoxCollider;
			if(maybeBox != null) {
				return new BoxColliderProfile(maybeBox);
			}
			SphereCollider maybeSphere = c as SphereCollider;
			if (maybeSphere != null) {
				return new SphereColliderProfile(maybeSphere);
			}
			//uh oh
			throw new InvalidOperationException(c.GetType().ToString() + " is not a supported collider type, please use only Capsules and Boxes");
		}
		public void RestoreSelfToCollider(Collider c) {
			RestoreSelfToGameObject(c.gameObject);
			//only because the objects from the pools will have no parent, so local and global scales are equal
			c.transform.localScale = scale;
		}
		/// <summary>
		/// Restores the state represented by itself to a GameObject
		/// </summary>
		/// <param name="o">The GameObject to restore itself to</param>
		public void RestoreSelfToGameObject(GameObject o) {
			o.name = name;
			o.tag = tag;
			o.layer = layer;
			o.transform.position = position;
			o.transform.rotation = rotation;
			o.transform.localScale = localScale;
		}
	}

	/// <summary>
	/// Represents the state of a box collider
	/// </summary>
	/// <seealso cref="MUGA.TransformProfile" />
	/// <seealso cref="MUGA.IRestoreableToCollider{UnityEngine.BoxCollider}" />
	[MessagePackObject(keyAsPropertyName: true)]
	public class BoxColliderProfile : TransformProfile, IRestoreableToCollider<BoxCollider> {
		public Vector3 center;
		public Vector3 size;

		public BoxColliderProfile(): base() {}
		public BoxColliderProfile (BoxCollider c): base(c) {
			center = c.center;
			size = c.size;
		}
		public void RestoreSelfToCollider(BoxCollider c) {
			base.RestoreSelfToCollider(c);
			c.center = center;
			c.size = size;
		}
	}

	/// <summary>
	/// Represents the state of a capsule collider
	/// </summary>
	/// <seealso cref="MUGA.TransformProfile" />
	/// <seealso cref="MUGA.IRestoreableToCollider{UnityEngine.CapsuleCollider}" />
	[MessagePackObject(keyAsPropertyName: true)]
	public class CapsuleColliderProfile: TransformProfile, IRestoreableToCollider<CapsuleCollider> {
		public Vector3 center;
		public float radius;
		public float height;
		public int axis; //0=X, 1=Y, 2=Z
		public CapsuleColliderProfile() : base() { }
		public CapsuleColliderProfile (CapsuleCollider c): base(c) {
			center = c.center;
			radius = c.radius;
			height = c.height;
			axis = c.direction;
		}
		public void RestoreSelfToCollider (CapsuleCollider c) {
			base.RestoreSelfToCollider(c);
			c.center = center;
			c.radius = radius;
			c.height = height;
			c.direction = axis;
		}
	}

	/// <summary>
	/// Represents the state of a sphere collider
	/// </summary>
	/// <seealso cref="MUGA.TransformProfile" />
	/// <seealso cref="MUGA.IRestoreableToCollider{UnityEngine.CapsuleCollider}" />
	[MessagePackObject(keyAsPropertyName: true)]
	public class SphereColliderProfile : TransformProfile, IRestoreableToCollider<CapsuleCollider> {
		public Vector3 center;
		public float radius;
		public SphereColliderProfile() : base() { }
		public SphereColliderProfile(SphereCollider c) : base(c) {
			center = c.center;
			radius = c.radius;
		}
		public void RestoreSelfToCollider(CapsuleCollider c) {
			base.RestoreSelfToCollider(c);
			c.center = center;
			c.radius = radius;
		}
	}
}
