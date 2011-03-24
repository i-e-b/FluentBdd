using System;
using System.Collections.Generic;

namespace UsageExample {
	public class GameScorer {
		private readonly List<int> throws = new List<int>(21);

		public int ScoreGame () {
			if (throws.Count > 21) throw new ArgumentException("Too many throws");
			while (throws.Count < 20) { throws.Add(0); }
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
			throws.Add(pinsIHitInThisBowlingThrow);
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