using FluentBDD;

namespace UsageExample {
	/// <summary>
	/// Concept: have a sub-interface on "Then" that hides the need to have
	/// a big (subject, result, values) lambda expression, and that means
	/// you don't get a ton of assertion extension methods messing up intellisense.
	/// </summary>
	[Behaviour("Smart assertions")]
	public class SmartAssertions : Behaviours {

		// no expectations
		public Scenario test1 =
			Given(() => Context.Of<a_thing>())
				.When("I return 10", o => 10)
				.Then("the result should be 10", the => the.result.should.equal(10))
				.Then("the result should not be 11", the => the.result.should.not.equal(11));

		public Scenario test2 =
			Given(() => Context.Of<a_dto>())
				.When("I do nothing", o => { })
				.Then("the DTO should be storing 'booga'", _ => _.subject(s => s.storedString).should.equal("booga"));

		// using expectations
		public Scenario test3 =
			Given(() => Context.Of<a_dto>())
				.When("I return 'wogga'", o => "wogga")
				.Using<some_expectations>()
				.Then("the returned string should be the same as expectation", _ => _.result.should.match.expectation(v => v.aValue));

		public Scenario test4 =
			Given(() => Context.Of<a_dto>())
				.When("I do nothing", o => { })
				.Using<some_expectations>()
				.Then("the DTO should be storing different string to expectation", _ => _.subject(s => s.storedString).should.not.match.expectation(v => v.aValue));

		// comparing result to subject
		public Scenario test5 =
			Given(() => Context.Of<a_dto>())
				.When("I return 'booga'", o => "booga")
				.Then("the DTO should be same as result", _ => _.result.should.match.subject(s => s.storedString));

		public Scenario test6 =
			Given(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("I return 'wogga'", (o, e) => e.Values.aValue)
				.Then("the DTO should be storing different string to result", _ => _.result.should.not.match.subject(s => s.storedString));

		// comparing subject to subject
		public Scenario test7 =
			Given(() => Context.Of<a_dto>())
				.When("I return 'booga'", o => "booga")
				.Then("subject string should be equal to itself", _ => _.subject(s => s.storedString).should.match.subject(s => s.storedString));

		public Scenario test8 =
			Given(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("I return 'wogga'", (o, e) => e.Values.aValue)
				.Then("subject string should be equal to itself", _ => _.subject(s => s.storedString).should.match.subject(s => s.storedString));


		// note: there is not "expectation.should..." on purpose. We don't test our expectations!

		// ignore
		public Scenario test9 =
			Given(() => Context.Of<a_thing>())
				.When("ignoring test", o => { })
				.Then("should be ignored", it => it.should_be_ignored);

		// true/false result
		public Scenario test10 =
			Given(() => Context.Of<a_thing>())
				.When("testing result for true", o => true)
				.Then("should be true", the => the.result.should.be._true);

		public Scenario test11 =
			Given(() => Context.Of<a_thing>())
				.When("testing result for true", o => true)
				.Then("should not be false", the => the.result.should.not.be._false);

		// true/false subject
		public Scenario test12 =
			Given(() => Context.Of<a_dto>())
				.When("testing subject for true", o => { o.boolValue = true; })
				.Then("should be true", the => the.subject(s => s.boolValue).should.be._true);

		public Scenario test13 =
			Given(() => Context.Of<a_dto>())
				.When("testing result for true", o => { o.boolValue = true; })
				.Then("should be true", the => the.subject(s => s.boolValue).should.not.be._false);

		// same as: result->value
		private const string a_string = "Hello!";
		public Scenario test14 =
			Given(() => Context.Of<a_dto>())
				.When("testing result with same instance", o => a_string)
				.Then("should be same instance", the => the.result.should.be.the_same._as(a_string));

		// same as: result->expectation
		public Scenario test15 =
			Given(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("testing result with same instance", (o, e) => e.Values.aValue)
				.Then("should be same instance", the => the.result.should.be.the_same.as_expectation(e => e.aValue));
		public Scenario test16 =
			Given(() => Context.Of<a_dto>())
				.Using<some_expectations>()
				.When("testing result with different instance", (o, e) => a_string)
				.Then("should not be same instance", the => the.result.should.not.be.the_same.as_expectation(e => e.aValue));

		// same as: result->subject
		public Scenario test17 =
			Given(() => Context.Of<a_dto>())
				.When("testing result with same instance", s => s.storedString)
				.Then("should be same instance", the => the.result.should.be.the_same.as_subject(s => s.storedString));

		// same as: subject->value
		public Scenario test18 =
			Given(() => Context.Of<a_dto>())
				.When("testing result with same instance injected into subject", o => { o.objectValue = a_string; })
				.Then("should be same instance", the => the.subject(s=>s.objectValue).should.be.the_same._as(a_string));

		// same as: subject->expectation
		public Scenario test19 =
			Given(() => Context.Of<a_dto>())
				.When("testing result with different instance", o => o.storedString = a_string)
				.Using<some_expectations>()
				.Then("should not be same instance", the => the.subject(s=>s.storedString).should.not.be.the_same.as_expectation(e=>e.aValue));

		// same as: subject->subject
		public Scenario test20 =
			Given(() => Context.Of<a_dto>())
				.When("testing string against itself", o => o.storedString = a_string)
				.Using<some_expectations>()
				.Then("should be same instance", the => the.subject(s=>s.storedString).should.be.the_same.as_subject(s=>s.storedString));

		// Try this syntax? (first with auto description, second without)
		//      .Then().subject(s=>s.storedString).should.be.the_same.as_subject(s=>s.storedString);
		// or   .Then("should be same instance").subject(s=>s.storedString).should.be.the_same.as_subject(s=>s.storedString)
		// smart assertion is given the source scenario, and passes it back from assertion methods.
		// build a set of description, expected lambda, actual lambda, assertion.


		// greater than: result->value
		// greater than: result->expectation
		// greater than: result->subject
		// greater than: subject->value
		// greater than: subject->expectation
		// greater than: subject->subject

		// greater or equal: result->value
		// greater or equal: result->expectation
		// greater or equal: result->subject
		// greater or equal: subject->value
		// greater or equal: subject->expectation
		// greater or equal: subject->subject


		// todo: all the other assertions in old-style assertions.
		// ignore, be true, be false, be same as, greater than, greater or equal to,
		// less than, less or equal to, be null, empty (string), 
		// instance of <T>, instance of (Type), contain (string), contain (enumerable->object),
		// contain (enumerable-> (T->bool)), contain same elements as (enumerable->enumerable),
		// contain same sequence as (enumerable->enumerable), 
	}


	public class some_expectations : IProvide<some_expectations> {
		public string aValue;

		public some_expectations[] Data () {
			return new[] {
				new some_expectations {aValue = "wogga"}
			};
		}

		public string StringRepresentation () {
			return "string = " + aValue;
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
		public object objectValue;
		public string storedString;
		public bool boolValue;
	}

	public class a_thing : Context<object> {
		public override void SetupContext () {
			Given("an object", () => new object());
		}
	}
}
