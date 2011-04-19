using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentBDD;

namespace FluentBDD.TreeBuilding
{
	// TODO: replace this weird thing with a pure tree structure (tests at leafs only)
	public class TestClosureTree: Map<string, Map<string, ListMap<string, TestClosure>>>
	{
		public TestClosureTree ():base(){}
	}
	
	public static class Builder
	{
		/// <summary>
		/// Builds a set of TestClosureTrees for the given spec class (spec means marked as Feature or Behaviour)
		/// </summary>
		public static TestClosureTree BuildSpecificationTree (Type containingType) {
			var givenCases = new TestClosureTree(); // GIVEN => (WHEN => (THEN, {WITH}))

			var sourceInstance = CreateInstanceOf(containingType);
			if (HasAttribute(containingType, typeof(BehaviourAttribute))) {
				var scenarioFields = GetFields<Scenario>(containingType);

				BuildAndMapScenarioTestClosures(givenCases, scenarioFields, sourceInstance);
			} else if (HasAttribute(containingType, typeof(FeatureAttribute))) {
				var behaviourFields = GetFields<Feature>(containingType);

				BuildAndMapScenarioTestClosures(givenCases, behaviourFields, sourceInstance);
			}
			
			return givenCases;
		}
		
		/// <summary>
		/// Reads and returns the description / title of the specification
		/// </summary>
		public static string GetSpecificationDescription (Type fixtureType) {
			foreach (var attrib in fixtureType.GetCustomAttributes(false)) {
				if (attrib is BehaviourAttribute) {
					return (attrib as BehaviourAttribute).Description;
				} else if (attrib is FeatureAttribute) {
					return (attrib as FeatureAttribute).Description;
				}
			}
			return "### Error: not a FluentBDD specification type ###";	
		}
		
		#region inner workings
		private static IEnumerable<FieldInfo> GetFields<T> (Type type) {
			return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(f => f.FieldType == typeof(T))
				.ToArray();
		}
		
		private static bool HasAttribute(Type type, Type attrib) {
			return type.GetCustomAttributes(false).Any(a=> a.GetType() == attrib);
		}
		
		private static object CreateInstanceOf (Type type) {
			return type.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}
		
		private static void BuildAndMapScenarioTestClosures (TestClosureTree tree, IEnumerable<FieldInfo> scenarioFields, object featureInstance) {
			foreach (var scenarioField in scenarioFields) {
				var tests = GetTestClosures(scenarioField, featureInstance);
				foreach (var test in tests) {
					tree[FixName(test.Given)][test.When][test.Then].Add(test);
				}
			}
		}
		
		private static string FixName (string givenName) {
			if (givenName == "Error" || givenName == "Given Error") return "### ERROR IN TEST SETUP ###";
			return givenName;
		}
		
		private static IEnumerable<TestClosure> GetTestClosures (FieldInfo sourceField, object featureInstance) {
			IBuildTests builder;

			try {
				builder = (IBuildTests)sourceField.GetValue(featureInstance);
				return builder.BuildTests();
			} catch (Exception ex) {
				return new[] { new TestClosure("Error", "Field name = " + sourceField.Name, ex.Message + "\r\n \r\n" + ex.StackTrace, () => { throw ex; }, () => { }) };
			}
		}
		#endregion
	}
}
