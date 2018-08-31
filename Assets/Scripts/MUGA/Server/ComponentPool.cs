using System;
using System.Collections.Generic;
using UnityEngine;

namespace MUGA.Server {
	/// <summary>
	/// An object pool for components
	/// </summary>
	/// <typeparam name="T">The type of component to pool</typeparam>
	internal class ComponentPool<T> where T: Component {
		private Queue<T> available = new Queue<T>();
		private Queue<T> used = new Queue<T>();
		public ComponentPool(int startSize = 10) {
			WarmupPool(startSize);
		}
		/// <summary>
		/// Takes an available component
		/// </summary>
		/// <returns>the component</returns>
		public T TakeComponent() {
			if(available.Count < 1) {
				AddToPool();
			}
			T component = available.Dequeue();
			component.gameObject.SetActive(true);
			used.Enqueue(component);
			return component;
		}
		/// <summary>
		/// Warmups the pool by pre-instantiating components.
		/// </summary>
		/// <param name="amount">The amount to pre-instantiate</param>
		public void WarmupPool(int amount) {
			for (int i = 0; i < amount; i++) {
				AddToPool();
			}
		}
		/// <summary>
		/// Clears the pool marking all objects as available
		/// </summary>
		public void ClearPool() {
			while(used.Count > 0) {
				T component = used.Dequeue();
				component.gameObject.SetActive(false);
				available.Enqueue(component);
			}
		}
		private void AddToPool() {
			GameObject obj = new GameObject("pooled_obj", typeof(T));
			obj.SetActive(false);
			available.Enqueue(obj.GetComponent<T>());
		}
	}
}
