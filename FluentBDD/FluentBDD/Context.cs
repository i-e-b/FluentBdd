using System;

namespace FluentBDD {
	public abstract class Context {
		public static T Of<T>() where T: Context, new() {
			return new T();
		}
	}

	public abstract class Context<TSubject> : Context {
		internal Scenario<TSubject, Context<TSubject>> TheScenarioContext;
		internal Func<TSubject> CreateSubjectFunc;
		internal string Description;
		internal TSubject TheSubject;

		public virtual void Setup() {}

		public Scenario<TSubject, Context<TSubject>> Given (string description, Func<TSubject> createSubject) {
			if (TheScenarioContext != null) throw new InvalidOperationException("Context already exists. If you've inherited from another context, continue with 'And(...)'");
			
			Description = "Given " + description;
			Setup();
			TheSubject = createSubject();
			TheScenarioContext = new Scenario<TSubject, Context<TSubject>>(TheSubject, "Given " + description);
			
			return TheScenarioContext;
		}

		public Scenario<TSubject, Context<TSubject>> GivenBaseContext () {
			if (TheScenarioContext == null) throw new InvalidOperationException("Context has not been created. Try starting with 'Given(...)'");
			return TheScenarioContext;
		}

		internal Scenario<TSubject, Context<TSubject>> GetScenario () {
			return TheScenarioContext;
		}
	}

	internal class WrapperContext<TSubject> : Context<TSubject> {

		internal WrapperContext (string description, Func<TSubject> createSubject) {
			Given(description, createSubject);
		}
	}

}