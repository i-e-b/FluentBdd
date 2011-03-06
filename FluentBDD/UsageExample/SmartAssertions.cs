using FluentBDD;

namespace UsageExample {
	[Feature("Smart assertions")]
	public class SmartAssertions : Feature {

		// no expectations
		public Scenario test1 =
			With(() => Context.Of<a_thing>())
				.When("I return 10", o => 10)
				.Then_("the result should be 10", the => the.result.should.equal(10))
				.Then_("the result should not be 11", the => the.result.should.not.equal(11));

		public Scenario test2 =
			With(() => Context.Of<a_dto>())
				.When("I do nothing", o => { })
				.Then_("the DTO should be storing 'booga'", _=>_.subject(s => s.storedString).should.equal("booga"));

		// using expectations
		public Scenario test3 =
			With(() => Context.Of<a_dto>())
				.When("I return 'wogga'", o => "wogga")
				.Using<some_expectations>()
				.Then_("the returned string should be the same as expectation", _ => _.result.should.match.expectation(v => v.aValue));

		public Scenario test4 =
			With(() => Context.Of<a_dto>())
				.When("I do nothing", o => { })
				.Using<some_expectations>()
				.Then_("the DTO should be storing different string to expectation", _ => _.subject(s => s.storedString).should.not.match.expectation(v=>v.aValue));

		// comparing result to subject
		public Scenario test5 =
			With(() => Context.Of<a_dto>())
				.When("I return 'booga'", o => "booga")
				.Then_("the DTO should be same as result", _ => _.result.should.match.subject(s => s.storedString));

		public Scenario test6 =
			With(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("I return 'wogga'", (o,e) => e.Values.aValue)
				.Then_("the DTO should be storing different string to result", _ => _.result.should.not.match.subject(s => s.storedString));

		// comparing subject to subject
		public Scenario test7 =
			With(() => Context.Of<a_dto>())
				.When("I return 'booga'", o => "booga")
				.Then_("subject string should be equal to itself", _ => _.subject(s => s.storedString).should.match.subject(s => s.storedString));

		public Scenario test8 =
			With(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("I return 'wogga'", (o, e) => e.Values.aValue)
				.Then_("subject string should be equal to itself", _ => _.subject(s => s.storedString).should.match.subject(s => s.storedString));


		// note: there is not "expectation.should..." on purpose. We don't test our expectations!

		// todo: all the other assertions in old-style assertions.
	}

	public class some_expectations : IProvide<some_expectations> {
		public string aValue;

		public some_expectations[] Data () {
			return new[] {
				new some_expectations {aValue = "wogga"}
			};
		}

		public string StringRepresentation () {
			return "A string = " + aValue;
		}
	}

	public class a_dto : Context<DtoThing>, IUse<some_expectations> {
		public some_expectations Values { get; set; }
		public override void SetupContext () {
			Given("a DTO object", () => new DtoThing())
				.And("I store 'booga' in it", dto => dto.storedString = "booga");
		}
	}

	public class DtoThing {
		public string storedString;
	}

	public class a_thing : Context<object> {
		public override void SetupContext () {
			Given("an object", () => new object());
		}
	}
}
