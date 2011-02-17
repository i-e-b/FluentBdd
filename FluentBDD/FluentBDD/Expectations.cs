namespace FluentBDD {

	public interface IUse<T> {
		T Values { get; set; }
	}

	public interface IProvide<T> {
		T[] Data();
		string StringRepresentation();
	}


}
