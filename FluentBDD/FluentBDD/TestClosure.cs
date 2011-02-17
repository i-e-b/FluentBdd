using System;
using System.ComponentModel;

namespace FluentBDD {
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class TestClosure {
		public TestClosure (string given, string when, string then, Action testMethod) {
			Given = given;
			When = when;
			Then = then;
			With = "";
			TestMethod = testMethod;
			ExpectedExceptionType = null;
			ExpectedExceptionMessage = null;
		}

		public TestClosure (string given, string when, string then, string with, Action testMethod) {
			Given = given;
			When = when;
			Then = then;
			With = with;
			TestMethod = testMethod;
			ExpectedExceptionType = null;
			ExpectedExceptionMessage = null;
		}


		public TestClosure (string given, string when, string then, Action testMethod, Type exception, string exceptionMessage) {
			Given = given;
			When = when;
			Then = then;
			With = null;
			TestMethod = testMethod;
			ExpectedExceptionType = exception;
			ExpectedExceptionMessage = exceptionMessage;
		}

		public TestClosure (string given, string when, string then, string with, Action testMethod, Type exception, string exceptionMessage) {
			Given = given;
			When = when;
			Then = then;
			With = with;
			TestMethod = testMethod;
			ExpectedExceptionType = exception;
			ExpectedExceptionMessage = exceptionMessage;
		}

		public string Given;
		public string When;
		public string Then;
		public string With;
		public Action TestMethod;
		public Type ExpectedExceptionType;
		public string ExpectedExceptionMessage;
	}
}