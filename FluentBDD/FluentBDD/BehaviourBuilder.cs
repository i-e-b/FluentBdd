using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FluentBDD {
	public class BehaviourBuilder<TSubject> {
		protected readonly List<Func<Context<TSubject>>> ContextSources;

		public BehaviourBuilder(Func<Context<TSubject>> firstContext) {
			ContextSources = new List<Func<Context<TSubject>>> {firstContext};
		}

		public BehaviourBuilder<TSubject> And (Func<Context<TSubject>> contextProvider) {
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

		public BehaviourWithoutAnAction<TSubject, TProofType, TProofSource> Using<TProofType, TProofSource> () where TProofSource : class, TProofType, IProvide<TProofType>, new() where TProofType : class {
			return new BehaviourWithoutAnAction<TSubject, TProofType, TProofSource>(ContextSources);
		}

		public BehaviourWithoutAnAction<TSubject, TProofSource, TProofSource> Using<TProofSource> ()
			where TProofSource : class, IProvide<TProofSource>, new() {
			return new BehaviourWithoutAnAction<TSubject, TProofSource, TProofSource>(ContextSources);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class no_result {}
}