using FluentBDD;

namespace UsageExample {

	[Feature("Playing with assertion syntax")]
	public class DifferentAssertionSyntaxPlayground : Feature {

		public Scenario old_style =
			GivenNoSubject()
				.When("I do stuff", no_subject => 1)
				.Then("I should get results", (s, r) => r.should_be_equal_to(1));


		/*public Scenario desired =
			GivenNoSubject()
				.When("I do stuff", no_subject => 1)
				.Then("I should get results", result.should.not.equal(2));

		public Scenario desired =
		   GivenNoSubject()
			   .When("I do stuff", no_subject => 1)
			   .Then("I should get results", result.should.equal(2));*/

		public Scenario test =
			GivenNoSubject()
				.When("I do stuff", no_subject => 1)
				.Then("I should get results", result<no_subject>.should.equal(2));
	}
}
