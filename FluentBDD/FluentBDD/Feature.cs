using System;

namespace FluentBDD {
	public class no_subject : Context<object> {
		public no_subject () {
			Given("no subject", () => null);
		}
	}

	public abstract class Feature {

		public static Scenario<object, Context<object>> GivenNoSubject() {
			return With<object>(Context.Of<no_subject>);
		}

		internal static T Create<T> () {
			return (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		internal static object CreateFor (Type featureType) {
			return featureType.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		public static Scenario<T, Context<T>> With<T> (Func<Context<T>> newContext) {
			var descr = newContext().GetScenario().ContextName;
			return new Scenario<T, Context<T>>(newContext, descr);
		}
	}

}