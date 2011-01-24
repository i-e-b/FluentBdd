using System;
using System.Collections.Generic;

namespace FluentBDD {

	/*
	 *   This file holds the fluent interface to the "Scenario" test DSL 
	 * 
	 *   Like most big fluent interfaces, it's nasty code.
	 *   You are probably best to explore it with intellisense before
	 *   trying to read the code below.
	 * 
	 */

	public abstract class Scenario {
		internal string ContextName;
		internal string ActionName;
		internal Action SetupAction;
		internal List<TestClosure> TestClosures = new List<TestClosure>();
	}

	public class Scenario<TSubject, TContext> : Scenario where TContext : Context<TSubject> {
		internal readonly ScenarioContext<TSubject> context;

		public Scenario (Func<Context<TSubject>> contextSource, string description) {
			ContextName = description;
			context = new ScenarioContext<TSubject>(contextSource);
		}

		internal Scenario (TSubject initialSubject, string description) {
			ContextName = description;
			context = new ScenarioContext<TSubject>(
				() => new WrapperContext<TSubject>(description, () => initialSubject)
			);
		}

		public Scenario<TSubject, TContext> And (string description, Action<TSubject> mutation) {
			ContextName += " and " + description;
			context.AddCondition(mutation);
			return this;
		}

		public ScenarioWithContextAndActionWithResult<TSubject, TResult, TContext> When<TResult> (string description, Func<TSubject, TResult> resultFunction) {
			ActionName = "When " + description;
			return new ScenarioWithContextAndActionWithResult<TSubject, TResult, TContext>(
				context,
				resultFunction,
				ContextName,
				ActionName);
		}


		public ScenarioWithContextAndVoidAction<TSubject, TContext> When (string description, Action<TSubject> resultFunction) {
			ActionName = "When " + description;
			return new ScenarioWithContextAndVoidAction<TSubject, TContext>(
				context,
				resultFunction,
				ContextName,
				ActionName);
		}
	}

	public class ScenarioWithContextAndVoidAction<TSubject, TContext> : Scenario where TContext:Context<TSubject> {
		private readonly ScenarioContext<TSubject> context;
		private readonly Action<TSubject> scenario_action;

		public ScenarioWithContextAndVoidAction (ScenarioContext<TSubject> context, Action<TSubject> scenarioAction, string contextName, string actionName) {
			ContextName = contextName;
			ActionName = actionName;
			this.context = context;
			scenario_action = scenarioAction;
		}

		public ScenarioForExceptions<TSubject, TException> ShouldThrow<TException> () {
			return new ScenarioForExceptions<TSubject, TException>(context, scenario_action, ContextName, ActionName); 
		}

		public ScenarioWithContextAndVoidAction<TSubject, TContext> Then (string description, Action<TSubject> action) {
			TestClosures.Add(
				new TestClosure
				{
					Cause = ActionName,
					Effect = description,
					TestMethod = () =>
					{
						Context<TSubject> localCtxSource;
						var localContext = context.CreateSubject(out localCtxSource);
						scenario_action(localContext);
						action(localContext);
					}
				});
			return this;
		}

		public ScenarioWithContextAndVoidAction<TSubject, TContext> Then<T> (string description, Action<T> contextAction)
			where T : TContext {
			TestClosures.Add(
				new TestClosure {
				    Cause = ActionName,
					Effect = description,
					TestMethod = () => {
					    Context<TSubject> localCtxSource;
						var localContext = context.CreateSubject(out localCtxSource);
						scenario_action(localContext);
						contextAction((T)localCtxSource);
					}
				});
			return this;
		}

	}

	public class ScenarioForExceptions<TSubject, TException> : Scenario  {
		private readonly ScenarioContext<TSubject> context;
		private readonly Action<TSubject> scenarioAction;

		public ScenarioForExceptions (ScenarioContext<TSubject> context, Action<TSubject> scenarioAction, string contextName, string actionName) {
			this.context = context;
			this.scenarioAction = scenarioAction;
			ContextName = contextName;
			ActionName = actionName;

			TestClosures.Add(
			     new TestClosure {
					Cause = ActionName,
			        Effect = "should get exception of type " + typeof (TException).Name,
			        ExpectedExceptionType = typeof(TException),
					TestMethod = () =>
					{
						Context<TSubject> localCtxSource;
						var localContext = context.CreateSubject(out localCtxSource);
						scenarioAction(localContext);
					}
				});
		}

		public Scenario WithMessage(string expectedMessage) {
			TestClosures.Add(
				 new TestClosure
				 {
					 Cause = ActionName,
					 Effect = "should get exception with message \"" + expectedMessage + "\"",
					 ExpectedExceptionType = typeof(TException),
					 ExpectedExceptionMessage = expectedMessage,
					 TestMethod = () =>
					 {
						 Context<TSubject> localCtxSource;
						 var localContext = context.CreateSubject(out localCtxSource);
						 scenarioAction(localContext);
					 }
				 });
			return this;
		}
	}

	public class ScenarioWithContextAndActionWithResult<TSubject, TResult, TContext> : Scenario where TContext:Context<TSubject> {
		private readonly ScenarioContext<TSubject> context;
		private readonly Func<TSubject, TResult> scenario_action;

		public ScenarioWithContextAndActionWithResult (ScenarioContext<TSubject> context, Func<TSubject, TResult> scenarioAction, string contextName, string actionName) {
			ContextName = contextName;
			ActionName = actionName;
			this.context = context;
			scenario_action = scenarioAction;
		}

		public ScenarioWithContextAndActionWithResult<TSubject, TResult, TContext> Then (string description, Action<TSubject, TResult> action) {
			TestClosures.Add(
				new TestClosure
				{
					Cause = ActionName,
					Effect = description,
					TestMethod = () =>
					{
						Context<TSubject> localCtxSource;
						var localContext = context.CreateSubject(out localCtxSource);
						action(localContext, scenario_action(localContext));
					}
				});
			return this;
		}
	}
}
