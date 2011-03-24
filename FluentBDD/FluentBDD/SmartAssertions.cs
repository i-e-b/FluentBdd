using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace FluentBDD {
	public class SmartAssertions<TSubject, TResult, TExpectations> {

		private static bool AsBool (object thing) {
			var asbool = thing as bool?;
			if (!asbool.HasValue) throw new ArgumentException("Can't cast from " + thing.GetType().Name + " to bool");
			return asbool.Value;
		}

		public Subject subject (Func<TSubject, object> getFromSubject) {
			return new Subject(getFromSubject);
		}

		public Result result { get { return new Result(); } }

		public Action<TSubject> should_be_ignored {
			get { return subject => Assert.Ignore(); }
		}

		public class Subject : IAssertWithSubject, IMatch, IAm_Subject, IAmSameAs_Subject {
			private readonly Func<TSubject, object> getFromSubject;
			private bool invert;

			public IAssertWithSubject should { get { return this; } }

			public Subject (Func<TSubject, object> getFromSubject) {
				this.getFromSubject = getFromSubject;
			}

			IAssertWithSubject IAssertWithSubject.not { get { invert = true; return this; } }
			IAm_Subject IAssertWithSubject.be { get { return this; } }

			Action<TSubject> IAm_Subject._true {
				get {
					if (invert) return subject => Assert.That(AsBool(getFromSubject(subject)), Is.False);
					return subject => Assert.That(AsBool(getFromSubject(subject)), Is.True);
				}
			}

			Action<TSubject> IAm_Subject._false {
				get {
					invert = !invert;
					return (this as IAm_Subject)._true;
				}
			}

			Action<TSubject> IAssertWithSubject.equal (object value) {
				if (invert) return subject => Assert.That(getFromSubject(subject), Is.Not.EqualTo(value));
				return subject => Assert.That(getFromSubject(subject), Is.EqualTo(value));
			}

			IMatch IAssertWithSubject.match { get { return this; } }

			Action<TSubject, TResult, TExpectations> IMatch.expectation (Func<TExpectations, object> valuesTest) {
				if (invert) return (subject, result, values) => Assert.That(getFromSubject(subject), Is.Not.EqualTo(valuesTest(values)));
				return (subject, result, values) => Assert.That(getFromSubject(subject), Is.EqualTo(valuesTest(values)));
			}


			Action<TSubject, TResult> IMatch.subject (Func<TSubject, object> valuesTest) {
				return (subject,result) => TestSubjectAgainstSubject(valuesTest, Assert.AreEqual, Assert.AreNotEqual)(subject);
			}

			private Action<TSubject> TestSubjectAgainstValue(object value, Action<object, object> forwardTest, Action<object, object> inverseTest) {
				if (invert) return subject => inverseTest(getFromSubject(subject), value);
				return subject => forwardTest(getFromSubject(subject), value);
			}
			private Action<TSubject> TestSubjectAgainstSubject(Func<TSubject, object> valuesTest, Action<object, object> forwardTest, Action<object, object> inverseTest) {
				if (invert) return subject => inverseTest(getFromSubject(subject), valuesTest(subject));
				return subject => forwardTest(getFromSubject(subject), valuesTest(subject));
			}
			private Action<TSubject, TResult, TExpectations> TestSubjectAgainstExpectations(Func<TExpectations, TResult> valuesTest, Action<object, object> forwardTest, Action<object, object> inverseTest) {
				if (invert) return (subject, result, values) => inverseTest(getFromSubject(subject), valuesTest(values));
				return (subject, result, values) => forwardTest(getFromSubject(subject), valuesTest(values));
			}

			IAmSameAs_Subject IAm_Subject.the_same { get { return this; } }

			Action<TSubject> IAmSameAs_Subject._as (object value) {
				return TestSubjectAgainstValue(value, Assert.AreSame, Assert.AreNotSame);
			}

			Action<TSubject, TResult, TExpectations> IAmSameAs_Subject.as_expectation (Func<TExpectations, TResult> valuesTest) {
				return TestSubjectAgainstExpectations(valuesTest, Assert.AreSame, Assert.AreNotSame);
			}

			Action<TSubject> IAmSameAs_Subject.as_subject (Func<TSubject, object> valuesTest) {
				return TestSubjectAgainstSubject(valuesTest, Assert.AreSame, Assert.AreNotSame);
			}
		}
		public class Result : IAssertWithSubjectAndResult, IMatch, IAm_Result, IAmSameAs_Result {
			private bool invert;

			public IAssertWithSubjectAndResult should {get { return this; }}
			IMatch IAssertWithSubjectAndResult.match { get { return this; } }
			IAssertWithSubjectAndResult IAssertWithSubjectAndResult.not {get {invert = true;return this;}}
			IAm_Result IAssertWithSubjectAndResult.be { get { return this; } }
			IAmSameAs_Result IAm_Result.the_same { get { return this; } }

				Action<TSubject, TResult> IAm_Result._true {
				get {
					if (!(typeof(bool).IsAssignableFrom(typeof(TResult))))
						throw new ArgumentException("Can't cast from " + typeof(TResult).Name + " to bool");

					if (invert) return (subject, result) => Assert.That(AsBool(result), Is.False);
					return (subject, result) => Assert.That(AsBool(result), Is.True);
				}
			}
			Action<TSubject, TResult> IAm_Result._false {
				get {
					invert = !invert;
					return (this as IAm_Result)._true;
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

			Action<TSubject, TResult> IMatch.subject (Func<TSubject, object> valuesTest) {
				if (invert) return (subject, result) => Assert.That(result, Is.Not.EqualTo(valuesTest(subject)));
				return (subject, result) => Assert.That(result, Is.EqualTo(valuesTest(subject)));
			}


			Action<TSubject, TResult> IAmSameAs_Result._as (TResult value) {
				if (invert) return (subject, result) => Assert.That(result, Is.Not.SameAs(value));
				return (subject, result) => Assert.That(result, Is.EqualTo(value));
			}

			Action<TSubject, TResult, TExpectations> IAmSameAs_Result.as_expectation (Func<TExpectations, TResult> valuesTest) {
				if (invert) return (subject, result, values) => Assert.That(result, Is.Not.EqualTo(valuesTest(values)));
				return (subject, result, values) => Assert.That(result, Is.EqualTo(valuesTest(values)));
			}

			Action<TSubject, TResult> IAmSameAs_Result.as_subject (Func<TSubject, TResult> valuesTest) {
				if (invert) return (subject, result) => Assert.That(result, Is.Not.EqualTo(valuesTest(subject)));
				return (subject, result) => Assert.That(result, Is.EqualTo(valuesTest(subject)));
			}
		}

		public interface IMatch {
			Action<TSubject, TResult, TExpectations> expectation (Func<TExpectations, object> valuesTest);
			Action<TSubject, TResult> subject (Func<TSubject, object> func);
		}

		public interface IAssertWithSubject {
			IMatch match { get; }
			IAssertWithSubject not { get; }
			IAm_Subject be { get; }
			Action<TSubject> equal (object value);
		}
		public interface IAm_Subject {
			Action<TSubject> _true { get; }
			Action<TSubject> _false { get; }
			IAmSameAs_Subject the_same { get; }
		}
		public interface IAmSameAs_Subject {
			Action<TSubject> _as (object value);
			Action<TSubject, TResult, TExpectations> as_expectation (Func<TExpectations, TResult> valuesTest);
			Action<TSubject> as_subject(Func<TSubject, object> valuesTest);
		}

		public interface IAssertWithSubjectAndResult {
			IMatch match { get; }
			IAssertWithSubjectAndResult not { get; }
			IAm_Result be { get; }
			Action<TSubject, TResult> equal (TResult value);
		}
		public interface IAm_Result {
			Action<TSubject, TResult> _true { get; }
			Action<TSubject, TResult> _false { get; }
			IAmSameAs_Result the_same { get; }
		}
		public interface IAmSameAs_Result {
			Action<TSubject, TResult> _as(TResult value);
			Action<TSubject, TResult, TExpectations> as_expectation(Func<TExpectations, TResult> valuesTest);
			Action<TSubject, TResult> as_subject(Func<TSubject, TResult> valuesTest);
		}

	}

}
