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
				var tests = GetTestClosures(scenarioField, featureInstance);
				foreach (var test in tests) {
					GivenCases[FixName(test.Given)][test.When].Add(new ClosureTest(test));
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

		private string FixName(string givenName) {
			if (givenName == "Error") return "### ERROR IN TEST SETUP ###";
			return "Given " + givenName;
		}


		private IEnumerable<TestClosure> GetTestClosures (FieldInfo scenarioField, object featureInstance) {
			Scenario scenario;

			try {
				scenario = (Scenario)scenarioField.GetValue(featureInstance);
				return scenario.BuildTests();
			} catch( Exception ex) {
				return new[] { new TestClosure("Error", "Field name = " + scenarioField.Name, ex.Message+"\r\n \r\n"+ex.StackTrace, () => { throw ex; }) };
			}
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