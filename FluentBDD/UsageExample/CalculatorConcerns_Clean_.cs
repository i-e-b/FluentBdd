using System;
using System.Runtime.Serialization;
using FluentBDD;
using FluentBDD.Assertions;
using UsageExample;

// This is a clean version of "CalculatorConcerns", showing real-world usage.
// Your own specifications should look more like this!
namespace CalculatorConcerns {
	[Behaviour("Creation")]
	public class Creation : Behaviours {
		public Scenario when_creating_a_calculator =
			GivenNoSubject()
				.When("I create a calculator", with => new Calculator())
				.Then("I should have a new calculator", (no_subject, calculator) => calculator.should_not_be_null());


		public Scenario when_creating_an_invalid_calculator =
			GivenNoSubject()
				.When("I create a calculator with a null math delegate", c => { new Calculator(null); })
				.ShouldThrow<ArgumentException>()
				.WithMessage("An math delegate must be provided");
	}

	[Behaviour("Addition",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the sum of numbers")]
	public class Addition : common_calculator_concerns {
		// Two inputs
		public Scenario calculator_can_add_two_numbers =
			Given(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press add once", c => press_add_n_times(c, 1))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be sum of first and second", (s, r, v) => r.should_be_equal_to(v.first_plus_second))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario cant_add_more_than_I_have_inputs_for_with_2_inputs =
			Given(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press add twice", c => press_add_n_times(c, 2))
				.Using<values_for_calculator_taking_inputs>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// Three inputs
		public Scenario calculator_can_add_two_of_three_numbers =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press add once", c => press_add_n_times(c, 1))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be sum of second and third input", (s, r, v) => r.should_be_equal_to(v.second_plus_third))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario calculator_can_add_three_numbers =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press add twice", c => press_add_n_times(c, 2))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be sum of first, second and third input", (s, r, v) => r.should_be_equal_to(v.first_second_plus_third))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario cant_add_more_than_I_have_inputs_for_with_3_inputs =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press add three times", c => press_add_n_times(c, 3))
				.Using<values_for_calculator_taking_inputs>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");
				

	}

	[Behaviour("Subtraction",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the difference between two numbers")]
	public class Subtraction : common_calculator_concerns {
		// Two inputs
		public Scenario calculator_can_add_two_numbers =
			Given(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press subtract once", c => press_subtract_n_times(c, 1))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be difference of first and second", (s, r, v) => r.should_be_equal_to(v.first_minus_second))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario cant_add_more_than_I_have_inputs_for_with_2_inputs =
			Given(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press subtract twice", c => press_subtract_n_times(c, 2))
				.Using<values_for_calculator_taking_inputs>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// Three inputs
		public Scenario calculator_can_add_two_of_three_numbers =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press subtract once", c => press_subtract_n_times(c, 1))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be sum of second and third input", (s, r, v) => r.should_be_equal_to(v.second_minus_third))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario calculator_can_add_three_numbers =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press subtract twice", c => press_subtract_n_times(c, 2))
				.Using<values_for_calculator_taking_inputs>()
				.Then("result should be sum of first, second and third input", (s, r, v) => r.should_be_equal_to(v.first_minus__second_minus_third))
				.Then("readout should be same as result", calculator_readout_should_match_result);

		public Scenario cant_add_more_than_I_have_inputs_for_with_3_inputs =
			Given(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("I press subtract three times", c => press_subtract_n_times(c, 3))
				.Using<values_for_calculator_taking_inputs>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");
	}


	[Behaviour("Data Contracts",
		"As calculator service provider",
		"To make loads of money, I want to be able to serialise my calculator via SOAP",
		"For this to work, I need attributes on the calculator and it's fields")]
	public class DataContracts : Behaviours {
		public Scenario calculator_should_have_datacontracts =
			Given(() => Context.Of<a_calculator>())
				.Verify()
				.ShouldHaveAttribute<DataContractAttribute>()
				.ShouldHaveAttribute<SerializableAttribute>()
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("stack", m => m.Name == "Stack");
				
	}

	#region Contexts and Expectations
	public class values_for_calculator_taking_inputs : IProvide<values_for_calculator_taking_inputs> {
		public int first, second, third;

		public int first_plus_second;
		public int second_plus_third;
		public int first_second_plus_third;
		public int first_minus_second;
		public int second_minus_third;
		public int first_minus__second_minus_third;

		public values_for_calculator_taking_inputs[] Data () {
			return new[] {
				createFor(10, 20, 30), createFor(-10, 20, 0), createFor(-5, -5, -5),
				createFor(0, 0, 0), createFor(5, -5, 5), createFor(10000, 20000, 30000),
				createFor(-10000, -20000, 30000)
			};
		}

		public string StringRepresentation () {
			return "{ " + first + ", " + second + ", " + third + " }";
		}

		private values_for_calculator_taking_inputs createFor(int fst, int snd, int thd) {
			return new values_for_calculator_taking_inputs {
				first = fst,
				second = snd,
				third = thd,
				first_plus_second = fst + snd,
				first_second_plus_third = fst + snd + thd,
				second_plus_third = snd + thd,
				first_minus_second = fst - snd,
				second_minus_third = snd - thd,
				first_minus__second_minus_third = fst - (snd - thd)
			};
		}
	}

	/// <summary> A few methods that help keep scenarios clean </summary>
	public class common_calculator_concerns : Behaviours {
		protected static int press_add_n_times (Calculator calculator, int n) {
			if (n < 1) throw new ArgumentException("n must be gte 1");
			for (int i = 0; i < n - 1; i++) {
				calculator.Add();
			}
			return calculator.Add();
		}

		protected static int press_subtract_n_times (Calculator calculator, int n) {
			if (n < 1) throw new ArgumentException("n must be gte 1");
			for (int i = 0; i < n - 1; i++) {
				calculator.Subtract();
			}
			return calculator.Subtract();
		}

		protected static void calculator_readout_should_match_result (Calculator subject, int result) {
			subject.Readout().should_be_equal_to(result);
		}
		
	}

	public class a_calculator_taking_three_inputs: Context<Calculator>, IUse<values_for_calculator_taking_inputs> {
		public values_for_calculator_taking_inputs Values { get; set; }
		public override void SetupContext() {
			Given("a calculator", () => new Calculator())
				.And("I type in the first, second and third values",
					 c =>
					 {
						 c.Press(Values.first);
						 c.Press(Values.second);
						 c.Press(Values.third);
					 });
		}
	}
	public class a_calculator_taking_two_inputs : Context<Calculator>, IUse<values_for_calculator_taking_inputs> {
		public values_for_calculator_taking_inputs Values { get; set; }
		public override void SetupContext () {
			Given("a calculator", () => new Calculator())
				.And("I type in the first and second values",
					 c =>
					 {
						 c.Press(Values.first);
						 c.Press(Values.second);
					 });
		}
	}

	internal class a_calculator : Context<Calculator> {
		public override void SetupContext () {
			Given("I have a calculator", () => new Calculator());
		}
	}
	#endregion
}
