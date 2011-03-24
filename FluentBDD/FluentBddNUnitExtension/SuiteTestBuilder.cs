using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentBDD;
using NUnit.Core;

namespace FluentBddNUnitExtension {
	public class SuiteTestBuilder : TestSuite {
		public const string CastingError = "Error: failed to cast to Scenario type. Make sure you NUnit plugins and BddLibrary are the same version!";
		private readonly Map<string, Map<string, ListMap<string, ClosureTest>>> GivenCases;

		public SuiteTestBuilder (Type fixtureType)
			: base(fixtureType) {
			GivenCases = new Map<string, Map<string, ListMap<string, ClosureTest>>>(); // GIVEN => (WHEN => (THEN, {WITH}))
			PrepareFeatureBase(fixtureType);

			var sourceInstance = CreateInstanceOf(fixtureType);
			if (Reflect.HasAttribute(fixtureType, typeof(BehaviourAttribute).FullName, false)) {
				var scenarioFields = GetFields<Scenario>(fixtureType);

				BuildAndMapScenarioTestClosures(scenarioFields, sourceInstance);
			} else if (Reflect.HasAttribute(fixtureType, typeof(FeatureAttribute).FullName, false)) {
				var behaviourFields = GetFields<Feature>(fixtureType);

				BuildAndMapScenarioTestClosures(behaviourFields, sourceInstance);
			}
			
			BuildTestTreeFromClosureMap(fixtureType);
		}



		private static object CreateInstanceOf (Type type) {
			return type.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		private void BuildTestTreeFromClosureMap (Type fixtureType) {
			foreach (var given in GivenCases.Keys) {
				var scenario = new TestFixture(fixtureType);
				scenario.TestName.Name = given;
				foreach (var when in GivenCases[given].Keys) {
					var when_suite = new TestFixture(fixtureType);
					when_suite.TestName.Name = when;
					foreach (var then in GivenCases[given][when].Keys) {
						int deeps = 0;
						var then_suite = new TestFixture(fixtureType);
						then_suite.TestName.Name = then;
						foreach (var test in GivenCases[given][when][then]) {
							if (test.EmptyWith()) {
								test.UseCompressedName();
								when_suite.Add(test);
							} else {
								deeps++;
								then_suite.Add(test);
							}
						}
						if (deeps > 0) when_suite.Add(then_suite);
					}
					scenario.Add(when_suite);
				}
				Add(scenario);
			}
		}

		private void BuildAndMapScenarioTestClosures (IEnumerable<FieldInfo> scenarioFields, object featureInstance) {
			foreach (var scenarioField in scenarioFields) {
				var tests = GetTestClosures(scenarioField, featureInstance);
				foreach (var test in tests) {
					GivenCases[FixName(test.Given)][test.When][test.Then].Add(new ClosureTest(test));
				}
			}
		}

		private string FixName (string givenName) {
			if (givenName == "Error" || givenName == "Given Error") return "### ERROR IN TEST SETUP ###";
			return givenName;
		}

		private IEnumerable<TestClosure> GetTestClosures (FieldInfo sourceField, object featureInstance) {
			IBuildTests builder;

			try {
				builder = (IBuildTests)sourceField.GetValue(featureInstance);
				return builder.BuildTests();
			} catch (Exception ex) {
				return new[] { new TestClosure("Error", "Field name = " + sourceField.Name, ex.Message + "\r\n \r\n" + ex.StackTrace, () => { throw ex; }, () => { }) };
			}
		}

		private IEnumerable<FieldInfo> GetFields<T> (Type fixtureType) {
			return fixtureType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(f => f.FieldType == typeof(T))
				.ToArray();
		}

		private void PrepareFeatureBase (Type fixtureType) {
			try {
				Fixture = Reflect.Construct(fixtureType);
			} catch {
				throw new Exception("Failed to construct " + fixtureType.FullName);
			}

			foreach (var attrib in fixtureType.GetCustomAttributes(false)) {
				if (attrib is BehaviourAttribute) {
					TestName.Name = (attrib as BehaviourAttribute).Description;
				} else if (attrib is FeatureAttribute) {
					TestName.Name = (attrib as FeatureAttribute).Description;
				}
			}
		}
	}
}