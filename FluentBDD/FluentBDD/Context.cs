using System;

namespace FluentBDD {
	public abstract class Context {
		public static TContext Of<TContext> () where TContext : Context, new() {
			return new TContext();
		}
	}

	public abstract class Context<TSubject> : Context {
		private SubjectBuilder<TSubject> SubjectSource;

		public abstract void SetupContext ();

		public virtual void TearDown(){}

		internal SubjectBuilder<TSubject> SetupAndReturnContextBuilder () {
			try {
				SetupContext();
				return SubjectSource;
			} catch (NullReferenceException nrex) {
				throw new Exception("Context setup failed due to a null reference. Did you provide a value source?", nrex);
			} catch (Exception ex) {
				throw new Exception("Context setup failed: " + ex.Message, ex);
			}
		}

		public SubjectBuilder<TSubject> Given (string description, Func<TSubject> createSubject) {
			if (SubjectSource != null) throw new ArgumentException("Context already exists. If you've inherited from another context, continue with 'GivenBaseContext().And(...)'");
			SubjectSource = new SubjectBuilder<TSubject>(description, createSubject);
			return SubjectSource;
		}

		public SubjectBuilder<TSubject> GivenBaseContext () {
			if (SubjectSource == null) throw new InvalidOperationException("Context has not been created. Remember 'base.SetupContext()' or try starting with 'Given(...)'");
			return SubjectSource;
		}
	}
}