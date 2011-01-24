using System;
using BddAddinAttributes;
using FluentBDD;
using UsageExample;
using Moq;

// This set of classes demonstrates the usage of the FluentBdd framework.
// The first few tests have a lot of comments explaining usage.
// The later ones are cleaner, and better represent what you'd expect to write.
namespace CalculatorConcerns {

	[Feature("Addition",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the sum of two numbers")]
	public class Addition : Feature {

		#region Contexts
		// These classes don't need to be embedded in 'Addition', nor do they need to be marked internal.
		// You can use any accessible class of type Context<T> for a scenario.
		// Making them internal and embedded keeps things tidy.
		internal class a_calculator_with_a_mock_in_it : Context<Calculator> {
			public Mock<IDoMath> mock_adder;
			protected int a, b;

			public a_calculator_with_a_mock_in_it () {
				Given("I have a calculator using the adder interface", () => new Calculator(mock_adder.Object))
					.And("I type in " + a + " and " + b,
					c => { c.Press(b); c.Press(a); });
			}

			public void check_adder_was_used () {
				mock_adder.Verify(m => m.Add(a, b), Times.Once());
			}

			public override void Setup () {
				a = 1;
				b = 2;
				mock_adder = new Mock<IDoMath>();
				mock_adder.Setup(m => m.Add(a, b)).Returns(3);
			}
		}
		internal class a_calculator : Context<Calculator> {
			public a_calculator () {
				Given("I have a calculator", () => new Calculator());
			}
		}
		
		internal class a_calculator_with_three_numbers_entered : Context<Calculator> {
			public a_calculator_with_three_numbers_entered () {
				Given("I have a calculator", () => new Calculator())
					.And("I enter 4, 6 and 8", c => {
					                           	c.Press(4);
					                           	c.Press(6);
					                           	c.Press(8);
					                           });
			}
		}
		#endregion

		#region Scenarios with contexts defined elsewhere (bread-and-butter: do this most of the time.)

		// this scenario's action ("when") gives NO result, so all the tests ("then") have only the subject.
		public Scenario calculator_readout_reflects_input =
			With(() => Context.Of<a_calculator_with_a_mock_in_it>())
				.When("entering zero into it", subject => subject.Press(0))
				.Then("the screen should show zero", subject => subject.Readout().should_be_equal_to(0));


		// this scenario's action ("when") gives a result, so all the tests ("then") take it as a param.
		public Scenario adding_returns_the_sum_of_last_two_numbers =
			With(() => Context.Of<a_calculator_with_a_mock_in_it>())
				.When("adding inputs", c => c.Add())
				.Then("the result should be the sum of inputs", (subject, result) => result.should_be_equal_to(3))
				.Then("the screen should show the result", (subject, result) => subject.Readout().should_be_equal_to(result));


		// no-result actions can test for exceptions.
		// to ensure no result, wrap methods in curly braces.
		public Scenario pressing_add_without_enough_input_causes_an_exception =
			With(() => Context.Of<a_calculator_with_a_mock_in_it>())
				.When("I press 'add' three times", AddThreeTimes) // method without parenthesis
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		private static void AddThreeTimes (Calculator c) { // method takes the subject as it's only parameter, so can be passed directly
			c.Add(); c.Add(); c.Add();
		}


		// This checks that the context is created once per 'then'
		public Scenario the_calculator_uses_the_adder_supplied =
			With(() => Context.Of<a_calculator_with_a_mock_in_it>())
				.When("adding inputs", c => { c.Add(); })
				.Then<a_calculator_with_a_mock_in_it>("adder interface should be used once",
													   c => c.check_adder_was_used()) // test method in subject, keeps scenario clean
				.Then<a_calculator_with_a_mock_in_it>("adder interface should be used and only once!",
													   c => c.check_adder_was_used());
		#endregion

		#region Lightweight-contexts (for simple cases or when roughing out an idea)

		// this scenario's action ("when") gives a result, so all the tests ("then") take it as a param.
		public Scenario when_adding_two_numbers =
			With(() => Context.Of<a_calculator>())
			.And("I have pressed 20 into the calculator", ctx => ctx.Press(20))
			.And("I have pressed 30 into the calculator", ctx => ctx.Press(30))
			.When("I press the add button", ctx => ctx.Add())
			.Then("the calculator should give me 50 as a result", (calc, result) => result.should_be_equal_to(50))
			.Then("the calculator screen should show same as result", (calc, result) => result.should_be_equal_to(calc.Readout()));

		// this scenario's action ("when") gives NO result, so all the tests ("then") have only the subject.
		public Scenario when_adding_three_numbers =
			With(() => Context.Of<a_calculator>())
				.And("I put 3, 8 and 1 into it", ctx => {
				                                 	ctx.Press(3);
				                                 	ctx.Press(8);
				                                 	ctx.Press(1);
				                                 })
				.When("I press the add button twice", c => { c.Add(); c.Add(); })
				.Then("the calculator screen should show 12", c => c.Readout().should_be_equal_to(12));

		#endregion

		// Same context used with different actions. Context will be grouped in output, but actions will be seperate.
		public Scenario last_item_on_stack_shows =
			With(() => Context.Of<a_calculator_with_three_numbers_entered>())
				.When("No action is taken", c => { })
				.Then("The last input value should be on the screen", c => c.Readout().should_be_equal_to(8));

		public Scenario adding_once_adds_last_two_items =
			With(() => Context.Of<a_calculator_with_three_numbers_entered>())
				.When("Adding once", c => c.Add())
				.Then("Result should be 14", (c, r) => r.should_be_equal_to(14));

		public Scenario adding_twice_adds_all_three_items =
			With(() => Context.Of<a_calculator_with_three_numbers_entered>())
				.When("Adding twice", c => {
				                      	c.Add();
				                      	return c.Add();
				                      })
				.Then("Result should be 18", (c, r) => r.should_be_equal_to(18));
	}

	// Below are scenarios that don't have subjects to be set up.
	// Expected to be used mostly for testing new() etc.
	[Feature("Creation")]
	public class CreatingACalculator : Feature {
		public Scenario when_creating_a_calculator =
			GivenNoSubject()
				.When("I create a calculator", with => new Calculator())
				.Then("I should have a new calculator", (no_subject, calculator) => calculator.should_not_be_null());

		
		public Scenario when_doing_things_with_exceptions =
			GivenNoSubject()
				.When("I create a calculator with a null adder", c => { new Calculator(null); })
				.ShouldThrow<ArgumentException>()
				.WithMessage("An adder must be provided");
	}

	[Feature("Subtraction",
		"As a user of a calculator",
		"To avoid making mistakes in simple arithmatic problems",
		"I want to be told the difference between two numbers")]
	public class Subtraction : Feature {
		internal class a_calculator_with_two_numbers_entered : Context<Calculator> {
			public const int a = 10;
			public const int b = 20;
			public const int expected_result = -10;

			public a_calculator_with_two_numbers_entered () {
				Given("a calculator", () => new Calculator())
					.And("I type " + a + " and " + b + " into it", s =>
					{
						s.Press(a);
						s.Press(b);
					});
			}
		}

		internal class a_calculator_with_three_numbers_entered : a_calculator_with_two_numbers_entered {
			public const int c = 5;

			public a_calculator_with_three_numbers_entered() {
				GivenBaseContext()
					.And("I type " + c + " into it", s => s.Press(c));
			}
		}

		public Scenario subtracting_numbers_gives_the_expected_result =
			With(() => Context.Of<a_calculator_with_two_numbers_entered>())
				.When("I press subtract", c => c.Subtract())
				.Then("I should get expected result", (s, r) => r.should_be_equal_to(a_calculator_with_two_numbers_entered.expected_result))
				.Then("screen should show result", (s, r) => s.Readout().should_be_equal_to(r));

		public Scenario subtracting_without_enough_input_gives_an_exception =
			With(() => Context.Of<a_calculator_with_two_numbers_entered>())
				.When("I press subtract twice", press_subtract_twice)
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		public Scenario subtracting_twice_uses_three_items_entered =
			With(() => Context.Of<a_calculator_with_three_numbers_entered>())
				.When("I press subtract twice", press_subtract_twice)
				.Then("I should get the difference of the three numbers in order", c => c.Readout().should_be_equal_to(-5));


		private static void press_subtract_twice (Calculator c) {
			c.Subtract();
			c.Subtract();
		}

		/* I would like to be able to say:
		 * 
		 
		public Scenario subtracting_numbers_gives_the_expected_result = 
			With(inputs_for_a_calculator, () => Context.Of<a_calculator>())
				.When("I press subtract", c => c.Subtract())
				.Then("I should get the difference of the two inputs", (s,r,input) => r.should_equal(i.expected_result));
		
		 * 
		 * This could be a parameter of type 'calculator_input' on the 'a_calculator' context ctor,
		 * where 'inputs_for_a_calculator' is an IEnumerable<calculator_input>.
		 * The 'Then' lambda would return subject, [result], input;
		 * where 'input' would be of type 'calculator_input'. The 'Then' would result in (?one test/multiple tests?)
		 * that runs all the test cases. [once when/then are split, might make more sense to do multiple]
		 * 
		 
		public class a_calculator : Context<Calculator, calculator_input> {
			public a_calculator (calculator_input i) {
				Given(...);
			}
		}
		 
		 * 
		 */
	}
}