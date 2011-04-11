﻿using System;
using FluentBDD;

namespace Advanced.UsageExample {
	[Behaviour("Exceptions in expectations")]
	class ExceptionsInExpectations : Behaviours {
		public Scenario a_load_of_different_exceptions =
			Given(() => Context.Of<an_exception_throwing_class>())
				.When("I cause an exception", s => s.ThrowException())
				.Using<exception_expectations>()
				.ShouldThrow(v => v.ExpectedException);
	}

	internal class an_exception_throwing_class : Context<ExceptionThrowingClass>, IUse<exception_expectations> {
		public exception_expectations Values { get; set; }

		public override void SetupContext() {
			Given("an exception throwing class", () => {
				Console.WriteLine ("Creating "+Values.ExceptionNumber+", "+Values.ExpectedException.GetType()+", "+Values.ExpectedException.Message);
				return new ExceptionThrowingClass(Values.ExceptionNumber);});
		}
	}

	internal class exception_expectations:IProvide<exception_expectations> {
		public int ExceptionNumber;
		public Exception ExpectedException;

		public exception_expectations[] Data() {
			return new[] {
			new exception_expectations {ExceptionNumber = 1, ExpectedException = new Exception("")},
			new exception_expectations {ExceptionNumber = 2, ExpectedException = new ArgumentException("")},
			new exception_expectations {ExceptionNumber = 4, ExpectedException = new ArgumentException("Other message")},
			new exception_expectations {ExceptionNumber = 3, ExpectedException = new ArgumentException("Booyah!")},
			new exception_expectations {ExceptionNumber = 0, ExpectedException = new Exception("Hello")}
		};
		}

		public override string ToString () {
			return StringRepresentation();
		}
		public string StringRepresentation() {
			return "[" + ExceptionNumber + "] = " + ExpectedException.GetType() + ", \"" + ExpectedException.Message + "\"";
		}
	}

	internal class ExceptionThrowingClass {
		private readonly int exceptionNumber;

		public ExceptionThrowingClass(int exceptionNumber) {
			this.exceptionNumber = exceptionNumber;
		}

		public void ThrowException() {
			switch (exceptionNumber) {
				case 0: throw new Exception("Hello");
				case 1: throw new Exception("Disregard this message!");
				case 2: throw new ArgumentException("Disregard this message!");
				case 3: throw new ArgumentException("Booyah!");
				case 4: throw new ArgumentException("Other message");
			}
		}
	}
}
