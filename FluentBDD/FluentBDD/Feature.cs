﻿using System;
using System.ComponentModel;

namespace FluentBDD {

	public abstract class Feature {

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class no_subject : Context<no_subject> {
			public override void SetupContext () {
				Given("no subject", () => new no_subject());
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class static_context<T> : Context<no_subject>, IUse<T> {
			public T Values { get; set; }
			public override void SetupContext () {
				Given("no subject", () => new no_subject());
			}
		}

		public static ContextBuilder<no_subject> GivenNoSubject () {
			return With<no_subject>(Context.Of<no_subject>);
		}

		public static ContextBuilder<no_subject> GivenStaticContextFor<T> () {
			return With<no_subject>(Context.Of<static_context<T>>);
		}

		public static ContextBuilder<TSubject> With<TSubject> (Func<Context<TSubject>> contextProvider) {
			return new ContextBuilder<TSubject>(contextProvider);
		}

		internal static object CreateFor (Type featureType) {
			return featureType.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}
	}
}