using FluentBDD;
using FluentBDD.Assertions;

namespace UsageExample {
	[Behaviour("Split proofs experiment")]
	class _Experiment__SplitProofs : Behaviours {

		// Old style
		public Scenario test = 
			Given<Calculator>(Context.Of<taking_two_numbers>)
			.Using<a_proof_with_two_numbers, Proofs_with_two_numbers>()
			.When("doing something", (subject, template) => subject.Add())
			.Then("should get a result", (subject, result, concrete) => result.should_be_equal_to(concrete.sum));


		// Newer, Eldon/Dermot inspired format
		public Scenario adding_two_numbers = Proved.By<a_proof_with_two_numbers, Proofs_with_two_numbers>()
			.Given<Calculator, taking_two_numbers>()
			.When("doing something", (subject, template) => subject.Add())
			.Then("should get a result", (subject, result, concrete) => result.should_be_equal_to(concrete.sum));

	}


	internal class taking_two_numbers : Context<Calculator>, IUse<a_proof_with_two_numbers> {
		public a_proof_with_two_numbers Values { get; set; }
		public override void SetupContext() {
			Given("a new calculator", () => new Calculator())
				.And("I type 'a' and 'b' into it", c => {
				                                   	c.Press(Values.a);
				                                   	c.Press(Values.b);
				                                   });
		}
	}
	internal interface a_proof_with_two_numbers {
		int a { get; set; }
		int b { get; set; }
		int sum { get; set; }
	}
	internal class Proofs_with_two_numbers : a_proof_with_two_numbers, IProvide<a_proof_with_two_numbers> {
		public int a { get; set; }
		public int b { get; set; }
		public int sum { get; set; }

		public a_proof_with_two_numbers[] Data () {
			return new[] {
				new Proofs_with_two_numbers {a = 1, b = 2, sum =3},
				new Proofs_with_two_numbers {a = -1, b = 1, sum = 0},
				new Proofs_with_two_numbers {a = 20, b = -5, sum = 15}
			};
		}

		public string StringRepresentation() {
			return "a = " + a + "; b = " + b + "; sum = " + sum;
		}
	}
}