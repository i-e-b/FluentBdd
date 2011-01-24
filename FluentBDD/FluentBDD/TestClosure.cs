using System;

namespace FluentBDD {
	internal class TestClosure {
		public string Cause;
		public string Effect;
		public Action TestMethod;
		public Type ExpectedExceptionType = null;
		public string ExpectedExceptionMessage = null;
	}
}