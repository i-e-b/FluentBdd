using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace FluentBDD {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class FeatureAttribute : Attribute {
		public FeatureAttribute () { }

		/// <summary>
		/// Feature specification. Will be tested by NUnit.
		/// </summary>
		/// <param name="description">Description, as shown in test results</param>
		public FeatureAttribute (string description) {
			Description = description;
		}

		internal string Description { get; set; }
	}

	public class Feature: IBuildTests, IBehaviourSpec {
		internal string Stakeholders;
		internal string Goal;
		internal string Solution;
		internal bool CoverageIncomplete;
		internal readonly IList<Type> CoveringTypes;

		#region Inner workings
		public Feature() {
			CoveringTypes = new List<Type>();
			CoverageIncomplete = false;
		}

		private Feature(string stakeholders) {
			Stakeholders = "For " + stakeholders;
			CoveringTypes = new List<Type>();
			CoverageIncomplete = false;
		}

		IEnumerable<TestClosure> IBuildTests.BuildTests () {
			return new []{new TestClosure(Stakeholders, Goal, Solution, TestBehaviour, () => { })};
		}

		private void TestBehaviour() {
			if (CoverageIncomplete) Assert.Ignore("Behaviour marked as incomplete");
			if (CoveringTypes == null || CoveringTypes.Count < 1) throw new InconclusiveException("No coverages defined");

			var isOk = true;
			foreach (var coveringType in CoveringTypes) {
				var featureAttribCount = coveringType.GetCustomAttributes(typeof (BehaviourAttribute), false).Length;
				
				if (featureAttribCount == 1) {
					Console.WriteLine(coveringType.FullName + " is a behaviour set [OK]");
				} else {
					isOk = false;
					Console.WriteLine(coveringType.FullName + " is not a behaviour set [FAIL]");
				}
			}
			Assert.AreEqual(true, isOk);
		}
		#endregion

		public static IBehaviourSpec For(params string[] stakeholders) {
			return new Feature(stakeholders.Aggregate((a, b) => a + ", " + b));
		}

		public IBehaviourSpec To(string goal) {
			if (string.IsNullOrEmpty(Goal)) Goal = "To " + goal;
			else Goal += "; and to " + goal;
			return this;
		}

		public IBehaviourSpec Should (string solution) {
			if (string.IsNullOrEmpty(Solution)) Solution = "Should " + solution;
			else Solution += "; and should " + solution;
			return this;
		}

		public Feature CoveredBy<TCoveringType>() {
			CoveringTypes.Add(typeof(TCoveringType));
			return this;
		}

		public Feature CoveredBy<TCoveringType> (params Expression<Func<TCoveringType, Scenario>>[] explicitScenarios) {
			CoveringTypes.Add(typeof(TCoveringType));
			return this;
		}

		public Feature CoverageNotComplete () {
			CoverageIncomplete = true;
			return this;
		}
	}

	public interface IBehaviourSpec {
		IBehaviourSpec To (string goal);
		IBehaviourSpec Should (string solution);
		Feature CoveredBy<TCoveringType> ();
		Feature CoveredBy<TCoveringType> (params Expression<Func<TCoveringType, Scenario>>[] explicitScenarios);
		Feature CoverageNotComplete ();
	}

}