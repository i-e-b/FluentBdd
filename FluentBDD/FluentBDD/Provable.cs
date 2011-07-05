namespace FluentBDD {
	public class Provable<TProofType, TProofSource>
		where TProofSource : class, TProofType, IProvide<TProofType>, new()
		where TProofType : class {
		internal Provable() {}

		public BehaviourWithoutAnAction<TSubject, TProofType, TProofSource>
			Given<TSubject, TContext> () where TContext : Context<TSubject>, IUse<TProofType>, new() {
			return Behaviours.Given<TSubject,TContext>().Using<TProofType, TProofSource>();
		}
	}
}
