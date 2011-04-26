using System;
using FluentBDD.Assertions;
using NUnit.Framework;

namespace FluentBDD {
	/// <summary>
	/// Hooks into Scenario.Then to give access to the SmartAssertions
	/// </summary>
	public static class SmartAssertionExtensions {
		public static SmartAssertionBase<S, R, Pt, Ps> Then<S, R, Pt, Ps> (this Scenario<S, R, Pt, Ps> scen, string description) where Ps : class, Pt, IProvide<Pt>, new() {
			return new SmartAssertionBase<S, R, Pt, Ps>(description, scen);
		}
		public static SmartAssertionBase<S, R> Then<S, R> (this Scenario<S, R> scen, string description) {
			return new SmartAssertionBase<S, R>(description, scen);
		}
	}

	/// <summary>
	/// Ignore helper for "When" cases
	/// </summary>
	public static class Ignore {
		public static void me (object a, object b) {
			Assert.Ignore("Ignored");
		}

		public static void me (object a) {
			Assert.Ignore("Ignored");
		}
	}
}