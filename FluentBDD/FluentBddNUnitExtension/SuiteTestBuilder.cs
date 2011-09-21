using System;
using System.Collections.Generic;
using FluentBDD;
using FluentBDD.TreeBuilding;
using NUnit.Core;

namespace FluentBddNUnitExtension {
	public class SuiteTestBuilder : TestSuite {
		public SuiteTestBuilder (Type fixtureType) : base(fixtureType) {
			PrepareFeatureBase(fixtureType);
			
			TestName.Name = SpecificationTreeBuilder.DescriptionFor(fixtureType);
			MapTestTreeToFixtures(SpecificationTreeBuilder.BuildFor(fixtureType), this, fixtureType);
		}

		/// <summary>
		/// Go through the Test tree, building a tree of TestFixures with Test nodes. 
		/// </summary>
		private void MapTestTreeToFixtures (TreeDictionary<string, TestClosure> source, TestSuite container, Type fixtureType) {
			foreach (var testClosure in source.Items) {
				container.Add(new ClosureTest(testClosure));
			}

			foreach (var child in source.Children) {
				var fixture = new TestFixture(fixtureType);
				fixture.TestName.Name = child.Key;
				foreach (var testClosure in child.Value.Items) {
					fixture.Add(new ClosureTest(testClosure));
				}

				foreach (var sub_child in child.Value.Children) {
					var sub_fixture = new TestFixture(fixtureType);
					sub_fixture.TestName.Name = sub_child.Key;
					MapTestTreeToFixtures(sub_child.Value, sub_fixture, fixtureType);
					fixture.Add(sub_fixture);
				}
				container.Add(fixture);
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