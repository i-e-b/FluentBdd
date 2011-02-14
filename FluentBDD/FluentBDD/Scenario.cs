using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentBDD {
	public abstract class Scenario {
		internal abstract IEnumerable<TestClosure> BuildTests ();
	}

	public class Scenario<TSubject, TResult> : Scenario, ITakeMessage {
		protected readonly string Description;
		protected readonly IEnumerable<Func<Context<TSubject>>> contextSources;
		protected readonly Func<TSubject, TResult> scenarioAction;

		internal readonly List<Group<string, Action<TSubject>>> subjectOnlyTests;
		internal readonly List<Group<string, Action<TSubject, TResult>>> subjectAndResultTests;

		protected Type expectedExceptionType;
		protected string expectedExceptionMessage;

		#region CTORs
		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			this.Description = description;
			this.contextSources = contextSources;
			scenarioAction = subject =>
			{
				action(subject);
				return default(TResult);
			};
		}

		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, TResult> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			this.Description = description;
			this.contextSources = contextSources;
			scenarioAction = action;
		}
		#endregion

		#region USING for IUse/IProvide
		public ScenarioWithExamples<TSubject, TResult, TExampleSource> Using<TExampleSource> () where TExampleSource : IProvide<TExampleSource>, new() {
			return new ScenarioWithExamples<TSubject, TResult, TExampleSource>(Description, contextSources, scenarioAction);
		}
		#endregion

		#region THENs
		public Scenario<TSubject, TResult> Then (string description, Action<TSubject> subjectOnlyTest) {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(description, subjectOnlyTest));
			return this;
		}

		public Scenario<TSubject, TResult> Then (string description, Action<TSubject, TResult> subjectAndResultTest) {
			subjectAndResultTests.Add(new Group<string, Action<TSubject, TResult>>(description, subjectAndResultTest));
			return this;
		}
		#endregion

		#region Exception cases
		public virtual ITakeMessage ShouldThrow<TException> () where TException : Exception {
			expectedExceptionType = typeof(TException);
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
				"Should throw " + expectedExceptionType.Name, subject => { }));
			return this;
		}

		public virtual Scenario WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
				"Should have exception message \"" + expectedExceptionMessage+"\"", subject => { }));
			return this;
		}
		#endregion

		#region Test closure building. These are what get run in NUnit.
		private IEnumerable<Func<SubjectBuilder<TSubject>>> BuildSubjects () {
			foreach (var contextSource in contextSources) {
				var builder = contextSource().SetupAndReturnContextBuilder();
				yield return () => builder;
			}
			yield break;
		}

		internal override IEnumerable<TestClosure> BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
			                      from subject in BuildSubjects()
			                      select new TestClosure(
			                      	subject().Description,
			                      	"When " + Description, test.A,
			                      	() => {
			                      		var s = subject().Build();
			                      		scenarioAction(s);
			                      		test.B(s);
									}, expectedExceptionType, expectedExceptionMessage));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
			                      from subject in BuildSubjects()
			                      select new TestClosure(
			                      	subject().Description,
			                      	"When " + Description, test.A,
			                      	() => {
			                      		var s = subject().Build();
			                      		test.B(s, scenarioAction(s));
									}, expectedExceptionType, expectedExceptionMessage));

			return testClosures;
		}
		#endregion

		#region Reflection tests
		public Scenario<TSubject, TResult> ShouldHaveAttribute<TAttributeType> () where TAttributeType : Attribute {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have " + typeof (TAttributeType).Name,
			                     	subject => typeof (TSubject)
			                     	           	.GetCustomAttributes(typeof (TAttributeType), true)
			                     	           	.Select(attribute => attribute as TAttributeType)
			                     	           	.ToArray().should_not_be_empty()));
			return this;
		}

		public Scenario<TSubject, TResult> ShouldHaveField (string fieldName) {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have a field named \"" + fieldName + "\"",
			                     	subject => typeof (TSubject)
			                     	           	.GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
			                     	           	.should_not_be_null()
			                     	));
			return this;
		}

		public Scenario<TSubject, TResult> ShouldHaveFieldWithAttribute<TAttributeType> (string fieldName, Func<TAttributeType, bool> condition) where TAttributeType : Attribute {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have a field named \"" + fieldName + "\" with "+typeof(TAttributeType).Name,
			                     	subject => {
			                     		var field = typeof(TSubject).GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
										if (field == null) throw new ArgumentException("Field not found: "+fieldName);
			                     		var attribs = field.GetCustomAttributes(typeof (TAttributeType), true).Select(attribute => attribute as TAttributeType);
			                     		attribs.should_contain(condition);
			                     	}));
			return this;
		}

		public Scenario<TSubject, TResult> ShouldHaveFieldWithAttribute<TAttributeType> (string fieldName) where TAttributeType : Attribute {
			return ShouldHaveFieldWithAttribute<TAttributeType>(fieldName, a => true);
		}
		#endregion
	}

	public interface ITakeMessage {
		Scenario WithMessage (string expectedMessage);
	}

	public class ScenarioWithExamples<TSubject, TResult, TExample> : Scenario<TSubject, TResult> where TExample : IProvide<TExample>, new() {
		internal readonly List<Group<string, Action<TSubject, TResult, TExample>>> subjectAndResultAndExampleTests;
		internal readonly List<Group<string, Action<TSubject, TExample>>> subjectAndExampleTests;

		#region CTORs
		public ScenarioWithExamples (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExample>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExample>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}

		public ScenarioWithExamples (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, TResult> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExample>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExample>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}
		#endregion

		#region THENs
		public ScenarioWithExamples<TSubject, TResult, TExample> Then (string description, Action<TSubject, TResult, TExample> subjectAndResultAndExampleTest) {
			subjectAndResultAndExampleTests.Add(new Group<string, Action<TSubject, TResult, TExample>>(description, subjectAndResultAndExampleTest));
			return this;
		}

		public ScenarioWithExamples<TSubject, TResult, TExample> Then (string description, Action<TSubject, TExample> subjectAndExampleTest) {
			subjectAndExampleTests.Add(new Group<string, Action<TSubject, TExample>>(description, subjectAndExampleTest));
			return this;
		}
		#endregion

		#region Exception cases
		public override ITakeMessage ShouldThrow<TException> () {
			expectedExceptionType = typeof(TException);
			subjectAndExampleTests.Add(new Group<string, Action<TSubject, TExample>>(
				"Should throw " + expectedExceptionType.Name, (s, e) => { }));
			return this;
		}

		public override Scenario WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectAndExampleTests.Add(new Group<string, Action<TSubject, TExample>>(
				"Should have exception message \"" + expectedExceptionMessage + "\"", (s, e) => { }));
			return this;
		}
		#endregion

		#region Test closure building. Don't bother looking until you've comprehended the simpler versions without examples.
		/// <summary>
		/// Build a subject for each combination of context and examples.
		/// </summary>
		private IEnumerable<Group<Func<Context<TSubject>>, TExample>> BuildSubjectsWithExamples () {
			var exampleData = new TExample().Data();
			foreach (var contextSource in contextSources) {
				var context = contextSource();
				if (context is IUse<TExample>) {
					for (int exampleIndex = 0; exampleIndex < exampleData.Length; exampleIndex++) {
						var example = exampleData[exampleIndex];
						int index = exampleIndex;
						yield return new Group<Func<Context<TSubject>>, TExample>(
							() => {
								var closureExample = new TExample().Data()[index];
								var fresh_context = contextSource();
								((IUse<TExample>)fresh_context).Values = closureExample;
								return fresh_context;
							},
							example);
					}
				} else {
					yield return new Group<Func<Context<TSubject>>, TExample>(
						() => contextSource(),
						default(TExample));
				}
			}
			yield break;
		}

		internal override IEnumerable<TestClosure> BuildTests () {
			// Cause = ActionName, Effect = Then.description,
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
			                      from tuple in BuildSubjectsWithExamples()
			                      select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
			                      	"When " + Description, test.A,
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		scenarioAction(subject);
			                      		test.B(subject);
			                      	}, expectedExceptionType, expectedExceptionMessage));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExample>().StringRepresentation(),
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		test.B(subject, scenarioAction(subject));
									}, expectedExceptionType, expectedExceptionMessage));

			//3
			testClosures.AddRange(from test in subjectAndResultAndExampleTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExample>().StringRepresentation(),
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExample>)context).Values;
			                      		var result = scenarioAction(subject);
										test.B(subject, result, example);
									}, expectedExceptionType, expectedExceptionMessage));

			//4
			testClosures.AddRange(from test in subjectAndExampleTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExample>().StringRepresentation(),
			                      	() => {
			                      		var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		var example = ((IUse<TExample>)context).Values;
			                      		scenarioAction(subject);
										test.B(subject, example);
									}, expectedExceptionType, expectedExceptionMessage));

			return testClosures;
		}

		private string GetLambdaBuilderDescription (Func<Context<TSubject>> builderFunc) {
			try {
				return builderFunc().SetupAndReturnContextBuilder().Description;
			} catch (Exception ex) {
				return "Setup failed: " + ex.GetType() + ", " + ex.Message;
			}
		}
		#endregion
	}
}