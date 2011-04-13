using System;

namespace FluentBDD {
	public class TestClosure {
		/// <summary>
		/// Create an assertion test without expectation values
		/// </summary>
		public TestClosure (string given, string when, string then, Action testMethod, Action tearDown) {
			Given = given;
			When = when;
			Then = then;
			With = "";
			TestMethod = testMethod;
			ExpectedExceptionType = ()=>null;
			ExpectedExceptionMessage = ()=>null;
			TearDown = tearDown;
		}

		/// <summary>
		/// Create an assertion test using expectation values
		/// </summary>
		public TestClosure (string given, string when, string then, string with, Action testMethod, Action tearDown) {
			Given = given;
			When = when;
			Then = then;
			With = with;
			TestMethod = testMethod;
			ExpectedExceptionType = ()=>null;
			ExpectedExceptionMessage = ()=>null;
			TearDown = tearDown;
		}

		/// <summary>
		/// Create test for exceptions without expectation values
		/// </summary>
		public TestClosure (string given, string when, string then, Action testMethod, Func<Type> exception, Func<string> exceptionMessage, Action tearDown) {
			Given = given;
			When = when;
			Then = then;
			With = null;
			TestMethod = testMethod;
			ExpectedExceptionType = exception;
			ExpectedExceptionMessage = exceptionMessage;
			TearDown = tearDown;
		}

		/// <summary>
		/// Create test for exceptions using expectation values
		/// </summary>
		public TestClosure (string given, string when, string then, string with, Action testMethod, Func<Type> exception, Func<string> exceptionMessage, Action tearDown) {
			Given = given;
			When = when;
			Then = then;
			With = with;
			TestMethod = testMethod;
			ExpectedExceptionType = exception;
			ExpectedExceptionMessage = exceptionMessage;
			TearDown = tearDown;
		}

		public string Given;
		public string When;
		public string Then;
		public string With;
		public Action TestMethod;
		public Func<Type> ExpectedExceptionType;
		public Func<string> ExpectedExceptionMessage;
		public Action TearDown;
	}
}