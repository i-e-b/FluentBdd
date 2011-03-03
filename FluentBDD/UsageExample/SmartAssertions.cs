using FluentBDD;

namespace UsageExample {
	[Feature("Smart assertions")]
	public class SmartAssertions : Feature {

		public Scenario test1 =
			With(() => Context.Of<a_thing>())
				.When("I return 10", o => 10)
				.Then_("the result should be 10", the => the.result.should.equal(10))
				.Then_("the result should not be 11", the => the.result.should.not.equal(11));

		public Scenario test2 =
			With(() => Context.Of<a_dto>())
				.When("I do nothing", o => { })
				.Then_("the DTO should be storing 'booga'", _=>_.subject(s => s.storedString).should.equal("booga"));
	}

	public class a_dto:Context<DtoThing>{
		public override void SetupContext() {
			Given("a DTO object", () => new DtoThing())
				.And("I store 'booga' in it", dto => dto.storedString = "booga");
		}
	}

	public class DtoThing {
		public string storedString;
	}

	public class a_thing:Context<object> {
		public override void SetupContext() {
			Given("an object", () => new object());
		}
	}
}
