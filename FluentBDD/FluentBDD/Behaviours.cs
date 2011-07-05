using System;
using System.ComponentModel;

namespace FluentBDD {

	public abstract class Behaviours {

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

		public static Provable<TExampleType, TExampleSource> ProvedBy<TExampleType, TExampleSource> ()
			where TExampleSource : class, TExampleType, IProvide<TExampleType>, new()
			where TExampleType : class {
			return new Provable<TExampleType, TExampleSource>();
		}

		public static Provable<TExampleSource, TExampleSource> ProvedBy<TExampleSource> ()
			where TExampleSource : class, IProvide<TExampleSource>, new()  {
			return new Provable<TExampleSource, TExampleSource>();
		}

		public static BehaviourBuilder<no_subject> GivenNoSubject () {
			return Given<no_subject, no_subject>();
		}

		public static BehaviourBuilder<no_subject> GivenStaticContextFor<T> () {
			return Given<no_subject, static_context<T>>();
		}

		public static BehaviourBuilder<TSubject> Given<TSubject, TContext> () where TContext : Context<TSubject>, new() {
			return new BehaviourBuilder<TSubject>(() => new TContext());

		}

		public static BehaviourBuilder<TSubject> Given<TSubject> (Func<Context<TSubject>> contextProvider) {
			return new BehaviourBuilder<TSubject>(contextProvider);
		}
	}
}