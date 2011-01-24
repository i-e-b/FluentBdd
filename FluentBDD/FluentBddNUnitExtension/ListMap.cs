using System.Collections.Generic;
using System.Linq;

namespace FluentBddNUnitExtension {
	public class ListMap<TKey, TValues> {
		private readonly Dictionary<TKey, List<TValues>> dict;

		public IList<TValues> this[TKey key] {
			get {
				if (!dict.ContainsKey(key)) {
					dict.Add(key, new List<TValues>());
				}
				return dict[key];
			}
		}

		public IEnumerable<TKey> Keys {
			get { return dict.Keys.ToList(); }
		}

		public ListMap() {
			dict = new Dictionary<TKey, List<TValues>>();
		}
	}
}
