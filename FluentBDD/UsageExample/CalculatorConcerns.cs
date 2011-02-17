using System;
using FluentBDD;
using UsageExample;
using Moq;
using System.Runtime.Serialization;

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
		// Contexts provide the subject for a scenario. They are a way to 
		// combine creation, setup and value injection without needing to
		// specify any behaviour or any specific concrete values.


		// These classes don't need to be embedded in 'Addition', nor do they need to be marked internal.
		// You can use any accessible class of type Context<T> for a scenario.
		// Making them internal and embedded keeps things tidy.
		internal class a_calculator_that_uses_a_math_provider_interface_and_two_values : Context<Calculator>, IUse<values_for_a_calculator_using_math_provider> {
			public values_for_a_calculator_using_math_provider Values { get; set; }

			public override void SetupContext () {
				Given("I have a calculator using the IDoMath interface", () => new Calculator(Values.MathProvider))
					.And("I type in " + Values.a + " and " + Values.b, c => { c.Press(Values.b); c.Press(Values.a); });
			}
		}

		internal class a_calculator_that_uses_internal_logic_and_two_values: Context<Calculator>, IUse<values_for_a_calculator_using_math_provider> {
			public values_for_a_calculator_using_math_provider Values { get; set; }
			// This context is a bit contrived to show how to test more than one context against 
			// a single set of expectations.
			public override void SetupContext () {
				Given("I have a calculator using internal logic", () => new Calculator())
					.And("I type in " + Values.a + " and " + Values.b, c => { c.Press(Values.b); c.Press(Values.a); });
			}
		}


		// Expectations for scenarios -- values for input and output, tests around mocks.
		// These help to keep contexts and behaviour specs nice and clean.
		// These can be re-used by any context that implements the matching IUse<>
		internal class values_for_a_calculator_using_math_provider : IProvide<values_for_a_calculator_using_math_provider> {
			private values_for_a_calculator_using_math_provider SetupWithMocks() {
				a = 1;
				b = 2;
				a_plus_b = a + b;
				mock_provider = new Mock<IDoMath>();
				mock_provider.Setup(m => m.Add(a, b)).Returns(3);
				MathProvider = mock_provider.Object;
				return this;
			}

			public void check_adder_was_used_once () {
				mock_provider.Verify(m => m.Add(a, b), Times.Once());
			}

			public int a, b;
			public int a_plus_b;
			public IDoMath MathProvider;
			protected Mock<IDoMath> mock_provider;

			public values_for_a_calculator_using_math_provider[] Data() {
				return new[] { SetupWithMocks() };
			}

			public string StringRepresentation() {
				return "a = " + a + ", b = " + b + " and a mock math provider";
			}
		}

		// more contexts at the bottom of the file...
		#endregion


		// checking mocks with the Context->Action->Values->Behaviour pattern
		public Scenario the_calculator_uses_the_adder_supplied = // the name of the scenario is inconsequential to how the tests are run. Use something instructive
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("adding inputs", c => c.Add())
				.Using<values_for_a_calculator_using_math_provider>()
				.Then("adder interface should be used once", (subject, values) => values.check_adder_was_used_once()) // test method in IProvide values, keeps scenario clean
				.Then("adder interface should be used and only once!", (s, v) => v.check_adder_was_used_once());

		// Checking two compatible contexts against the same behaviour and the same values
		// The 'then' tests will appear in different places in the test output, as it is grouped by context.
		public Scenario calculators_do_adding =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.And(Context.Of<a_calculator_that_uses_internal_logic_and_two_values>) // Different context, same subject type.
				.When("adding inputs", c => c.Add())
				.Using<values_for_a_calculator_using_math_provider>()
				.Then("should add the inputs (using two contexts!)", (subject, result, values) => result.should_be_equal_to(values.a_plus_b));

		// if you prefer, you can specify the 'with' case as below. Pay attention to the lack of brackets on 'Context.Of<T>'
		// syntax is "With<subjectType>(Context.Of<contextType>)"
		public Scenario alternative_syntax =
			With<Calculator>(Context.Of<a_calculator_taking_two_inputs>)
				.When("Entering another number", c => c.Press(0))
				.Using<values_for_a_calculator_taking_two_inputs>()
				.Then("should have new number in readout", (s,r,v) => s.Readout().should_be_equal_to(0));

		// Here's how to leave an inconclusive scenario (useful as a placholder when roughing out behaviours)
		public Scenario unfinished_scenario =
			With(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("doing something I haven't defined yet", c => { })
				.Using<values_for_a_calculator_taking_two_inputs>()
				.Then("should result in something I haven't tested yet", s => s.should_be_ignored());

		// Here's how to ignore an entire scenario (should it be failing for a good and temporary reason)
		public Scenario broken_scenario =
			With(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("doing something I've broken, but marked as ignored", c => c.should_be_ignored())
				.Using<values_for_a_calculator_taking_two_inputs>()
				.Then("should ignore broken test!", s => { throw new Exception("I'm broken!"); });

		// this scenario's action ("when") gives NO result, so all the tests ("then") have only the subject. (and values if you request them)
		public Scenario calculator_readout_reflects_input =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("entering zero into it", subject => subject.Press(0))
				.Using<values_for_a_calculator_using_math_provider>()
				.Then("the screen should show zero", subject => subject.Readout().should_be_equal_to(0));


		// this scenario's action ("when") gives a result, so all the tests ("then") take it as a param.
		public Scenario adding_returns_the_sum_of_last_two_numbers =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("adding inputs", c => c.Add())
				.Using<values_for_a_calculator_using_math_provider>()
				.Then("the result should be the sum of inputs", (subject, result, values) => result.should_be_equal_to(3))
				.Then("the screen should show the result", (s, r, v) => s.Readout().should_be_equal_to(r));

		// this scenario uses the context's values in the test Action.
		// it's a bit messy, and should generally be avoided.
		public Scenario using_context_in_test_action =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("I press 'a' again", (subject, context) => 
					subject.Press(((a_calculator_that_uses_a_math_provider_interface_and_two_values)context).Values.a))
				.Using<values_for_a_calculator_using_math_provider>()
				.Then("the screen should show 'a'", (s, r, v) => s.Readout().should_be_equal_to(v.a));


		// Testing for exceptions
		public Scenario pressing_add_without_enough_input_causes_an_exception =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("I press 'add' three times", AddThreeTimes) // method without parenthesis, must return void.
				.Using<values_for_a_calculator_using_math_provider>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// If you don't care about the message, you'll have to say so explicitly.
		public Scenario ignoring_exception_messages =
			With(() => Context.Of<a_calculator_that_uses_a_math_provider_interface_and_two_values>())
				.When("I press 'add' three times and ignore the exception message", AddThreeTimes) // method without parenthesis, must return void.
				.Using<values_for_a_calculator_using_math_provider>()
				.ShouldThrow<InvalidOperationException>()
				.IgnoreMessage();

		#region Same context used with different actions. Context will be grouped in output, but actions will be seperate.
		public Scenario last_item_on_stack_shows =
			With(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("No action is taken", c => { }) // common pattern for doing nothing
				.Using<values_for_a_calculator_taking_three_inputs>()
				.Then("The last input value should be on the screen", (c,v) => c.Readout().should_be_equal_to(v.c));

		public Scenario adding_once_adds_last_two_items =
			With(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("Adding once", c => c.Add())
				.Using<values_for_a_calculator_taking_three_inputs>()
				.Then("Result should be b+c", (c, r, v) => r.should_be_equal_to(v.b_plus_c));

		public Scenario adding_twice_adds_all_three_items =
			With(() => Context.Of<a_calculator_taking_three_inputs>())
				.When("Adding twice", c => AddTwice(c)) // Feature's method in lambda
				.Using<values_for_a_calculator_taking_three_inputs>()
				.Then("Result should be a+b+c", (c, r, v) => r.should_be_equal_to(v.a_plus_b_plus_c));
		#endregion

		// more complex 'when' actions can be rolled out into static methods to keep scenarios clean.
		private static void AddThreeTimes (Calculator c) { // method takes the subject as it's only parameter, so can be passed directly
			c.Add(); c.Add(); c.Add();
		}
		private static int AddTwice(Calculator c) {
			c.Add(); return c.Add();
		}
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
				.When("I create a calculator with a null math delegate", c => { new Calculator(null); })
				.ShouldThrow<ArgumentException>()
				.WithMessage("An math delegate must be provided");
	}

	[Feature("Subtraction",
		"As a user of a calculator",
		"To avoid making mistakes in simple arithmatic problems",
		"I want to be told the difference between two numbers")]
	public class Subtraction : Feature {

		public Scenario subtracting_numbers_gives_the_expected_result =
			With(() => Context.Of<a_calculator_with_two_numbers_entered>())
				.When("I press subtract", c => c.Subtract())
				.Then("I should get expected result", (s, r) => r.should_be_equal_to(a_calculator_with_two_numbers_entered.expected_result))
				.Then("screen should show result", (s, r) => s.Readout().should_be_equal_to(r));

		public Scenario subtracting_twice_uses_three_items_entered =
			With(() => Context.Of<a_calculator_with_three_numbers_entered>())
				.When("I press subtract twice", press_subtract_twice)
				.Then("I should get the difference of the three numbers in order", c => c.Readout().should_be_equal_to(-5));

		public Scenario subtracting_two_numbers =
			With(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press subtract", c => c.Subtract())
				.Using<values_for_a_calculator_taking_two_inputs>()
				.Then("I should get the difference of the two inputs in order",
					  (subject, result, values) => result.should_be_equal_to(values.a_minus_b))
				.Then("display should match result",
					  (subject, result, values) => result.should_be_equal_to(subject.Readout()));


		public Scenario subtracting_without_enough_input_gives_an_exception =
			With(() => Context.Of<a_calculator_taking_two_inputs>())
				.When("I press subtract twice", press_subtract_twice)
				.Using<values_for_a_calculator_taking_two_inputs>()
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		public Scenario subtracting_without_enough_input_gives_an_exception__without_examples =
			With(() => Context.Of<a_calculator_with_two_numbers_entered>())
				.When("I press subtract twice", press_subtract_twice)
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");


		public Scenario subtracting_two_numbers_without_examples =
			With<Calculator>(Context.Of<a_calculator_with_two_numbers_entered>)
				.When("I press subtract", c => { c.Subtract(); })
				.Then("I should see \"-10\" on the readout",
					 subject => subject.Readout().should_be_equal_to(-10));


		private static void press_subtract_twice (Calculator c) {
			c.Subtract();
			c.Subtract();
		}

		internal class a_calculator_with_two_numbers_entered : Context<Calculator> {
			public const int a = 10;
			public const int b = 20;
			public const int expected_result = -10;

			public override void SetupContext () {
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

			public override void SetupContext () {
				base.SetupContext();
				GivenBaseContext()
					.And("I type " + c + " into it", s => s.Press(c));
			}
		}
	}

	[Feature("Data Contracts",
		"As calculator service provider",
		"To make loads of money, I want to be able to serialise my calculator via SOAP",
		"For this to work, I need attributes on the calculator and it's fields")]
	public class DataContracts : Feature {
		internal class a_calculator : Context<Calculator> {
			public override void SetupContext () {
				Given("I have a calculator", () => new Calculator());
			}
		}

		public Scenario calculator_should_have_datacontracts =
			With(() => Context.Of<a_calculator>())
				.Verify()
				.ShouldHaveAttribute<DataContractAttribute>() // subject type must have the named attribute
				.ShouldHaveAttribute<SerializableAttribute>()
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("stack", m => m.Name == "Stack") // must have field, attribute and correct attrib values.
				.ShouldHaveField("display") // just checks the field is there
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("doMath"); // field must have attribute, but attribute values don't matter.

	}


	// Features can inherit from other features.
	// If the base class isn't decorated with the 'Feature' attribute, it's own tests won't be run.
	[Feature("Inhereted subtraction")]
	public class OtherSubtraction : Subtraction { }


	internal class a_calculator_taking_two_inputs : Context<Calculator>, IUse<values_for_a_calculator_taking_two_inputs> {
		public values_for_a_calculator_taking_two_inputs Values { get; set; }

		public override void SetupContext () {
			Given("a calculator", () => new Calculator())
				.And("I type 'a' into it", s => s.Press(Values.a))
				.And("I type 'b' into it", s => s.Press(Values.b));
		}
	}
	internal class values_for_a_calculator_taking_two_inputs : IProvide<values_for_a_calculator_taking_two_inputs> {
		public values_for_a_calculator_taking_two_inputs[] Data () {
			return new[] {
					new values_for_a_calculator_taking_two_inputs {a = 0, b = 10, a_minus_b = -10, a_plus_b = 10},
					new values_for_a_calculator_taking_two_inputs {a = 10, b = 10, a_minus_b = 0, a_plus_b = 20},
					new values_for_a_calculator_taking_two_inputs {a = 10, b = 0, a_minus_b = 10, a_plus_b = 10},
				};
		}

		public string StringRepresentation () {
			return "{ a = " + a + "; b = " + b + "; a+b : " + a_plus_b + "; a-b : " + a_minus_b + "; }";
		}

		public int a { get; set; }
		public int b { get; set; }
		public int a_minus_b { get; set; }
		public int a_plus_b { get; set; }
	}


	internal class a_calculator_taking_three_inputs : Context<Calculator>, IUse<values_for_a_calculator_taking_three_inputs> {
		public values_for_a_calculator_taking_three_inputs Values {get; set; }
		public override void SetupContext () {
			Given("I have a calculator", () => new Calculator())
				.And("I enter a, b and c", c =>
				{
					c.Press(Values.a);
					c.Press(Values.b);
					c.Press(Values.c);
				});
		}
	}
	internal class values_for_a_calculator_taking_three_inputs : IProvide<values_for_a_calculator_taking_three_inputs> {
		public values_for_a_calculator_taking_three_inputs[] Data() {
			return new[] {
					new values_for_a_calculator_taking_three_inputs {a = 0, b = 10, c = 5,
						a_minus_b = -10, a_plus_b = 10, a_plus_b_plus_c = 15, b_plus_c = 15},

					new values_for_a_calculator_taking_three_inputs {a = 10, b = 10, c=-5,
						a_minus_b = 0, a_plus_b = 20, a_plus_b_plus_c = 15, b_plus_c = 5},

					new values_for_a_calculator_taking_three_inputs {a = 10, b = 0, c=10,
						a_minus_b = 10, a_plus_b = 10, a_plus_b_plus_c = 20, b_plus_c = 10},
				};
		}

		public string StringRepresentation () {
			return "{ a = " + a + "; b = " + b + "; c = " + c + "; }";
		}

		public int a { get; set; }
		public int b { get; set; }
		public int c { get; set; }
		public int a_minus_b { get; set; }
		public int a_plus_b { get; set; }
		public int a_plus_b_plus_c { get; set; }
		public int b_plus_c { get; set; }
	}
}