using System.Collections.Generic;
using System.Linq;

namespace FluentBDD.TreeBuilding {
	public class Map<TKey, TValue> where TValue:new() {	
		private readonly Dictionary<TKey, TValue> dict;

		public Map() {
			dict = new Dictionary<TKey, TValue>();
		}

		public TValue this[TKey key] {
			get {
				if (!dict.ContainsKey(key)) {
					dict.Add(key, new TValue());
				}
				return dict[key];
			}
			set {
				if (!dict.ContainsKey(key)) {
					dict.Add(key, value);
				} else {
					dict[key] = value;
				}
			}
		}

		public IEnumerable<TKey> Keys {
			get { return dict.Keys.ToList(); }
		}
	}
}
