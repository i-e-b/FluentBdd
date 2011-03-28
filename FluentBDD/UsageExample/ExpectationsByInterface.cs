﻿using System;
using System.Linq;
using FluentBDD;
using FluentBDD.Assertions;
using UsageExample;

/*
 * Showing how to use different expectation/value providers with the same context
 * 
 * The Using<>() clause takes an interface that specifies properties and methods available,
 * and a type which is a IProvide<> of that interface.
 */

// The bowling scores example from SpecFlow
namespace BowlingScores {
	[Behaviour("Score calculation",
		"In order to know my performance",
		"As a player",
		"I want the system to calculate my total score")]
	class ScoringConcerns : Behaviours {
		public Scenario scoring_for_a_series_of_games_played = Proved.By<IGameExpectations, valid_games>()
			.Given<GameScorer, a_game_scorer_that_takes_a_series_of_pin_hits>()
			.When("I score a valid game", (game_scorer, example) => game_scorer.ScoreGame())
			.Then("I should get a final score", (game_scorer, result, values) => result.should_be_equal_to(values.finalScore));

		public Scenario I_shouldnt_be_able_to_bowl_when_the_game_is_over = Proved.By<IGameExpectations, games_with_too_many_throws>()
			.Given<GameScorer, a_game_scorer_that_takes_a_series_of_pin_hits>()
			.When("I score an invalid game", (game_scorer, example) => game_scorer.ScoreGame())
			.ShouldThrow<ArgumentException>()
			.WithMessage("Too many throws");
	}

	// this context takes IGameExpectations as it's values, so can take many providers
	internal class a_game_scorer_that_takes_a_series_of_pin_hits : Context<GameScorer>, IUse<IGameExpectations> {
		public IGameExpectations Values { get; set; }

		public override void SetupContext () {
			Given("I have a 10-pin bowling game scorer", () => new GameScorer())
				.And("I play a game", gs =>
				{
					foreach (var pins_I_hit_in_this_bowling_throw in Values.pinHits) {
						gs.ThrowBowlingBall(pins_I_hit_in_this_bowling_throw);
					}
				});
		}
	}

	// the bare essentials for the context
	internal interface IGameExpectations {
		int finalScore { get; }
		string nickName { get; }
		int[] pinHits { get; }
		string StringRepresentation ();
	}

	// one provider of values/expectations
	internal class valid_games : IGameExpectations, IProvide<IGameExpectations> {
		public int finalScore { get; set; }
		public string nickName { get; set; }
		public int[] pinHits { get; set; }

		private static readonly valid_games[] values = new[] {
			new valid_games {
				nickName = "Gutter game", finalScore = 0, pinHits = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
			},
			new valid_games {
				nickName = "One spare", finalScore = 29, pinHits = new[] {3, 7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}
			},
			new valid_games {
				nickName = "All spares", finalScore = 110, pinHits = new[] {1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1}
			},
			new valid_games {
				nickName = "Beginner's game", finalScore = 43, pinHits = new[] {2, 7, 1, 5, 1, 1, 1, 3, 1, 1, 1, 4, 1, 1, 1, 1, 8, 1, 1, 1, 0}
			},
			new valid_games {
				nickName = "Another beginner's game", finalScore = 40, pinHits = new[] {2, 7, 3, 4, 1, 1, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 1}
			},
			new valid_games {
				nickName = "All Strikes", finalScore = 300, pinHits = new[] {10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10}
			}
		};

		public IGameExpectations[] Data () { return values; }

		public string StringRepresentation () {
			return nickName + ", final score = " + finalScore + " for pin hits " + pinHits.Aggregate("", (a, b) => a + ", " + b).Substring(1);
		}
	}

	// a different, but compatible provider of values/expectations
	internal class games_with_too_many_throws : IGameExpectations, IProvide<IGameExpectations> {
		public int finalScore { get; set; }
		public string nickName { get; set; }
		public int[] pinHits { get; set; }

		private static readonly games_with_too_many_throws[] values = new[] {
			new games_with_too_many_throws {
				nickName = "Too many throws", finalScore = 0, pinHits = new[] {3, 7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 5, 5, 5}
			}
		};

		public IGameExpectations[] Data () { return values; }

		public string StringRepresentation () {
			return nickName + ", final score = " + finalScore + " for pin hits " + pinHits.Aggregate("", (a, b) => a + ", " + b).Substring(1);
		}
	}
}
