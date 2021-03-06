using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentBDD.TreeBuilding {
	public class TestClosureTree : TreeDictionary<string, TestClosure> { }

	public static class SpecificationTreeBuilder {
		/// <summary>
		/// Builds a set of TestClosureTrees for the given spec class (spec means marked as Feature or Behaviour)
		/// </summary>
		public static TestClosureTree BuildFor (Type containingType) {
			var sourceInstance = CreateInstanceOf(containingType);
			IEnumerable<FieldInfo> fields;
			if (HasAttribute(containingType, typeof(BehavioursAttribute))) {
				fields = GetFields<Behaviour>(containingType);
			} else if (HasAttribute(containingType, typeof(FeatureSetAttribute))) {
				fields = GetFields<Feature>(containingType);
			} else {
				throw new ArgumentException("Not a recognised specification");
			}
			return BuildBehaviourTree(fields, sourceInstance);
		}

		/// <summary>
		/// Reads and returns the description / title of the specification
		/// </summary>
		public static string DescriptionFor (Type fixtureType) {
			foreach (var attrib in fixtureType.GetCustomAttributes(false)) {
				if (attrib is BehavioursAttribute) return (attrib as BehavioursAttribute).Description;
				if (attrib is FeatureSetAttribute) return (attrib as FeatureSetAttribute).Description;
			}
			return "### Error: not a FluentBDD specification type ###";
		}

		#region inner workings
		private static IEnumerable<FieldInfo> GetFields<T> (Type type) {
			return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.Where(f => f.FieldType == typeof(T))
				.ToArray();
		}

		private static bool HasAttribute (Type type, Type attrib) {
			return type.GetCustomAttributes(false).Any(a => a.GetType() == attrib);
		}

		private static object CreateInstanceOf (Type type) {
			return type.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		private static TestClosureTree BuildBehaviourTree (IEnumerable<FieldInfo> behaviourFields, object featureInstance) {
			var tree = new TestClosureTree();
			foreach (var behaviourField in behaviourFields) {
				var tests = GetTestClosures(behaviourField, featureInstance);
				foreach (var test in tests) {
					var given = FixName(test.Given);

					if (string.IsNullOrEmpty(test.With)) tree[given][test.When].AddItem(test);
					else tree[given][test.When][test.Then].AddItem(test);
				}
			}
			return tree;
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
