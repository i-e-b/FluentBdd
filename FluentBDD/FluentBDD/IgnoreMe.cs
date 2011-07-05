using NUnit.Framework;

namespace FluentBDD {
	/// <summary>
	/// Ignore helper for "When" cases
	/// </summary>
	public static class Ignore {
		public static void me (object a, object b) {
			Assert.Ignore("Ignored");
		}

		public static void me (object a) {
			Assert.Ignore("Ignored");
		}
	}
}