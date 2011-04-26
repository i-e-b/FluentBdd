namespace FluentBDD {
	public class Provable<TExampleType, TExampleSource>
		where TExampleSource : class, TExampleType, IProvide<TExampleType>, new()
		where TExampleType : class {
		internal Provable() {}

		public ScenarioWithoutAnAction<TSubject, TExampleType, TExampleSource>
			Given<TSubject, TContext> () where TContext : Context<TSubject>, IUse<TExampleType>, new() {
			return Behaviours.Given<TSubject,TContext>().Using<TExampleType, TExampleSource>();
		}
	}
}
