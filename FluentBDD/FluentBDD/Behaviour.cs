using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace FluentBDD {
	[EditorBrowsable(EditorBrowsableState.Always)]
	public abstract class Behaviour: IBuildTests {
		IEnumerable<TestClosure> IBuildTests.BuildTests() {
			return null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Behaviour<TSubject, TResult> : Behaviour, ITakeMessage, IBuildTests {
		protected readonly string Description;
		protected readonly IEnumerable<Func<Context<TSubject>>> contextSources;
		protected readonly Func<TSubject, Context<TSubject>, TResult> scenarioAction;

		internal readonly List<Group<string, Action<TSubject>>> subjectOnlyTests;
		internal readonly List<Group<string, Action<TSubject, TResult>>> subjectAndResultTests;
		internal readonly List<Group<string, Func<Exception>>> exceptionTests;

		protected Type expectedExceptionType;
		protected string expectedExceptionMessage;

		#region CTORs
		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();
			exceptionTests = new List<Group<string, Func<Exception>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				action(subject);
				return default(TResult);
			};
		}

		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject, Context<TSubject>> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();
			exceptionTests = new List<Group<string, Func<Exception>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				action(subject, context);
				return default(TResult);
			};
		}

		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, TResult> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();
			exceptionTests = new List<Group<string, Func<Exception>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				return action(subject);
			};
		}

		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, Context<TSubject>, TResult> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();
			exceptionTests = new List<Group<string, Func<Exception>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				return action(subject, context);
			};
		}
		#endregion

		#region THENs
		public Behaviour<TSubject, TResult> Then (string description, Action<TSubject> subjectOnlyTest) {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(description, subjectOnlyTest));
			return this;
		}

		public Behaviour<TSubject, TResult> Then (string description, Action<TSubject, TResult> subjectAndResultTest) {
			subjectAndResultTests.Add(new Group<string, Action<TSubject, TResult>>(description, subjectAndResultTest));
			return this;
		}
		#endregion

		#region Exception cases
		public virtual ITakeMessage ShouldThrow<TException> () where TException : Exception {
			expectedExceptionType = typeof(TException);
			subjectOnlyTests.Insert(0, new Group<string, Action<TSubject>>(
				"should throw " + expectedExceptionType.Name, subject => { }));
			return this;
		}

		Behaviour ITakeMessage.WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectOnlyTests.Insert(0, new Group<string, Action<TSubject>>(
				"should have exception message \"" + expectedExceptionMessage+"\"", subject => { }));
			return this;
		}

		Behaviour ITakeMessage.IgnoreMessage () {
			return this;
		}

		
		public Behaviour<TSubject, TResult> Then(string description, Func<Exception> sampleExceptionSource) {
			exceptionTests
				.Add(new Group<string, Func<Exception>>(
				     	description,
				     	sampleExceptionSource
				     	));
			return this;
		}
		#endregion

		#region Test closure building. These are what get run in NUnit.
		private IEnumerable<Func<Context<TSubject>>> BuildSubjects () {
			foreach (var contextSource in contextSources) {
				var source = contextSource;
				yield return source;
			}
			yield break;
		}

		IEnumerable<TestClosure> IBuildTests.BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
			                      from contextSource in BuildSubjects()
			                      select new TestClosure(
									"Given " + contextSource().SetupAndReturnContextBuilder().Description,
			                      	"When " + Description, "Then " + test.A,
			                      	() => {
			                      		var context = contextSource();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		scenarioAction(subject, context);
			                      		test.B(subject);
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => contextSource().TearDown()
									));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
								  from contextSource in BuildSubjects()
			                      select new TestClosure(
									"Given " + contextSource().SetupAndReturnContextBuilder().Description,
									"When " + Description, "Then " + test.A,
									() => {
										var context = contextSource();
										var subject = context.SetupAndReturnContextBuilder().Build();
										test.B(subject, scenarioAction(subject, context));
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => contextSource().TearDown()));

			
			//3
			testClosures.AddRange(from test in exceptionTests
								  from contextSource in BuildSubjects()
			                      select new TestClosure(
									"Given " + contextSource().SetupAndReturnContextBuilder().Description,
									"When " + Description, "Then " + test.A,
									() => {
										var context = contextSource();
										var subject = context.SetupAndReturnContextBuilder().Build();
										scenarioAction(subject, context);
									}, () => test.B().GetType(), () => test.B().Message,
									() => contextSource().TearDown()));

			return testClosures;
		}
		#endregion

		#region Reflection tests
		public Behaviour<TSubject, TResult> ShouldHaveAttribute<TAttributeType> () where TAttributeType : Attribute {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have " + typeof (TAttributeType).Name,
			                     	subject => Assert.IsNotEmpty(
												typeof (TSubject)
			                     	           	.GetCustomAttributes(typeof (TAttributeType), true)
			                     	           	.Select(attribute => attribute as TAttributeType)
			                     	           	.ToArray())));
			return this;
		}

		public Behaviour<TSubject, TResult> ShouldHaveField (string fieldName) {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have a field named \"" + fieldName + "\"",
			                     	subject => Assert.IsNotNull(
										typeof (TSubject).GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
			                     	           	)
			                     	));
			return this;
		}

		public Behaviour<TSubject, TResult> ShouldHaveFieldWithAttribute<TAttributeType> (string fieldName, Func<TAttributeType, bool> condition) where TAttributeType : Attribute {
			subjectOnlyTests.Add(new Group<string, Action<TSubject>>(
			                     	"Should have a field named \"" + fieldName + "\" with "+typeof(TAttributeType).Name,
			                     	subject => {
			                     		var field = typeof(TSubject).GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
										if (field == null) throw new ArgumentException("Field not found: "+fieldName);
			                     		var attribs = field.GetCustomAttributes(typeof (TAttributeType), true).Select(attribute => attribute as TAttributeType);

			                     		Assert.That(attribs.Any(condition), Is.True);
			                     	}));
			return this;
		}

		public Behaviour<TSubject, TResult> ShouldHaveFieldWithAttribute<TAttributeType> (string fieldName) where TAttributeType : Attribute {
			return ShouldHaveFieldWithAttribute<TAttributeType>(fieldName, a => true);
		}
		#endregion
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class BehaviourWithoutAnAction<TSubject, TExampleType, TExampleSource> : Behaviour where TExampleSource : class, TExampleType, IProvide<TExampleType>, new() {
		protected readonly List<Func<Context<TSubject>>> ContextSources;

		public BehaviourWithoutAnAction (List<Func<Context<TSubject>>> contextSources) {
			ContextSources = contextSources;
		}

		public BehaviourWithoutAnAction<TSubject, TExampleType, TExampleSource> 
			And<TContext> () where TContext : Context<TSubject>, new()  {
			ContextSources.Add(() => new TContext());
			return this;
		}

		public Behaviour<TSubject, TResult, TExampleType, TExampleSource> When<TResult> (string description, Func<TSubject, IUse<TExampleType>, TResult> action) {
			return new Behaviour<TSubject, TResult, TExampleType, TExampleSource>(description, ContextSources, (subject, context) => action(subject, (IUse<TExampleType>)context ));
		}

		public Behaviour<TSubject, no_result, TExampleType, TExampleSource> When (string description, Action<TSubject, IUse<TExampleType>> action) {
			return new Behaviour<TSubject, no_result, TExampleType, TExampleSource>(description, ContextSources, (subject, context) => action(subject, (IUse<TExampleType>)context));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Behaviour<TSubject, TResult, TExampleType, TExampleSource> : Behaviour<TSubject, TResult>, ITakeMessage, IBuildTests where TExampleSource : class, TExampleType, IProvide<TExampleType>, new() {

		internal readonly List<Group<string, Action<TSubject, TResult, TExampleSource>>> subjectAndResultAndExampleTests;
		internal readonly List<Group<string, Action<TSubject, TExampleSource>>> subjectAndExampleTests;
		internal readonly List<Group<string, Func<TExampleType, Exception>>> specificExceptionTests;

		#region CTORs
		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject, Context<TSubject>> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExampleSource>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExampleSource>>>();
			specificExceptionTests = new List<Group<string, Func<TExampleType, Exception>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}

		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, Context<TSubject>, TResult> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExampleSource>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExampleSource>>>();
			specificExceptionTests = new List<Group<string, Func<TExampleType, Exception>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}
		#endregion

		#region THENs
		public Behaviour<TSubject, TResult, TExampleType, TExampleSource> Then (string description, Action<TSubject, TResult, TExampleSource> subjectAndResultAndExampleTest) {
			subjectAndResultAndExampleTests.Add(new Group<string, Action<TSubject, TResult, TExampleSource>>(description, subjectAndResultAndExampleTest));
			return this;
		}

		public Behaviour<TSubject, TResult, TExampleType, TExampleSource> Then (string description, Action<TSubject, TExampleSource> subjectAndExampleTest) {
			subjectAndExampleTests.Add(new Group<string, Action<TSubject, TExampleSource>>(description, subjectAndExampleTest));
			return this;
		}
		#endregion

		#region Exception cases
		public override ITakeMessage ShouldThrow<TException> () {
			expectedExceptionType = typeof(TException);
			subjectAndExampleTests.Insert(0, new Group<string, Action<TSubject, TExampleSource>>(
				"should throw " + expectedExceptionType.Name, (s, e) => { }));
			return this;
		}

		Behaviour ITakeMessage.WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectAndExampleTests.Insert(0, new Group<string, Action<TSubject, TExampleSource>>(
				"should have exception message \"" + expectedExceptionMessage + "\"", (s, e) => { }));
			return this;
		}

		Behaviour ITakeMessage.IgnoreMessage () {
			return this;
		}

		public Behaviour<TSubject, TResult, TExampleType, TExampleSource> Then(string description, Func<TExampleType, Exception> sampleExceptionSource) {
			specificExceptionTests
				.Add(new Group<string, Func<TExampleType, Exception>>(
				     	description,
				     	sampleExceptionSource
				     	));
			return this;
		}
		
		public Behaviour<TSubject, TResult, TExampleType, TExampleSource> ShouldThrowException(Func<TExampleType, Exception> sampleExceptionSource) {
			specificExceptionTests
				.Add(new Group<string, Func<TExampleType, Exception>>(
				     	"should throw exception",
				     	sampleExceptionSource
				     	));
			return this;
		}
		#endregion

		#region Test closure building. Don't bother looking until you've comprehended the simpler versions without examples.
		/// <summary>
		/// Build a subject for each combination of context and examples.
		/// </summary>
		private IEnumerable<Group<Func<Context<TSubject>>, IProvide<TExampleType>>> BuildSubjectsWithExamples () {
			var exampleData = new TExampleSource().Data();
			foreach (var contextSource in contextSources) {
				var context = contextSource();
				if (context is IUse<TExampleType>) {
					for (int exampleIndex = 0; exampleIndex < exampleData.Length; exampleIndex++) {
						var example = exampleData[exampleIndex];
						int index = exampleIndex;
						Func<Context<TSubject>> source = contextSource;
						yield return new Group<Func<Context<TSubject>>, IProvide<TExampleType>>(
							() => {
								var closureExample = new TExampleSource().Data()[index];
								var fresh_context = source();
								((IUse<TExampleType>)fresh_context).Values = closureExample;
								return fresh_context;
							},
							example as IProvide<TExampleType>);
					}
				} else {
					throw new InvalidOperationException("Expected " + context.GetType().FullName + " to implement IUse<" + typeof (TExampleType).Name + ">, but could not cast it.");
				}
			}
			yield break;
		}

		IEnumerable<TestClosure> IBuildTests.BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
								  from tuple in BuildSubjectsWithExamples()
			                      select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		scenarioAction(subject, context);
			                      		test.B(subject);
			                      	}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
								  from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A, 
									" with " + tuple._2<TExampleSource>().StringRepresentation(),
									() => {
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		test.B(subject, scenarioAction(subject, context));
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//3
			testClosures.AddRange(from test in subjectAndResultAndExampleTests
								  from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A, 
									" with " + tuple._2<TExampleSource>().StringRepresentation(),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExampleType>)context).Values;
			                      		var result = scenarioAction(subject, context);
										test.B(subject, result, example as TExampleSource);
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			
			//4
			testClosures.AddRange(from test in subjectAndExampleTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									" with " + tuple._2<TExampleSource>().StringRepresentation(),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExampleType>)context).Values;
			                      		scenarioAction(subject, context);
										test.B(subject, example as TExampleSource);
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//5: Tests for exceptions matching an example exception
			testClosures.AddRange(from test in specificExceptionTests
								  from tuple in BuildSubjectsWithExamples()
			                      select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									GetWithExceptionName(tuple, test),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExampleType>)context).Values;
			                      		scenarioAction(subject, context);
										test.B(example as TExampleSource);
			                      	},
			                      	() => GetExceptionResultType(tuple, test),
			                      	() => GetExceptionMessage(tuple, test),
									() => GetContext(tuple).TearDown()
			                      	));


			return testClosures;
		}

		private Context<TSubject> GetContext(Group<Func<Context<TSubject>>, IProvide<TExampleType>> tuple) {
			return tuple._1<Func<Context<TSubject>>>().Invoke();
		}
		private Exception GetException(Group<Func<Context<TSubject>>, IProvide<TExampleType>> tuple, Group<string, Func<TExampleType, Exception>> test) {
			var context = tuple._1<Func<Context<TSubject>>>().Invoke();
			var example = ((IUse<TExampleType>)context).Values;
			return test.B(example as TExampleSource);	
		}
		private Type GetExceptionResultType (Group<Func<Context<TSubject>>, IProvide<TExampleType>> tuple, Group<string, Func<TExampleType, Exception>> test) {
			return GetException(tuple, test).GetType();
		}
		private string GetExceptionMessage (Group<Func<Context<TSubject>>, IProvide<TExampleType>> tuple, Group<string, Func<TExampleType, Exception>> test) {
			var message = GetException(tuple, test).Message;
			if (string.IsNullOrEmpty(message)) return null;
			return message;
		}
		private string GetWithExceptionName (Group<Func<Context<TSubject>>, IProvide<TExampleType>> tuple, Group<string, Func<TExampleType, Exception>> test) {
			var exception = GetException(tuple, test);
			if (string.IsNullOrEmpty(exception.Message)) return "With exception of type " + exception.GetType() + ", ignoring message, with " + tuple._2<TExampleSource>().StringRepresentation();
			return "With exception of type " + exception.GetType() + " and message \"" + exception.Message + "\", with " + tuple._2<TExampleSource>().StringRepresentation();
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class no_values : IProvide<no_values> {
		public no_values[] Data(){return new no_values[]{};}
		public string StringRepresentation(){return "### ERROR: use of expectation values but none provided ###";}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ITakeMessage {
		Behaviour WithMessage (string expectedMessage);
		Behaviour IgnoreMessage();
	}
}