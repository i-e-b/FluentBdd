using System;
using System.Collections.Generic;

namespace FluentBDD {
	public class ContextBuilder<TSubject> {
		protected readonly List<Func<Context<TSubject>>> ContextSources;

		public ContextBuilder(Func<Context<TSubject>> firstContext) {
			ContextSources = new List<Func<Context<TSubject>>> {firstContext};
		}

		public ContextBuilder<TSubject> And (Func<Context<TSubject>> contextProvider) {
			ContextSources.Add(contextProvider);
			return this;
		}

		public Scenario<TSubject, no_result> When (string description, Action<TSubject> action) {
			return new Scenario<TSubject, no_result>(description, ContextSources, action);
		}


		public Scenario<TSubject, TResult> When<TResult> (string description, Func<TSubject, TResult> action) {
			return new Scenario<TSubject, TResult>(description, ContextSources, action);
		}

		public Scenario<TSubject, no_result> Verify () {
			return new Scenario<TSubject, no_result>("inspecting \"" + typeof(TSubject).Name + "\" type",
			ContextSources, s => { });
		}
	}

	public class no_result {}
}