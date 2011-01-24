using System;
using System.Collections.Generic;

namespace UsageExample {
	/// <summary>
	/// Dummy subject class.
	/// </summary>
	public class Calculator {
		private readonly IDoMath doMath;
		private readonly Stack<int> stack = new Stack<int>();

		public Calculator() {
			doMath = new DoMath();
		}

		public Calculator(IDoMath doMath) {
			if (doMath == null) throw new ArgumentException("An adder must be provided");
			this.doMath = doMath;
		}

		public void Press (int i) {
			stack.Push(i);
		}
		public int Add () {
			var r = doMath.Add(stack.Pop(), stack.Pop());
			stack.Push(r);
			return r;
		}
		public int Readout() {
			return stack.Peek();
		}

		public int Subtract () {
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