using System;
using FluentBDD;
using FluentBDD.Assertions;
using UsageExample;

// This file shows a way to wrap up a complex set of behaviour behind an interface (IDoCalculatorStuff)
// defining a set of behaviours without defining contexts, actions or specific tests -- allowing these
// to be specified multiple times later (with sets different contexts/values/tests)
namespace ComplexInheritence {

	#region Template for behaviour --->
	// Note: this is a 'Feature', but does not have the feature attribute so won't be tested
	public class Pressing_buttons<TContextProvider, TSubject, TValuesProvider> : Behaviours
		where TContextProvider : IDoCalculatorStuff<TSubject, TValuesProvider>, new()
		where TValuesProvider : class, IProvide<TValuesProvider>, new() {

		#region Template bindings
		public static Context<TSubject> CalculatorTakingTwoInputs () {return (new TContextProvider()).SubjectTakingTwoInputs();}
		public static Context<TSubject> CalculatorTakingThreeInputs () {return (new TContextProvider()).SubjectTakingThreeInputs();}
		private static int press_button_n_times(TSubject subject, int i) {return (new TContextProvider()).press_button_n_times(subject, i);}
		private static void subject_readout_should_match_result(TSubject subject, int result) {(new TContextProvider()).subject_readout_should_match_result(subject, result);}
		private static void result_should_be_first_then_second(int result, TValuesProvider ValuesProvider) {(new TContextProvider()).result_should_be_first_then_second(result, ValuesProvider);}
		private static void result_should_be_second_then_third(int result, TValuesProvider ValuesProvider) {(new TContextProvider()).result_should_be_second_then_third(result, ValuesProvider);}
		private static void result_should_be_second_and_third_then_first(int result, TValuesProvider ValuesProvider) {(new TContextProvider()).result_should_be_second_and_third_then_first(result, ValuesProvider);}
		#endregion

		// Two inputs
		public Scenario calculator_can_add_two_numbers =
			Given<TSubject>(CalculatorTakingTwoInputs)
				.When("I press the button once", c => press_button_n_times(c, 1))
				.Using<TValuesProvider>()
				.Then("result should be result of first and second", (s, r, v) => result_should_be_first_then_second(r,v))
				.Then("readout should be same as result", (s,r,v) => subject_readout_should_match_result(s,r));

		public Scenario cant_add_more_than_I_have_inputs_for_with_2_inputs =
			Given<TSubject>(CalculatorTakingTwoInputs)
				.When("I press the button twice", c => press_button_n_times(c, 2))
				.Using<TValuesProvider>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// Three inputs
		public Scenario calculator_can_add_two_of_three_numbers =
			Given<TSubject>(CalculatorTakingThreeInputs)
				.When("I press the button once", c => press_button_n_times(c, 1))
				.Using<TValuesProvider>()
				.Then("result should be result of second and third input", (s, r, v) => result_should_be_second_then_third(r,v))
				.Then("readout should be same as result", subject_readout_should_match_result);

		public Scenario calculator_can_add_three_numbers =
			Given<TSubject>(CalculatorTakingThreeInputs)
				.When("I press the button twice", c => press_button_n_times(c, 2))
				.Using<TValuesProvider>()
				.Then("result should be result of second and third then first input", (s, r, v) => result_should_be_second_and_third_then_first(r,v))
				.Then("readout should be same as result", subject_readout_should_match_result);

		public Scenario cant_add_more_than_I_have_inputs_for_with_3_inputs =
			Given<TSubject>(CalculatorTakingThreeInputs)
				.When("I press the button three times", c => press_button_n_times(c, 3))
				.Using<TValuesProvider>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");
	}

	public interface IDoCalculatorStuff<TSubject, TValuesProvider> {
		// Contexts:
		Context<TSubject> SubjectTakingTwoInputs ();
		Context<TSubject> SubjectTakingThreeInputs ();

		// expectations:
		int press_button_n_times (TSubject subject, int i);
		void subject_readout_should_match_result (TSubject subject, int result);
		void result_should_be_first_then_second (int result, TValuesProvider ValuesProvider);
		void result_should_be_second_then_third (int result, TValuesProvider ValuesProvider);
		void result_should_be_second_and_third_then_first (int result, TValuesProvider ValuesProvider);
	}
	#endregion <--- end of templating

	#region Contexts and expectation Values
	public class values_for_calculator_taking_inputs : IProvide<values_for_calculator_taking_inputs> {
		public int first, second, third;

		public int first_plus_second;
		public int second_plus_third;
		public int first_plus_second_plus_third;
		public int first_minus_second;
		public int second_minus_third;
		public int first_minus__second_minus_third;

		public values_for_calculator_taking_inputs[] Data () {
			return new[] {createFor(10, 20, 30), createFor(-10, 20, 0), createFor(-5, -5, -5),createFor(0, 0, 0), createFor(5, -5, 5), createFor(10000, 20000, 30000),createFor(-10000, -20000, 30000)};
		}

		public string StringRepresentation () {
			return "{ " + first + ", " + second + ", " + third + " }";
		}

		private values_for_calculator_taking_inputs createFor(int fst, int snd, int thd) {
			return new values_for_calculator_taking_inputs {
				first = fst,second = snd,third = thd,
				first_plus_second = fst + snd,
				first_plus_second_plus_third = fst + snd + thd,
				second_plus_third = snd + thd,
				first_minus_second = fst - snd,
				second_minus_third = snd - thd,
				first_minus__second_minus_third = fst - (snd - thd)
			};
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
	#endregion

	// Not a descendant of 'Feature', but has the feature attribute (so will be tested)
	[Behaviour("Addition",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the sum of numbers")]
	public class Addition : Pressing_buttons<Addition, Calculator, values_for_calculator_taking_inputs>,
		IDoCalculatorStuff<Calculator, values_for_calculator_taking_inputs>
	{
		public Context<Calculator> SubjectTakingTwoInputs () {return new a_calculator_taking_two_inputs();}
		public Context<Calculator> SubjectTakingThreeInputs () {return new a_calculator_taking_three_inputs();}

		public int press_button_n_times(Calculator subject, int n) {
			if (n < 1) throw new ArgumentException("n must be gte 1");
			for (int i = 0; i < n - 1; i++) {
				subject.Add();
			}
			return subject.Add();
		}

		public void subject_readout_should_match_result(Calculator subject, int result) {result.should_be_equal_to(subject.Readout());}
		public void result_should_be_first_then_second(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.first_plus_second);}
		public void result_should_be_second_then_third(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.second_plus_third);}     
		public void result_should_be_second_and_third_then_first(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.first_plus_second_plus_third);}
	}

	
	[Behaviour("Subtraction",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the difference of numbers")]
	public class Subtraction : Pressing_buttons<Subtraction, Calculator, values_for_calculator_taking_inputs>,
		IDoCalculatorStuff<Calculator, values_for_calculator_taking_inputs>
	{
		public Context<Calculator> SubjectTakingTwoInputs () {return new a_calculator_taking_two_inputs();}
		public Context<Calculator> SubjectTakingThreeInputs () {return new a_calculator_taking_three_inputs();}

		public int press_button_n_times(Calculator subject, int n) {
			if (n < 1) throw new ArgumentException("n must be gte 1");
			for (int i = 0; i < n - 1; i++) {
				subject.Subtract();
			}
			return subject.Subtract();
		}

		public void subject_readout_should_match_result(Calculator subject, int result) {result.should_be_equal_to(subject.Readout());}
		public void result_should_be_first_then_second(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.first_minus_second);}
		public void result_should_be_second_then_third(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.second_minus_third);}
		public void result_should_be_second_and_third_then_first(int result, values_for_calculator_taking_inputs ValuesProvider) {result.should_be_equal_to(ValuesProvider.first_minus__second_minus_third);}
	}
}
