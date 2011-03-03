using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBDD {
	public abstract partial class Feature{

		public class result<TReturnType>: IShould<TReturnType> {
			private bool Invert = false;

			public static IShould<TReturnType> should {
				get { return new result<TReturnType>(); }
			}

			public IShould<TReturnType> not {
				get {
					this.Invert = true;
					return this;
				}
			}

			// subject, result
			public Action<TReturnType, T> equal<T> (T value) {
				if (Invert) return (subject, result) => result.should_not_be_equal_to(value);
				return (subject, result) => result.should_be_equal_to(value);
			}
		}
		
		public interface IShould<Tr> {
			IShould<Tr> not { get; }
			Action<Tr, T> equal<T> (T value);
		}
	}

}
