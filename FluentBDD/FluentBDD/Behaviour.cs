using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using FluentBDD.Assertions;
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
		protected readonly Func<TSubject, Context<TSubject>, TResult> behaviourAction;

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
			behaviourAction = (subject, context) =>
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
			behaviourAction = (subject, context) =>
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
			behaviourAction = (subject, context) =>
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
			behaviourAction = (subject, context) =>
			{
				return action(subject, context);
			};
		}
		#endregion

		#region THENs
		internal Behaviour<TSubject, TResult> Then (string description, Action<TSubject, TResult> subjectAndResultTest) {
			subjectAndResultTests.Add(new Group<string, Action<TSubject, TResult>>(description, subjectAndResultTest));
			return this;
		}

		public SmartAssertionBase<TSubject, TResult> Then (string description) {
			return new SmartAssertionBase<TSubject, TResult>(description, this);
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


		internal Behaviour<TSubject, TResult> Then (string description, Func<Exception> sampleExceptionSource) {
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
			                      		behaviourAction(subject, context);
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
										test.B(subject, behaviourAction(subject, context));
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
										behaviourAction(subject, context);
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
	public class BehaviourWithoutAnAction<TSubject, TProofType, TProofSource> : Behaviour where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		protected readonly List<Func<Context<TSubject>>> ContextSources;

		public BehaviourWithoutAnAction (List<Func<Context<TSubject>>> contextSources) {
			ContextSources = contextSources;
		}
		
		/// <summary>
		/// Add another context which shares the same subject and behaviour as the first.
		/// </summary>
		public BehaviourWithoutAnAction<TSubject, TProofType, TProofSource> 
			AlsoGiven<TContext> () where TContext : Context<TSubject>, new()  {
			ContextSources.Add(() => new TContext());
			return this;
		}

		public Behaviour<TSubject, TResult, TProofType, TProofSource> When<TResult> (string description, Func<TSubject, TProofType, TResult> action) {
			return new Behaviour<TSubject, TResult, TProofType, TProofSource>(description, ContextSources, (subject, context) => action(subject,((IUse<TProofType>)context).Values ));
		}

		public Behaviour<TSubject, no_result, TProofType, TProofSource> When (string description, Action<TSubject, TProofType> action) {
			return new Behaviour<TSubject, no_result, TProofType, TProofSource>(description, ContextSources, (subject, context) => action(subject, ((IUse<TProofType>)context).Values));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Behaviour<TSubject, TResult, TProofType, TProofSource> : Behaviour<TSubject, TResult>, ITakeMessage, IBuildTests where TProofSource : class, TProofType, IProvide<TProofType>, new() {

		internal readonly List<Group<string, Action<TSubject, TResult, TProofSource>>> subjectAndResultWithProofsTests;
		internal readonly List<Group<string, Action<TSubject, TProofSource>>> subjectWithProofsTests;
		internal readonly List<Group<string, Func<TProofType, Exception>>> specificExceptionTests;

		#region CTORs
		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject, Context<TSubject>> action)
			: base(description, contextSources, action) {
			subjectAndResultWithProofsTests = new List<Group<string, Action<TSubject, TResult, TProofSource>>>();
			subjectWithProofsTests = new List<Group<string, Action<TSubject, TProofSource>>>();
			specificExceptionTests = new List<Group<string, Func<TProofType, Exception>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}

		public Behaviour (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, Context<TSubject>, TResult> action)
			: base(description, contextSources, action) {
			subjectAndResultWithProofsTests = new List<Group<string, Action<TSubject, TResult, TProofSource>>>();
			subjectWithProofsTests = new List<Group<string, Action<TSubject, TProofSource>>>();
			specificExceptionTests = new List<Group<string, Func<TProofType, Exception>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}
		#endregion

		#region THENs
		public Behaviour<TSubject, TResult, TProofType, TProofSource> Then (string description, Action<TSubject, TResult, TProofSource> subjectAndResultWithProofsTest) {
			subjectAndResultWithProofsTests.Add(new Group<string, Action<TSubject, TResult, TProofSource>>(description, subjectAndResultWithProofsTest));
			return this;
		}

		new public SmartAssertionBase<TSubject, TResult, TProofType, TProofSource> Then (string description) {
			return new SmartAssertionBase<TSubject, TResult, TProofType, TProofSource>(description, this);
		}
		#endregion

		#region Exception cases
		public override ITakeMessage ShouldThrow<TException> () {
			expectedExceptionType = typeof(TException);
			subjectWithProofsTests.Insert(0, new Group<string, Action<TSubject, TProofSource>>(
				"should throw " + expectedExceptionType.Name, (s, e) => { }));
			return this;
		}

		Behaviour ITakeMessage.WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectWithProofsTests.Insert(0, new Group<string, Action<TSubject, TProofSource>>(
				"should have exception message \"" + expectedExceptionMessage + "\"", (s, e) => { }));
			return this;
		}

		Behaviour ITakeMessage.IgnoreMessage () {
			return this;
		}

		internal Behaviour<TSubject, TResult, TProofType, TProofSource> Then(string description, Func<TProofType, Exception> sampleExceptionSource) {
			specificExceptionTests
				.Add(new Group<string, Func<TProofType, Exception>>(
				     	description,
				     	sampleExceptionSource
				     	));
			return this;
		}
		
		public Behaviour<TSubject, TResult, TProofType, TProofSource> ShouldThrowException(Func<TProofType, Exception> sampleExceptionSource) {
			specificExceptionTests
				.Add(new Group<string, Func<TProofType, Exception>>(
				     	"should throw exception",
				     	sampleExceptionSource
				     	));
			return this;
		}
		#endregion

		#region Test closure building. Don't bother looking until you've comprehended the simpler versions without proofs.
		/// <summary>
		/// Build a subject for each combination of context and proof.
		/// </summary>
		private IEnumerable<Group<Func<Context<TSubject>>, IProvide<TProofType>>> BuildSubjectsWithProofs () {
			var proofData = new TProofSource().Data();
			foreach (var contextSource in contextSources) {
				var context = contextSource();
				if (context is IUse<TProofType>) {
					for (int proofIndex = 0; proofIndex < proofData.Length; proofIndex++) {
						var proof = proofData[proofIndex];
						int index = proofIndex;
						Func<Context<TSubject>> source = contextSource;
						yield return new Group<Func<Context<TSubject>>, IProvide<TProofType>>(
							() => {
								var values = new TProofSource().Data()[index];
								var fresh_context = source();
								((IUse<TProofType>)fresh_context).Values = values;
								return fresh_context;
							},
							proof as IProvide<TProofType>);
					}
				} else {
					throw new InvalidOperationException("Expected " + context.GetType().FullName + " to implement IUse<" + typeof (TProofType).Name + ">, but could not cast it.");
				}
			}
			yield break;
		}

		IEnumerable<TestClosure> IBuildTests.BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
								  from tuple in BuildSubjectsWithProofs()
			                      select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		behaviourAction(subject, context);
			                      		test.B(subject);
			                      	}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
								  from tuple in BuildSubjectsWithProofs()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A, 
									" with " + tuple._2<TProofSource>().StringRepresentation(),
									() => {
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		test.B(subject, behaviourAction(subject, context));
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//3
			testClosures.AddRange(from test in subjectAndResultWithProofsTests
								  from tuple in BuildSubjectsWithProofs()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A, 
									" with " + tuple._2<TProofSource>().StringRepresentation(),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var values = ((IUse<TProofType>)context).Values;
			                      		var result = behaviourAction(subject, context);
										test.B(subject, result, values as TProofSource);
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			
			//4
			testClosures.AddRange(from test in subjectWithProofsTests
			                      from tuple in BuildSubjectsWithProofs()
								  select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									" with " + tuple._2<TProofSource>().StringRepresentation(),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var values = ((IUse<TProofType>)context).Values;
			                      		behaviourAction(subject, context);
										test.B(subject, values as TProofSource);
									}, () => expectedExceptionType, () => expectedExceptionMessage,
									() => GetContext(tuple).TearDown()));

			//5: Tests for exceptions matching an example exception
			testClosures.AddRange(from test in specificExceptionTests
								  from tuple in BuildSubjectsWithProofs()
			                      select new TestClosure(
									"Given " + GetLambdaBuilderDescription(tuple.A),
									"When " + Description, "Then " + test.A,
									GetWithExceptionName(tuple, test),
									() =>
									{
										var context = GetContext(tuple);
										var subject = context.SetupAndReturnContextBuilder().Build();
										var values = ((IUse<TProofType>)context).Values;
			                      		behaviourAction(subject, context);
										test.B(values as TProofSource);
			                      	},
			                      	() => GetExceptionResultType(tuple, test),
			                      	() => GetExceptionMessage(tuple, test),
									() => GetContext(tuple).TearDown()
			                      	));


			return testClosures;
		}

		private Context<TSubject> GetContext(Group<Func<Context<TSubject>>, IProvide<TProofType>> tuple) {
			return tuple._1<Func<Context<TSubject>>>().Invoke();
		}
		private Exception GetException(Group<Func<Context<TSubject>>, IProvide<TProofType>> tuple, Group<string, Func<TProofType, Exception>> test) {
			var context = tuple._1<Func<Context<TSubject>>>().Invoke();
			var values = ((IUse<TProofType>)context).Values;
			return test.B(values as TProofSource);	
		}
		private Type GetExceptionResultType (Group<Func<Context<TSubject>>, IProvide<TProofType>> tuple, Group<string, Func<TProofType, Exception>> test) {
			return GetException(tuple, test).GetType();
		}
		private string GetExceptionMessage (Group<Func<Context<TSubject>>, IProvide<TProofType>> tuple, Group<string, Func<TProofType, Exception>> test) {
			var message = GetException(tuple, test).Message;
			if (string.IsNullOrEmpty(message)) return null;
			return message;
		}
		private string GetWithExceptionName (Group<Func<Context<TSubject>>, IProvide<TProofType>> tuple, Group<string, Func<TProofType, Exception>> test) {
			var exception = GetException(tuple, test);
			if (string.IsNullOrEmpty(exception.Message)) return "With exception of type " + exception.GetType() + ", ignoring message, with " + tuple._2<TProofSource>().StringRepresentation();
			return "With exception of type " + exception.GetType() + " and message \"" + exception.Message + "\", with " + tuple._2<TProofSource>().StringRepresentation();
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