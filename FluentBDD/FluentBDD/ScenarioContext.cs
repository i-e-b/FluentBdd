using System;
using System.Collections.Generic;

namespace FluentBDD {
	public class ScenarioContext<TSubject> {
		private readonly Func<Context<TSubject>> GetNewContext;
		private Context<TSubject> context;
		private TSubject subject;
		private readonly List<Action<TSubject>> conditions = new List<Action<TSubject>>();

		public ScenarioContext (Func<Context<TSubject>> setup) {
			GetNewContext = setup;
		}

		public void AddCondition (Action<TSubject> condition) {
			conditions.Add(condition);
		}

		public TSubject CreateSubject (out Context<TSubject> localCtxSource) {
			context = GetNewContext();
			localCtxSource = context;

			subject = context.TheSubject;

			foreach (var condition in context.GetScenario().context.conditions) {
				condition(subject);
			}

			foreach (var condition in conditions) {
				condition(subject);
			}

			return subject;
		}
	}
}