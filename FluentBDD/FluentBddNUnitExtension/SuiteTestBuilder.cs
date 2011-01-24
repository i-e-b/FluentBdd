using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentBDD;
using NUnit.Core;

namespace FluentBddNUnitExtension {
	public class SuiteTestBuilder : TestSuite {
		public const string CastingError = "Error: failed to cast to Scenario type. Make sure you NUnit plugins and BddLibrary are the same version!";
		private readonly Map<string, ListMap<string, ClosureTest>> GivenCases;

		public SuiteTestBuilder(Type fixtureType) : base(fixtureType) {
			GivenCases = new Map<string, ListMap<string, ClosureTest>>(); // GIVEN => WHEN, {THEN}
			PrepareFeatureBase(fixtureType);


			var featureInstance = Feature.CreateFor(fixtureType);
			var scenarioFields = GetScenarioFields(fixtureType);

			foreach (var scenarioField in scenarioFields) {
				var name = GetContextName(scenarioField, featureInstance);
				if (string.IsNullOrEmpty(name) || name == "no subject" || name == "Given no subject") {
					AddTestsWithoutSubjectGrouping(scenarioField, featureInstance);
				} else {
					PrepareTestsWithSubjectGrouping(name, scenarioField, featureInstance);
				}
			}

			foreach (var given in GivenCases.Keys) {
				var scenario = new TestFixture(fixtureType);
				scenario.TestName.Name = given;
				foreach (var when in GivenCases[given].Keys) {
					var when_suite = new TestFixture(fixtureType);
					when_suite.TestName.Name = when;
					foreach (var test in GivenCases[given][when]) {
						when_suite.Add(test);
					}
					scenario.Add(when_suite);
				}
				Add(scenario);
			}
		}
		
		private void PrepareTestsWithSubjectGrouping(string scenarioName, FieldInfo scenarioField, object featureInstance) {
			var given = GivenCases[scenarioName];

			var tests = GetTestClosures(scenarioField, featureInstance);
			foreach (var test in tests) {
				given[test.Cause].Add(new ClosureTest(test));
			}
		}

		private void AddTestsWithoutSubjectGrouping(FieldInfo scenarioField, object featureInstance) {
			var tests = GetTestClosures(scenarioField, featureInstance);
			foreach (var test in tests) {
				test.Effect = test.Cause + ", then " + test.Effect;
				Add(new ClosureTest(test));
			}
		}
		

		private IEnumerable<TestClosure> GetTestClosures (FieldInfo scenarioField, object featureInstance) {
			var scenario = scenarioField.GetValue(featureInstance) as Scenario;
			return (scenario == null) ? (ErrorTestClosure()) : (scenario.TestClosures);
		}

		private IEnumerable<TestClosure> ErrorTestClosure() {
			return new[] { new TestClosure { Effect = CastingError } };
		}

		private string GetContextName(FieldInfo scenarioField, object featureInstance) {
			var scenario = scenarioField.GetValue(featureInstance) as Scenario;
			return (scenario != null) 
				? (scenario.ContextName)
				: (CastingError);
		}

		private IEnumerable<FieldInfo> GetScenarioFields(Type fixtureType) {
			return fixtureType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(f => f.FieldType == typeof (Scenario))
				.ToArray();
		}

		private void PrepareFeatureBase(Type fixtureType) {
			Fixture = Reflect.Construct(fixtureType);

			foreach (var attrib in fixtureType.GetCustomAttributes(false)) {
				if (attrib is FeatureAttribute) {
					TestName.Name = (attrib as FeatureAttribute).Description;
				}
			}
		}
	}
}