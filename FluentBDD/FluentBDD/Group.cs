namespace FluentBDD {
	internal class Group<Ta, Tb> {
		public Ta A { get; set; }
		public Tb B { get; set; }

		/// <summary>
		/// First value
		/// </summary>
		public T _1<T> () where T : Ta {
			return (T)A;
		}

		/// <summary>
		/// Second value
		/// </summary>
		public T _2<T> () where T : Tb {
			return (T)B;
		}


		public Ta First () {
			return A;
		}
		public Tb Second () {
			return B;
		}

		public Group (Ta a, Tb b) {
			A = a;
			B = b;
		}
	}

	internal class Group<Ta, Tb, Tc> : Group<Ta, Tb> {
		public Tc C { get; set; }

		public Group (Ta a, Tb b, Tc c)
			: base(a, b) {
			C = c;
		}

		/// <summary>
		/// Third value
		/// </summary>
		public T _3<T> () where T : Tc {
			return (T)C;
		}
	}
}