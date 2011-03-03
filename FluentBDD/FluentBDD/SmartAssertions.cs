using System;
using NUnit.Framework;

namespace FluentBDD {
	public class SmartAssertions<TSubject, TResult, TExpectations> {


		public Subject subject (Func<TSubject, object> getFromSubject) {
			return new Subject(getFromSubject);
		}

		public class Subject : IShould_s, IMatch {
			private readonly Func<TSubject, object> getFromSubject;
			private bool invert;

			public IShould_s should { get { return this; } }

			public Subject (Func<TSubject, object> getFromSubject) {
				this.getFromSubject = getFromSubject;
			}

			IShould_s IShould_s.not { get { invert = true; return this; } }


			Action<TSubject> IShould_s.equal (object value) {
				if (invert) return subject => Assert.That(getFromSubject(subject), Is.Not.EqualTo(value));
				return subject => Assert.That(getFromSubject(subject), Is.EqualTo(value));
			}

			IMatch IShould.match { get { return this; } }

			Action<TSubject, TResult, TExpectations> IMatch.expectation (Func<TExpectations, object> valuesTest) {
				if (invert) return (subject, result, values) => Assert.That(getFromSubject(subject), Is.Not.EqualTo(valuesTest(values)));
				return (subject, result, values) => Assert.That(getFromSubject(subject), Is.EqualTo(valuesTest(values)));
			}


		}

		public Result result { get { return new Result(); } }
		public class Result : IShould_sr, IMatch {
			private bool invert;

			public IShould_sr should {
				get { return this; }
			}

			IMatch IShould.match { get { return this; } }
			IShould_sr IShould_sr.not {
				get {
					invert = true;
					return this;
				}
			}

			Action<TSubject, TResult> IShould_sr.equal (TResult value) {
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
		public interface IShould {
			IMatch match { get; }
		}

		public interface IShould_s : IShould {
			IShould_s not { get; }
			Action<TSubject> equal (object value);
		}
		public interface IShould_sr : IShould {
			IShould_sr not { get; }
			Action<TSubject, TResult> equal (TResult value);
		}

	}
}
