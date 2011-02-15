using System;
using FluentBDD;
using NUnit.Core;

namespace FluentBddNUnitExtension {
	internal class ClosureTest : Test {
		internal readonly TestClosure testClosure;

		public ClosureTest (TestClosure testClosure) : base(testClosure.Then) {
			this.testClosure = testClosure;
			TestName.Name = testClosure.With ?? "???";
		}

		public void UseCompressedName() {
			TestName.Name = testClosure.Then + (testClosure.With ?? "");
		}

		public bool EmptyWith () {
			return string.IsNullOrEmpty(testClosure.With);
		}

		public override TestResult Run(EventListener listener, ITestFilter filter) {
			var result = new TestResult(TestName);
			listener.TestStarted(TestName);

			try {
				testClosure.TestMethod();
				TestNonExceptionCondition(result);
			} catch (Exception ex) {
				TestExceptionCondition(result, ex);
			}

			listener.TestFinished(result);
			return result;
		}

		private void TestNonExceptionCondition(TestResult result) {
			if (! TestShouldThrowException()) result.Success();
			else result.Failure("The scenario did not throw an exception", "");
		}

		private void TestExceptionCondition(TestResult result, Exception ex) {
			if (! TestShouldThrowException()) {
				result.Failure(ex.Message, ex.StackTrace);
			} else {
				if (! ThrownTypeMatchesExpectation(ex)) {
					result.Failure("Expected exception type of "
					               + testClosure.ExpectedExceptionType.Name
					               + " but got " + ex.GetType().Name, "");
				} else {
					if (! TestExpectsAMessage()) result.Success();
					else {
						if (ThrownMessageMatchesExpectation(ex)) result.Success();
						else result.Failure("Expected exception message \r\n\""
						                    + testClosure.ExpectedExceptionMessage
						                    + "\" but got \r\n\"" + ex.Message, "\"");
					}
				}
			}
		}

		private bool ThrownMessageMatchesExpectation(Exception ex) {
			return testClosure.ExpectedExceptionMessage == ex.Message;
		}

		private bool TestExpectsAMessage() {
			return testClosure.ExpectedExceptionMessage != null;
		}

		private bool ThrownTypeMatchesExpectation(Exception ex) {
			return testClosure.ExpectedExceptionType == ex.GetType();
		}

		private bool TestShouldThrowException() {
			return testClosure.ExpectedExceptionType != null;
		}

		public override int TestCount { get { return 1; } }
		public override object Fixture { get; set; }
	}
}