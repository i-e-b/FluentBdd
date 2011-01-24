using NUnit.Framework;

namespace BddAddinAttributes {

	/// <summary>
	/// Just a few example language-like assertions.
	/// There are tons of libraries that have many more available.
	/// </summary>
	public static class Assertions {
		public static void should_be_equal_to(this object actual, object expected) {
			Assert.AreEqual(expected, actual);
		}


		public static void should_not_be_null (this object actual) {
			Assert.IsNotNull(actual);
		}
	}
}
