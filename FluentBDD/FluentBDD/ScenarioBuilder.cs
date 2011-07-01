using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FluentBDD {
	public class ScenarioBuilder<TSubject> {
		protected readonly List<Func<Context<TSubject>>> ContextSources;

		public ScenarioBuilder(Func<Context<TSubject>> firstContext) {
			ContextSources = new List<Func<Context<TSubject>>> {firstContext};
		}

		public ScenarioBuilder<TSubject> And (Func<Context<TSubject>> contextProvider) {
			ContextSources.Add(contextProvider);
			return this;
		}

		public Behaviour<TSubject, no_result> When (string description, Action<TSubject> action) {
			return new Behaviour<TSubject, no_result>(description, ContextSources, action);
		}

		public Behaviour<TSubject, no_result> When (string description, Action<TSubject, Context<TSubject>> action) {
			return new Behaviour<TSubject, no_result>(description, ContextSources, action);
		}

		public Behaviour<TSubject, TResult> When<TResult> (string description, Func<TSubject, TResult> action) {
			return new Behaviour<TSubject, TResult>(description, ContextSources, action);
		}

		public Behaviour<TSubject, no_result> Verify () {
			return new Behaviour<TSubject, no_result>("inspecting \"" + typeof(TSubject).Name + "\" type",
			ContextSources, s => { });
		}

		public BehaviourWithoutAnAction<TSubject, TExampleType, TExampleSource> Using<TExampleType, TExampleSource> () where TExampleSource : class, TExampleType, IProvide<TExampleType>, new() where TExampleType : class {
			return new BehaviourWithoutAnAction<TSubject, TExampleType, TExampleSource>(ContextSources);
		}

		public BehaviourWithoutAnAction<TSubject, TExampleSource, TExampleSource> Using<TExampleSource> ()
			where TExampleSource : class, IProvide<TExampleSource>, new() {
			return new BehaviourWithoutAnAction<TSubject, TExampleSource, TExampleSource>(ContextSources);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class no_result {}
}