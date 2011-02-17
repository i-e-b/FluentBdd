﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FluentBDD {
	[EditorBrowsable(EditorBrowsableState.Always)]
	public abstract class Scenario {
		internal abstract IEnumerable<TestClosure> BuildTests ();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Scenario<TSubject, TResult> : Scenario, ITakeMessage {
		protected readonly string Description;
		protected readonly IEnumerable<Func<Context<TSubject>>> contextSources;
		protected readonly Func<TSubject, Context<TSubject>, TResult> scenarioAction;

		internal readonly List<Group<string, Action<TSubject>>> subjectOnlyTests;
		internal readonly List<Group<string, Action<TSubject, TResult>>> subjectAndResultTests;

		protected Type expectedExceptionType;
		protected string expectedExceptionMessage;

		#region CTORs
		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				action(subject);
				return default(TResult);
			};
		}

		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject, Context<TSubject>> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				action(subject, context);
				return default(TResult);
			};
		}

		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, TResult> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				return action(subject);
			};
		}

		public Scenario (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, Context<TSubject>, TResult> action) {
			subjectOnlyTests = new List<Group<string, Action<TSubject>>>();
			subjectAndResultTests = new List<Group<string, Action<TSubject, TResult>>>();

			Description = description;
			this.contextSources = contextSources;
			scenarioAction = (subject, context) =>
			{
				return action(subject, context);
			};
		}
		#endregion

		#region USING for IUse/IProvide
		public ScenarioWithExamples<TSubject, TResult, TExampleSource, TExampleSource> 
			Using<TExampleSource> ()
			
			where TExampleSource : class, IProvide<TExampleSource>, new() {

			return new ScenarioWithExamples<TSubject, TResult, TExampleSource, TExampleSource>(Description, contextSources, scenarioAction);
		}

		public ScenarioWithExamples<TSubject, TResult, TExampleType, TExampleSource>
			Using<TExampleType, TExampleSource> ()
			where TExampleSource : class, TExampleType, IProvide<TExampleType>, new() {
			return new ScenarioWithExamples<TSubject, TResult, TExampleType, TExampleSource>(Description, contextSources, scenarioAction);
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
			subjectOnlyTests.Insert(0, new Group<string, Action<TSubject>>(
				"Should throw " + expectedExceptionType.Name, subject => { }));
			return this;
		}

		Scenario ITakeMessage.WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectOnlyTests.Insert(0, new Group<string, Action<TSubject>>(
				"Should have exception message \"" + expectedExceptionMessage+"\"", subject => { }));
			return this;
		}

		Scenario ITakeMessage.IgnoreMessage () {
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

		internal override IEnumerable<TestClosure> BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
			                      from contextSource in BuildSubjects()
			                      select new TestClosure(
			                      	contextSource().SetupAndReturnContextBuilder().Description,
			                      	"When " + Description, test.A,
			                      	() => {
			                      		var context = contextSource();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		scenarioAction(subject, context);
			                      		test.B(subject);
									}, expectedExceptionType, expectedExceptionMessage));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
								  from contextSource in BuildSubjects()
			                      select new TestClosure(
									contextSource().SetupAndReturnContextBuilder().Description,
			                      	"When " + Description, test.A,
									() => {
										var context = contextSource();
										var subject = context.SetupAndReturnContextBuilder().Build();
										test.B(subject, scenarioAction(subject, context));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ITakeMessage {
		Scenario WithMessage (string expectedMessage);
		Scenario IgnoreMessage();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ScenarioWithExamples<TSubject, TResult, TExampleType, TExampleSource> : Scenario<TSubject, TResult>, ITakeMessage where TExampleSource : class, TExampleType, IProvide<TExampleType>, new() {

		internal readonly List<Group<string, Action<TSubject, TResult, TExampleSource>>> subjectAndResultAndExampleTests;
		internal readonly List<Group<string, Action<TSubject, TExampleSource>>> subjectAndExampleTests;

		#region CTORs
		public ScenarioWithExamples (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Action<TSubject, Context<TSubject>> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExampleSource>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExampleSource>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}

		public ScenarioWithExamples (string description, IEnumerable<Func<Context<TSubject>>> contextSources, Func<TSubject, Context<TSubject>, TResult> action)
			: base(description, contextSources, action) {
			subjectAndResultAndExampleTests = new List<Group<string, Action<TSubject, TResult, TExampleSource>>>();
			subjectAndExampleTests = new List<Group<string, Action<TSubject, TExampleSource>>>();

			expectedExceptionType = null;
			expectedExceptionMessage = null;
		}
		#endregion

		#region THENs
		public ScenarioWithExamples<TSubject, TResult, TExampleType, TExampleSource> Then (string description, Action<TSubject, TResult, TExampleSource> subjectAndResultAndExampleTest) {
			subjectAndResultAndExampleTests.Add(new Group<string, Action<TSubject, TResult, TExampleSource>>(description, subjectAndResultAndExampleTest));
			return this;
		}

		public ScenarioWithExamples<TSubject, TResult, TExampleType, TExampleSource> Then (string description, Action<TSubject, TExampleSource> subjectAndExampleTest) {
			subjectAndExampleTests.Add(new Group<string, Action<TSubject, TExampleSource>>(description, subjectAndExampleTest));
			return this;
		}
		#endregion

		#region Exception cases
		public override ITakeMessage ShouldThrow<TException> () {
			expectedExceptionType = typeof(TException);
			subjectAndExampleTests.Insert(0, new Group<string, Action<TSubject, TExampleSource>>(
				"Should throw " + expectedExceptionType.Name, (s, e) => { }));
			return this;
		}

		Scenario ITakeMessage.WithMessage (string expectedMessage) {
			expectedExceptionMessage = expectedMessage;
			subjectAndExampleTests.Insert(0, new Group<string, Action<TSubject, TExampleSource>>(
				"Should have exception message \"" + expectedExceptionMessage + "\"", (s, e) => { }));
			return this;
		}

		Scenario ITakeMessage.IgnoreMessage () {
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
					var source = contextSource;
					yield return new Group<Func<Context<TSubject>>, IProvide<TExampleType>>(
						source,
						null);
				}
			}
			yield break;
		}

		internal override IEnumerable<TestClosure> BuildTests () {
			var testClosures = new List<TestClosure>();

			//1
			testClosures.AddRange(from test in subjectOnlyTests
			                      from tuple in BuildSubjectsWithExamples().Take(1)
			                      select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
			                      	"When " + Description, test.A,
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		scenarioAction(subject, context);
			                      		test.B(subject);
			                      	}, expectedExceptionType, expectedExceptionMessage));

			//2
			testClosures.AddRange(from test in subjectAndResultTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExampleSource>().StringRepresentation(),
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
			                      		test.B(subject, scenarioAction(subject, context));
									}, expectedExceptionType, expectedExceptionMessage));

			//3
			testClosures.AddRange(from test in subjectAndResultAndExampleTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExampleSource>().StringRepresentation(),
									() =>
									{
										var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExampleType>)context).Values;
			                      		var result = scenarioAction(subject, context);
										test.B(subject, result, example as TExampleSource);
									}, expectedExceptionType, expectedExceptionMessage));

			
			//4
			testClosures.AddRange(from test in subjectAndExampleTests
			                      from tuple in BuildSubjectsWithExamples()
								  select new TestClosure(
									GetLambdaBuilderDescription(tuple.A),
									"When " + Description, test.A, " with " + tuple._2<TExampleSource>().StringRepresentation(),
			                      	() => {
			                      		var context = tuple._1<Func<Context<TSubject>>>()();
										var subject = context.SetupAndReturnContextBuilder().Build();
										var example = ((IUse<TExampleType>)context).Values;
			                      		scenarioAction(subject, context);
										test.B(subject, example as TExampleSource);
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