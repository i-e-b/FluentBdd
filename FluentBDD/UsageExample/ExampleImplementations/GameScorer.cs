using System;

namespace UsageExample {
	public class GameScorer {
		public GameScorer () {
			currentRoll = 0;
		}

		private readonly int[] throws = new int[21];
		private int currentRoll;

		public int ScoreGame () {
			int score = 0;
			int frameIndex = 0;
			for (int frame = 0; frame < 10; frame++) {
				if (isStrike(frameIndex)) {
					score += 10 + strikeBonus(frameIndex);
					frameIndex++;
				} else if (isSpare(frameIndex)) {
					score += 10 + spareBonus(frameIndex);
					frameIndex += 2;
				} else {
					score += sumOfBallsInFrame(frameIndex);
					frameIndex += 2;
				}
			}
			return score;
		}

		public void ThrowBowlingBall (int pinsIHitInThisBowlingThrow) {
			if (currentRoll >= 21) throw new ArgumentException("Game is over");
			throws[currentRoll++] = pinsIHitInThisBowlingThrow;
		}

		private bool isStrike (int frameIndex) {
			return throws[frameIndex] == 10;
		}

		private int sumOfBallsInFrame (int frameIndex) {
			return throws[frameIndex] + throws[frameIndex + 1];
		}

		private int spareBonus (int frameIndex) {
			return throws[frameIndex + 2];
		}

		private int strikeBonus (int frameIndex) {
			return throws[frameIndex + 1] + throws[frameIndex + 2];
		}

		private bool isSpare (int frameIndex) {
			return throws[frameIndex] + throws[frameIndex + 1] == 10;
		}


		// This is temporary crap...
		public void StoreNickName(string nickName) {
			NickName = nickName;
		}

		protected string NickName { get; set; }

		public string GetNickName () {
			return NickName;
		}
	}
}