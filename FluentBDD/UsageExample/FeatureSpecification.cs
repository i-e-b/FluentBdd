using FluentBDD;

namespace CustomerFeatures {

	/// <summary>
	/// This layout doesn't do any production code testing, but allows us to map out
	/// the features and scenarios we intend to write.
	/// These behaviours DO result in NUnit tests.
	/// 
	/// Specific scenario "CoveredBy"s allow us the ReSharper-drive the tests, but we
	/// can subsequently remove them to make the behaviours less fragile.
	/// </summary>
	[FeatureSet("High level feature behaviour examples")]
	public class FeatureExamples : Feature {
		public const string Ivor = "bowling alley owner Ivor Biggin";
		public const string InventCorp = "equipment partner InventCorp";
		

		/// <summary>
		/// A behaviour with specific scenarios required for the test to pass.
		/// Each scenario inside each "CoveredBy" is a unit test.
		/// Fails unless given type is a 'Feature' and the expressions given are Scenario types.
		/// 
		/// This is the way you'd start to write your scenarios outside-in.
		/// </summary>
		public Feature very_specific_behaviour =
			For(Ivor)
				.To("display scores to players")
				.Should("have the system calculate score from pin counting machine (in progress)")
				.CoveredBy<BowlingScores.ScoringConcerns>(
					scoringConcerns => scoringConcerns.I_shouldnt_be_able_to_bowl_when_the_game_is_over,
					scoringConcerns => scoringConcerns.scoring_for_a_series_of_games_played);

		/// <summary>
		/// A behaviour in place. Describes stakeholders and purpose,
		/// with a test of coverage. Fails unless given type is a 'Feature' with at least one scenario.
		/// 
		/// This is the way you'd leave the behaviour once the features are green (this cleanup to be done as part of the refactor cycle)
		/// </summary>
		public Feature a_behaviour =
			For(Ivor)
				.To("display scores to players")
				.Should("have the system calculate score from pin counting machine")
				.CoveredBy<BowlingScores.ScoringConcerns>();

		/// <summary>
		/// Placeholder behaviour. Describes stakeholders and purpose, but
		/// causes a unit test which always gives an 'inconclusive' result.
		/// 
		/// This helps remind that a feature is required, but doesn't make you go into details until you're ready.
		/// </summary>
		public Feature incomplete =
			For(Ivor, InventCorp)
				.To("read pin hits from alley into scoring system")
				.Should("be able to read pin hit serial data")
				.CoverageNotComplete();

		/// <summary>
		/// Here we see a behaviour that covers more than one feature.
		/// There is no hard-and-fast rule as to how many features to include before breaking
		/// a big behaviour into smaller behaviours; just use your best judgement... collaborate!
		/// </summary>
		public Feature more_than_one_subfeature =
			For("a calculator user")
				.To("do basic math")
				.Should("be able to create a calculator which can add and subtract")
					.CoveredBy<CalculatorConcerns.Creation>()
					.CoveredBy<CalculatorConcerns.Addition>()
					.CoveredBy<CalculatorConcerns.Subtraction>();

	}
	
	[FeatureSet("Drill down")]
	public class drillDown:Feature {
		public Feature drill_down = 
			For("Programmers using FluentBDD")
				.To("successfully practice outside-in development")
				.Should("allow features to be covered by other features")
				.CoveredBy<detailFeature>();
	}
	
	[FeatureSet("Detail features")]
	public class detailFeature:Feature {}
	[Behaviours("Dummy behaviour")]
	public class dummyBehaviour: Behaviours{}
	
}
