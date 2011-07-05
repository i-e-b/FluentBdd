using System;
using FluentBDD;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace FluentBddNUnitExtension {
	/// <summary>
	/// Add-in interface: NUnit uses this for installation and binding.
	/// </summary>
	[NUnitAddin(Description = "FluentBDD Feature/Behaviour test suite builder")]
	public class BehaviourBuilder : IAddin, ISuiteBuilder {
		public bool CanBuildFrom(Type type) {
			return Reflect.HasAttribute(type, typeof(BehavioursAttribute).FullName, false)
				|| Reflect.HasAttribute(type, typeof(FeatureSetAttribute).FullName, false);
		}

		public Test BuildFrom(Type type) {
			return new SuiteTestBuilder(type);
		}

		public bool Install(IExtensionHost host) {
			var builders = host.GetExtensionPoint("SuiteBuilders");
			if (builders == null) return false;

			builders.Install(this);
			return true;
		}
	}
}
