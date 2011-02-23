using System;
using System.ComponentModel;

namespace FluentBDD {

	public abstract class Feature {

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class no_subject : Context<object> {
			public override void SetupContext () {
				Given("no subject", () => null);
			}

			public object Values { get; set; }
		}

		public static ContextBuilder<object> GivenNoSubject () {
			return With<object>(Context.Of<no_subject>);
		}

		public static ContextBuilder<TSubject> With<TSubject> (Func<Context<TSubject>> contextProvider) {
			return new ContextBuilder<TSubject>(contextProvider);
		}

		internal static T Create<T> () {
			return (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		internal static object CreateFor (Type featureType) {
			return featureType.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}
	}
}