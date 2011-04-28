using System;
using System.Runtime.Serialization;
using FluentBDD;
using UsageExample;

namespace CalculatorConcerns {
	[Feature("Calculator features")]
	public class CalculatorFeatures : Feature {
		public Feature creation =
			For("Calculator users")
				.To("start using a calculator")
				.Should("be able to create a calculator")
				.CoveredBy<Creation>();

		public Feature addition =
			For("Calculator users")
				.To("find the sum of a list of numbers")
				.Should("have an addition function on the calculator")
				.CoveredBy<Addition>();

		public Feature subtraction =
			For("Calculator users")
				.To("find the difference of a list of numbers")
				.Should("have a subtraction function on the calculator")
				.CoveredBy<Subtraction>();

		public Feature data_contracts =
			For("Calculator feature aggregators")
				.To("be able to serialise the state of a calculator")
				.Should("have data contract attributes set on the calculator")
				.CoveredBy<DataContracts>();
	}

	[Behaviour("Calculator Creation")]
	public class Creation : Behaviours {
		public Scenario when_creating_a_calculator =
			GivenNoSubject()
				.When("I create a calculator", with => new Calculator())
				.Then("I should have a new calculator").result.should_not_be_null;


		public Scenario when_creating_an_invalid_calculator =
			GivenNoSubject()
				.When("I create a calculator with a null math delegate", c => { new Calculator(null); })
				.Then("should not be allowed to create calculator").should_throw(new ArgumentException("A math delegate must be provided"));
	}

	[Behaviour("Addition")]
	public class Addition : common_calculator_concerns {
		// Two inputs
		public Scenario calculator_can_add_two_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("I press add once", (calculator, e) => press_add_n_times(calculator, 1))
				.Then("result should be sum of first and second").result.should_be_equal_to.proof(p => p.first_plus_second)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario cant_add_more_than_I_have_inputs_for_with_2_inputs =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("I press add twice", (calculator, e) => press_add_n_times(calculator, 2))
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// Three inputs
		public Scenario calculator_can_add_two_of_three_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press add once", (calculator, e) => press_add_n_times(calculator, 1))
				.Then("result should be sum of second and third input").result.should_be_equal_to.proof(p => p.second_plus_third)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario calculator_can_add_three_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press add twice", (c, e) => press_add_n_times(c, 2))
				.Then("result should be sum of first, second and third input").result.should_be_equal_to.proof(p => p.first_second_plus_third)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario cant_add_more_than_I_have_inputs_for_with_3_inputs =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press add three times", (calculator, e) => press_add_n_times(calculator, 3))
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");
	}

	[Behaviour("Subtraction")]
	public class Subtraction : common_calculator_concerns {
		// Two inputs
		public Scenario calculator_can_add_two_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("I press subtract once", (c, e) => press_subtract_n_times(c, 1))
				.Then("result should be difference of first and second").result.should_be_equal_to.proof(p => p.first_minus_second)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario cant_add_more_than_I_have_inputs_for_with_2_inputs =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("I press subtract twice", (c, e) => press_subtract_n_times(c, 2))
				.Then("should show error message").should_throw(new InvalidOperationException("Stack empty."));

		// Three inputs
		public Scenario calculator_can_add_two_of_three_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press subtract once", (c, e) => press_subtract_n_times(c, 1))
				.Then("result should be sum of second and third input").result.should_be_equal_to.proof(p => p.second_minus_third)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario calculator_can_add_three_numbers =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press subtract twice", (c, e) => press_subtract_n_times(c, 2))
				.Then("result should be sum of first, second and third input").result.should_be_equal_to.proof(p => p.first_minus__second_minus_third)
				.Then("readout should be same as result").subject_part(c => c.Readout()).should_be_equal_to.result;

		public Scenario cant_add_more_than_I_have_inputs_for_with_3_inputs =
			ProvedBy<values_for_calculator_taking_inputs>()
				.Given<Calculator, a_calculator_taking_three_inputs>()
				.When("I press subtract three times", (c, e) => press_subtract_n_times(c, 3))
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");
	}

	[Behaviour("Data Contracts")]
	public class DataContracts : Behaviours {
		public Scenario calculator_should_have_datacontracts =
			Given<Calculator, a_calculator>()
				.Verify()
				.ShouldHaveAttribute<DataContractAttribute>()
				.ShouldHaveAttribute<SerializableAttribute>()
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("stack", m => m.Name == "Stack");

	}

	#region Contexts and Expectations
	internal class a_calculator_taking_three_inputs : Context<Calculator>, IUse<values_for_calculator_taking_inputs> {
		public values_for_calculator_taking_inputs Values { get; set; }
		public override void SetupContext () {
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

	internal class a_calculator_taking_two_inputs : Context<Calculator>, IUse<values_for_calculator_taking_inputs> {
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

	/// <summary>
	/// Quite a complex expectation provider -- this uses a 'createFor' method to 
	/// generate each instance.
	/// </summary>
	internal class values_for_calculator_taking_inputs : IProvide<values_for_calculator_taking_inputs> {
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

		private values_for_calculator_taking_inputs createFor (int fst, int snd, int thd) {
			return new values_for_calculator_taking_inputs
			{
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
	}

	#endregion
}
