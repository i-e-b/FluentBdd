using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentBDD;
using FluentBDD.TreeBuilding;
using NUnit.Core;

namespace FluentBddNUnitExtension {
	public class SuiteTestBuilder : TestSuite {
		public const string CastingError = "Error: failed to cast to Scenario type. Make sure you NUnit plugins and BddLibrary are the same version!";
		
		public SuiteTestBuilder (Type fixtureType)
			: base(fixtureType) {
			PrepareFeatureBase(fixtureType);
			
			TestName.Name = Builder.GetSpecificationDescription(fixtureType);
			var givenCases = Builder.BuildSpecificationTree(fixtureType);
			
			BuildTestTreeFromClosureMap(givenCases, fixtureType);
		}

		private void BuildTestTreeFromClosureMap (TestClosureTree GivenCases, Type fixtureType) {
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
							var closureTest = new ClosureTest(test);
							if (closureTest.EmptyWith()) {
								closureTest.UseCompressedName();
								when_suite.Add(closureTest);
							} else {
								deeps++;
								then_suite.Add(closureTest);
							}
						}
						if (deeps > 0) when_suite.Add(then_suite);
					}
					scenario.Add(when_suite);
				}
				Add(scenario);
			}
		}

		private void PrepareFeatureBase (Type fixtureType) {
			try {
				this.Fixture = Reflect.Construct(fixtureType);
			} catch {
				throw new Exception("Failed to construct " + fixtureType.FullName);
			}
		}
	}
}