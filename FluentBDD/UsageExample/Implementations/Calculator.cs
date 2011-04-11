using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UsageExample {
	public class SubCalculator {
		protected int display; // just here to test hierarchy flattening in tests.
	}

	/// <summary>
	/// Dummy subject class.
	/// </summary>
	[DataContract, Serializable]
	public class Calculator : SubCalculator {
		[DataMember]
		private readonly IDoMath doMath;

		[DataMember(Name="Stack")]
		private readonly Stack<int> stack = new Stack<int>();

		public Calculator() {
			doMath = new DoMath();
		}

		public Calculator(IDoMath doMath) {
			if (doMath == null) throw new ArgumentException("An math delegate must be provided");
			this.doMath = doMath;
		}

		public void Press (int i) {
			stack.Push(i);
		}
		public int Add () {
			if (stack.Count < 2) throw new InvalidOperationException("Stack empty.");
			var r = doMath.Add(stack.Pop(), stack.Pop());
			stack.Push(r);
			return r;
		}
		public int Readout() {
			return stack.Peek();
		}

		public int Subtract () {
			if (stack.Count < 2) throw new InvalidOperationException("Stack empty.");
			var b = stack.Pop();
			var a = stack.Pop();
			var r = doMath.Subtract(a, b);
			stack.Push(r);
			return r;
		}
	}

	public interface IDoMath {
		int Add (int a, int b);
		int Subtract (int a, int b);
	}

	class DoMath : IDoMath {
		public int Add(int a, int b) {
			return a + b;
		}
		public int Subtract (int a, int b) {
			return a - b;
		}
	}
}