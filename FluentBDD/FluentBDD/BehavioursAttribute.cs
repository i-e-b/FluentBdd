﻿using System;

namespace FluentBDD {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class BehavioursAttribute : Attribute {
		public BehavioursAttribute() {}

		/// <summary>
		/// Feature specification. Will be tested by NUnit.
		/// </summary>
		/// <param name="description">Description, as shown in test results</param>
		public BehavioursAttribute (string description) {
			Description = description;
		}

		/// <summary>
		/// Feature specification with optional informative test. Will be tested by NUnit.
		/// </summary>
		/// <param name="description">Description, as shown in test results</param>
		/// <param name="info">Textual info (ignored)</param>
		public BehavioursAttribute (string description, params string[] info) {
			Description = description;
			Info = info;
		}

		internal string Description { get; set; }
		internal string[] Info { get; set; }
	}
}
