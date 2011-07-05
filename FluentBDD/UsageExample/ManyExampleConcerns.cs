using System;
using FluentBDD;
using Moq;
using System.Runtime.Serialization;

// This set of classes demonstrates the usage of the FluentBdd framework.
// The first few tests have a lot of comments explaining usage.
// The later ones are cleaner, and better represent what you'd expect to write.
namespace UsageExample {

	[Behaviours("Addition",
		"As a user of a calculator",
		"To avoid making mistakes",
		"I want to be told the sum of two numbers")]
	public class Addition : Behaviours {
		// checking mocks with the Proof->Context->Action->Behaviour pattern
		public Behaviour the_calculator_uses_the_adder_supplied = // the name of the behaviour is inconsequential to how the tests are run. Use something instructive
			ProvedBy<values_for_a_calculator_using_math_provider>()
			.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("adding inputs", (c,e) => c.Add())
				.Then("adder interface should be used once").check_proof(p => p.adder_was_used_once()) // test method in IProvide values, keeps behaviour clean
				.Then("adder interface should be used and only once!").check_proof(p => p.adder_was_used_once());

		// Checking two compatible contexts against the same behaviour and the same values
		// The 'then' tests will appear in different places in the test output, as it is grouped by context.
		public Behaviour calculators_do_adding =
			ProvedBy<values_for_a_calculator_using_math_provider>()
				.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.AlsoGiven<a_calculator_that_uses_internal_logic_and_two_values>() // Different context, same subject type.
				.When("adding inputs", (c, e) => c.Add())
				.Then("should add the inputs (using two contexts!)").Result.should_be_equal_to.Proof(p => p.a_plus_b);

		// if you prefer, you can specify the 'using' case as below. Pay attention to the lack of brackets on 'Context.Of<T>'
		// syntax is "With<subjectType>(Context.Of<contextType>)"
		public Behaviour alternative_syntax =
			Given<Calculator, a_calculator_taking_two_inputs>().Using<values_for_a_calculator_taking_two_inputs>()
				.When("Entering another number", (c, e) => c.Press(0))
				.Then("should have new number in readout").Subject[s => s.Readout()].should_be_equal_to.the_value(0);

		// Here's how to leave an inconclusive behaviour (useful as a placholder when roughing out behaviours)
		public Behaviour unfinished_behaviour =
			ProvedBy<values_for_a_calculator_taking_two_inputs>()
				.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("doing something I haven't defined yet", (c, e) => { })
				.Then("should result in something I haven't tested yet").should_be_ignored;

		// Here's how to ignore an entire behaviour (should it be failing for a good and temporary reason)
		public Behaviour broken_behaviour =
			ProvedBy<values_for_a_calculator_taking_two_inputs>()
			.Given<Calculator, a_calculator_taking_two_inputs>()
				.When("doing something I've broken, but marked as ignored", Ignore.me)
				.Then("should ignore broken test!", (subject, result, values) => { throw new Exception("I'm broken!"); });

		// this behaviour's action ("when") gives NO result, so all the tests ("then") have only the subject. (and values if you request them)
		public Behaviour calculator_readout_reflects_input =
			Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.Using<values_for_a_calculator_using_math_provider>()
				.When("entering zero into it", (subject, proof) => subject.Press(0))
				.Then("the screen should show zero").Subject[s => s.Readout()].should_be_equal_to.the_value(0);


		// this behaviour's action ("when") gives a result, so all the tests ("then") take it as a param.
		public Behaviour adding_returns_the_sum_of_last_two_numbers =
			ProvedBy<values_for_a_calculator_using_math_provider>()
				.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("adding inputs", (c, e) => c.Add())
				.Then("the result should be the sum of inputs").Result.should_be_equal_to.the_value(3)
				.Then("the screen should show the result").Subject[s => s.Readout()].should_be_equal_to.TheResult;



		// this behaviour uses the context's values in the test Action.
		public Behaviour using_context_in_test_action =
			ProvedBy<values_for_a_calculator_using_math_provider>()
				.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("I press 'a' again", (subject, proof) => subject.Press(proof.a))
				.Then("the screen should show 'a'").Subject[s => s.Readout()].should_be_equal_to.Proof(p => p.a);

		// Testing for exceptions
		public Behaviour pressing_add_without_enough_input_causes_an_exception =
			ProvedBy<values_for_a_calculator_using_math_provider>()
			.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("I press 'add' three times", AddThreeTimes) // method without parenthesis, must return void.
				.ShouldThrow<InvalidOperationException>()
				.WithMessage("Stack empty.");

		// If you don't care about the message, you'll have to say so explicitly.
		public Behaviour ignoring_exception_messages =
			ProvedBy<values_for_a_calculator_using_math_provider>()
			.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("I press 'add' three times and ignore the exception message", AddThreeTimes)
				.ShouldThrow<InvalidOperationException>()
				.IgnoreMessage();

		
		// If you want to describe the mode of failure, or the behaviour it indicates, do this:
		public Behaviour described_exceptions =
			ProvedBy<values_for_a_calculator_using_math_provider>()
				.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("I press 'add' three times", AddThreeTimes)
				.Then("I should get an error and no result").should_throw(new InvalidOperationException("Stack empty."));

		// To use a described exception but ignore the message, pass an empty string for the message
		public Behaviour described_exceptions_ignoring_message =
			ProvedBy<values_for_a_calculator_using_math_provider>()
				.Given<Calculator, a_calculator_that_uses_a_math_provider_interface_and_two_values>()
				.When("I press 'add' three times", AddThreeTimes)
				.Then("I should get an error and no result, regardless of message").should_throw(new InvalidOperationException(""));

		#region Same context used with different actions. Context will be grouped in output, but actions will be seperate.
		public Behaviour last_item_on_stack_shows =
			Given<Calculator, a_calculator_taking_three_inputs>()
				.Using<values_for_a_calculator_taking_three_inputs>()
				.When("No action is taken", (c, e) => { }) // common pattern for doing nothing
				.Then("The last input value should be on the screen").Subject[s => s.Readout()].should_be_equal_to.Proof(p => p.c);

		public Behaviour adding_once_adds_last_two_items =
			Given<Calculator, a_calculator_taking_three_inputs>()
				.Using<values_for_a_calculator_taking_three_inputs>()
				.When("Adding once", (c, e) => c.Add())
				.Then("Result should be b+c").Result.should_be_equal_to.Proof(p => p.b_plus_c);

		public Behaviour adding_twice_adds_all_three_items =
			Given<Calculator, a_calculator_taking_three_inputs>()
				.Using<values_for_a_calculator_taking_three_inputs>()
				.When("Adding twice", (c, e) => AddTwice(c)) // Feature's method in lambda
				.Then("Result should be a+b+c").Result.should_be_equal_to.Proof(p => p.a_plus_b_plus_c);
		#endregion

		// more complex 'when' actions can be rolled out into static methods to keep behaviours clean.
		private static void AddThreeTimes (Calculator c, object values) { // method takes the subject as it's only parameter, so can be passed directly
			c.Add(); c.Add(); c.Add();
		}
		private static int AddTwice(Calculator c) {
			c.Add(); return c.Add();
		}


		#region Contexts
		// Contexts provide the subject for a behaviour. They are a way to 
		// combine creation, setup and value injection without needing to
		// specify any behaviour or any specific concrete values.


		// These classes don't need to be embedded in 'Addition', nor do they need to be marked internal.
		// You can use any accessible class of type Context<T> for a behaviour.
		// Making them internal and embedded keeps things tidy.
		internal class a_calculator_that_uses_a_math_provider_interface_and_two_values : Context<Calculator>, IUse<values_for_a_calculator_using_math_provider> {
			public values_for_a_calculator_using_math_provider Values { get; set; }

			public override void SetupContext () {
				Given("I have a calculator using the IDoMath interface", () => new Calculator(Values.MathProvider))
					.And("I type in " + Values.a + " and " + Values.b, c => { c.Press(Values.b); c.Press(Values.a); });
			}
		}

		internal class a_calculator_that_uses_internal_logic_and_two_values : Context<Calculator>, IUse<values_for_a_calculator_using_math_provider> {
			public values_for_a_calculator_using_math_provider Values { get; set; }
			// This context is a bit contrived to show how to test more than one context against 
			// a single set of expectations.
			public override void SetupContext () {
				Given("I have a calculator using internal logic", () => new Calculator())
					.And("I type in " + Values.a + " and " + Values.b, c => { c.Press(Values.b); c.Press(Values.a); });
			}
		}


		// Expectations for behaviours -- values for input and output, tests around mocks.
		// These help to keep contexts and behaviour specs nice and clean.
		// These can be re-used by any context that implements the matching IUse<>
		internal class values_for_a_calculator_using_math_provider : IProvide<values_for_a_calculator_using_math_provider> {
			private values_for_a_calculator_using_math_provider SetupWithMocks () {
				a = 1;
				b = 2;
				a_plus_b = a + b;
				mock_provider = new Mock<IDoMath>();
				mock_provider.Setup(m => m.Add(a, b)).Returns(3);
				MathProvider = mock_provider.Object;
				return this;
			}

			public void adder_was_used_once () {
				mock_provider.Verify(m => m.Add(a, b), Times.Once());
			}

			public int a, b;
			public int a_plus_b;
			public IDoMath MathProvider;
			protected Mock<IDoMath> mock_provider;

			public values_for_a_calculator_using_math_provider[] Data () {
				return new[] { SetupWithMocks() };
			}

			public string StringRepresentation () {
				return "a = " + a + ", b = " + b + " and a mock math provider";
			}
		}

		// more contexts at the bottom of the file...
		#endregion

	}


	// Below are behaviours that don't have subjects to be set up.
	// Expected to be used mostly for testing new() etc.
	[Behaviours("Creation")]
	public class CreatingACalculator : Behaviours {
		public Behaviour when_creating_a_calculator =
			GivenNoSubject()
				.When("I create a calculator", with => new Calculator())
				.Then("I should have a new calculator").Result.should_not_be_null;


		public Behaviour when_doing_things_with_exceptions =
			GivenNoSubject()
				.When("I create a calculator with a null math delegate", c => { new Calculator(null); })
				.ShouldThrow<ArgumentException>()
				.WithMessage("A math delegate must be provided");
	}

	[Behaviours("Data Contracts",
		"As calculator service provider",
		"To make loads of money, I want to be able to serialise my calculator via SOAP",
		"For this to work, I need attributes on the calculator and it's fields")]
	public class DataContracts : Behaviours {
		internal class a_calculator : Context<Calculator> {
			public override void SetupContext () {
				Given("I have a calculator", () => new Calculator());
			}
		}

		public Behaviour calculator_should_have_datacontracts =
			Given<Calculator, a_calculator>()
				.Verify()
				.ShouldHaveAttribute<DataContractAttribute>() // subject type must have the named attribute
				.ShouldHaveAttribute<SerializableAttribute>()
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("stack", m => m.Name == "Stack") // must have field, attribute and correct attrib values.
				.ShouldHaveField("display") // just checks the field is there
				.ShouldHaveFieldWithAttribute<DataMemberAttribute>("doMath"); // field must have attribute, but attribute values don't matter.

	}


	// Features can inherit from other features.
	// If the base class isn't decorated with the 'Feature' attribute, it's own tests won't be run.
	[Behaviours("Inhereted subtraction")]
	public class OtherSubtraction : DataContracts { }


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