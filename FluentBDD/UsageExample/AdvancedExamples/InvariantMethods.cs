using FluentBDD;

namespace Advanced.UsageExample {
	[Behaviours("Invariant method testing",
		"When testing invariant functions (such as static calculations)",
		"we can test a set of inputs and outputs as a sanity test of our logic.")]
	public class InvariantMethods : Behaviours {

		public Behaviour complex_calculation_should_work =
			GivenStaticContextFor<values_for_complex_calculation>()
				.Using<values_for_complex_calculation>()
				.When("I calculate the value with a known input", (no_subject, context) => StaticMethods.Calculate(context.Values.input))
				.Then("I should get the matching output").result.should_be_equal_to.proof(p => p.result);

		public Behaviour same_using_interfaces_for_expectations =
			GivenStaticContextFor<ISimpleInOutExpectations>()
				.Using<ISimpleInOutExpectations, values_for_complex_calculation_using_interface>()
				.When("I calculate the value with a known input", (no_subject, context) => StaticMethods.Calculate(context.Values.input))
				.Then("I should get the matching output (using interface)").result.should_be_equal_to.proof(p => p.result);

	}

	public static class StaticMethods {
		public static int Calculate (int input) {
			return ((input * input) + input) / 4;
		}
	}

	public interface ISimpleInOutExpectations {
		int result { get; set; }
		int input { get; set; }
	}

	public class values_for_complex_calculation_using_interface : IProvide<ISimpleInOutExpectations>, ISimpleInOutExpectations {
		public int result { get; set; }
		public int input { get; set; }

		private static readonly values_for_complex_calculation_using_interface[] values = new[] {
			new values_for_complex_calculation_using_interface(0, 0),
			new values_for_complex_calculation_using_interface(3, 3),
			new values_for_complex_calculation_using_interface(1000, 250250),
			new values_for_complex_calculation_using_interface(58,855),
			new values_for_complex_calculation_using_interface(6482, 10505701)
		};

		public values_for_complex_calculation_using_interface () { }
		private values_for_complex_calculation_using_interface (int inp, int res) {
			input = inp;
			result = res;
		}
		public ISimpleInOutExpectations[] Data () { return values; }
		public string StringRepresentation () { return input + " => " + result; }
	}

	public class values_for_complex_calculation : IProvide<values_for_complex_calculation> {
		public int result { get; set; }
		public int input { get; set; }

		private static readonly values_for_complex_calculation[] values = new[] {
			new values_for_complex_calculation(0, 0),
			new values_for_complex_calculation(3, 3),
			new values_for_complex_calculation(1000, 250250),
			new values_for_complex_calculation(58,855),
			new values_for_complex_calculation(6482, 10505701)
		};

		public values_for_complex_calculation () { }
		private values_for_complex_calculation (int inp, int res) {
			input = inp;
			result = res;
		}
		public values_for_complex_calculation[] Data () { return values; }
		public string StringRepresentation () { return input + " => " + result; }
	}
}
