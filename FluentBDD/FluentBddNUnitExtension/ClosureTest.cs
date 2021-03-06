﻿using System;
using FluentBDD;
using NUnit.Core;
using NUnit.Framework;

namespace FluentBddNUnitExtension {
	/// <summary>
	/// This class wraps a FluentBDD test closure as an NUnit test
	/// </summary>
	internal class ClosureTest : Test {
		internal readonly TestClosure testClosure;

		public ClosureTest (TestClosure testClosure) : base(testClosure.Then) {
			this.testClosure = testClosure;
			TestName.Name = testClosure.With ?? "";
			if (string.IsNullOrEmpty(TestName.Name)) TestName.Name = testClosure.Then + (testClosure.With ?? "");
		}

		public bool EmptyWith () {
			return string.IsNullOrEmpty(testClosure.With);
		}

		public override TestResult Run(EventListener listener, ITestFilter filter) {
			var result = new TestResult(this);

			lock (testClosure) {
				try {
					testClosure.TestMethod();
					TestNonExceptionCondition(result);
				} catch (IgnoreException iex) {
					result.Ignore(iex.Message);
				} catch (InconclusiveException icex) {
					result.Invalid(icex.Message);
				} catch (Exception ex) {
					TestExceptionCondition(result, ex);
				}
	
				try {
					testClosure.TearDown();
				} catch (Exception ex) {
					result.Failure("Exception in tear-down: "+ex.Message, ex.StackTrace);
				}
			}
			
			listener.TestFinished(result);
			return result;
		}

		public override string TestType {
			get { return "Test"; }
		}

		private void TestNonExceptionCondition(TestResult result) {
			if (! TestShouldThrowException()) result.Success();
			else result.Failure("The behaviour did not throw an exception", "");
		}

		private void TestExceptionCondition(TestResult result, Exception ex) {
			if (! TestShouldThrowException()) {
				result.Failure("Test or setup failed: "+ex.Message, ex.StackTrace);
			} else {
				if (! ThrownTypeMatchesExpectation(ex)) {
					result.Failure("Expected exception type of "
					               + testClosure.ExpectedExceptionType().Name
					               + " but got " + ex.GetType().Name, "\r\nat\r\n"+ex.StackTrace);
				} else {
					if (! TestExpectsAMessage()) result.Success();
					else {
						if (ThrownMessageMatchesExpectation(ex)) result.Success();
						else result.Failure("Expected exception message \r\n\""
						                    + testClosure.ExpectedExceptionMessage()
						                    + "\" but got \r\n\"" + ex.Message, "\"");
					}
				}
			}
		}

		private bool ThrownMessageMatchesExpectation(Exception ex) {
			return testClosure.ExpectedExceptionMessage() == ex.Message;
		}

		private bool TestExpectsAMessage() {
			return testClosure.ExpectedExceptionMessage != null
				&&
				testClosure.ExpectedExceptionMessage() != null;
		}

		private bool ThrownTypeMatchesExpectation(Exception ex) {
			return testClosure.ExpectedExceptionType() == ex.GetType();
		}

		private bool TestShouldThrowException() {
			return testClosure.ExpectedExceptionType != null
				&&
				testClosure.ExpectedExceptionType() != null;
		}

		public override object Fixture { get; set; }
	}
}