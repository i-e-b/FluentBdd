namespace System.Collections.Generic {
	[Serializable]
	public class TreeDictionary<TKey, TValue> 
	{
		protected readonly List<TValue> items;
		protected TreeDictionary<TKey, TValue> parent;
		protected readonly Dictionary<TKey, TreeDictionary<TKey, TValue>> children;

		#region Properties
		/// <summary>
		/// Data stored in this node
		/// </summary>
		public IEnumerable<TValue> Items {
			get { return items; }
		}

		/// <summary>
		/// Parent node
		/// </summary>
		public TreeDictionary<TKey, TValue> Parent {
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// List of child nodes
		/// </summary>
		public Dictionary<TKey, TreeDictionary<TKey, TValue>> Children {
			get { return children; }
		}

		/// <summary>
		/// Child by key
		/// </summary>
		public TreeDictionary<TKey, TValue> this[TKey key] {
			get {
				if (!children.ContainsKey(key)) children.Add(key, new TreeDictionary<TKey, TValue>());
				return children[key];
			}
		}

		#endregion

		public TreeDictionary () {
			children = new Dictionary<TKey, TreeDictionary<TKey, TValue>>();
			items = new List<TValue>();
		}

		/// <summary>
		/// Add a new child to this node.
		/// </summary>
		public void AddChild (TKey key, TreeDictionary<TKey, TValue> child) {
			children.Add(key, child);
			child.Parent = this;
		}

		/// <summary>
		/// Add a node item to the node items list.
		/// </summary>
		public void AddItem(TValue item) {
			items.Add(item);
		}

		public bool HasItems {
			get { return items.Count > 0; }
		}
	}
}
