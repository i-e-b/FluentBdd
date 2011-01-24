using System;

namespace FluentBDD {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class FeatureAttribute : Attribute {
		public FeatureAttribute() {}

		/// <summary>
		/// Feature specification. Will be tested by NUnit.
		/// </summary>
		/// <param name="description">Description, as shown in test results</param>
		public FeatureAttribute (string description) {
			Description = description;
		}

		/// <summary>
		/// Feature specification with optional informative test. Will be tested by NUnit.
		/// </summary>
		/// <param name="description">Description, as shown in test results</param>
		/// <param name="info">Textual info (ignored)</param>
		public FeatureAttribute (string description, params string[] info) {
			Description = description;
		}

		internal string Description { get; set; }
	}
}
