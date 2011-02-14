namespace FluentBDD {

	public interface IUse<T> where T : IProvide<T>{
		T Values { get; set; }
	}

	public interface IProvide<T> {
		T[] Data();
		string StringRepresentation();
	}

}
