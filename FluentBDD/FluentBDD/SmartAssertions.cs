using System;
using NUnit.Framework;

namespace FluentBDD {
	public class SmartAssertions<TSubject, TResult, TExpectations> {


		public Subject subject (Func<TSubject, object> getFromSubject) {
			return new Subject(getFromSubject);
		}

		public class Subject : IAssertWithSubject, IMatch {
			private readonly Func<TSubject, object> getFromSubject;
			private bool invert;

			public IAssertWithSubject should { get { return this; } }

			public Subject (Func<TSubject, object> getFromSubject) {
				this.getFromSubject = getFromSubject;
			}

			IAssertWithSubject IAssertWithSubject.not { get { invert = true; return this; } }


			Action<TSubject> IAssertWithSubject.equal (object value) {
				if (invert) return subject => Assert.That(getFromSubject(subject), Is.Not.EqualTo(value));
				return subject => Assert.That(getFromSubject(subject), Is.EqualTo(value));
			}

			IMatch IAssertWithSubject.match { get { return this; } }

			Action<TSubject, TResult, TExpectations> IMatch.expectation (Func<TExpectations, object> valuesTest) {
				if (invert) return (subject, result, values) => Assert.That(getFromSubject(subject), Is.Not.EqualTo(valuesTest(values)));
				return (subject, result, values) => Assert.That(getFromSubject(subject), Is.EqualTo(valuesTest(values)));
			}


		}

		public Result result { get { return new Result(); } }
		public class Result : IAssertWithSubjectAndResult, IMatch {
			private bool invert;

			public IAssertWithSubjectAndResult should {
				get { return this; }
			}

			IMatch IAssertWithSubjectAndResult.match { get { return this; } }
			IAssertWithSubjectAndResult IAssertWithSubjectAndResult.not {
				get {
					invert = true;
					return this;
				}
			}

			Action<TSubject, TResult> IAssertWithSubjectAndResult.equal (TResult value) {
				if (invert) return (subject, result) => Assert.That(result, Is.Not.EqualTo(value));
				return (subject, result) => Assert.That(result, Is.EqualTo(value));
			}

			Action<TSubject, TResult, TExpectations> IMatch.expectation (Func<TExpectations, object> valuesTest) {
				if (invert) return (subject, result, values) => Assert.That(result, Is.Not.EqualTo(valuesTest(values)));
				return (subject, result, values) => Assert.That(result, Is.EqualTo(valuesTest(values)));
			}

		}

		public interface IMatch {
			Action<TSubject, TResult, TExpectations> expectation (Func<TExpectations, object> valuesTest);
		}

		public interface IAssertWithSubject {
			IMatch match { get; }
			IAssertWithSubject not { get; }
			Action<TSubject> equal (object value);
		}
		public interface IAssertWithSubjectAndResult {
			IMatch match { get; }
			IAssertWithSubjectAndResult not { get; }
			Action<TSubject, TResult> equal (TResult value);
		}

	}
}
