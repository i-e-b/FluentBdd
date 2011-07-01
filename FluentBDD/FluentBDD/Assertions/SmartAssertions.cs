using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FluentBDD.Assertions {
	#region Assertions for Scenarios with proofs
	/// <summary>
	/// Base for assertions. These select the 'actual' values.
	/// Types mirror those of the source scenario.
	/// NB : Proofs are never part of the 'actual', this is intentional.
	/// </summary>
	public class SmartAssertionBase<TSubject, TResult, TProofType, TProofSource> where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		private readonly string description;
		private readonly Behaviour<TSubject, TResult, TProofType, TProofSource> scen;
		public SmartAssertionBase (string description, Behaviour<TSubject, TResult, TProofType, TProofSource> Scen) {
			this.description = description;
			scen = Scen;
		}

		/// <summary>Test that the scenario's subject matches an expectation</summary>
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> subject { get { return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => s); } }
		
		/// <summary> Select something from the subject to test against an expectation </summary>
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> subject_part (Func<TSubject, object> selector) {
			return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => selector(s));
		}
		/// <summary> Test that the result returned by the "When" clause matches an expectation </summary>
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> result { get { return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => r); } }
		
		/// <summary> Select something from the result returned by the "When" clause to test against an expectation  </summary>
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> result_part (Func<TResult, object> selector) {
			return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => selector(r));
		}

		/// <summary> Test that the scenario throws an exception of a matching type and message. </summary>
		/// <param name="ex">An example exception to match against. To ignore the message in tests, pass an example exception with an empty message string</param>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_throw (Exception ex) {
			return scen.Then(description, p => ex);
		}

		/// <summary> Return a value based on proof values to test against an expectation </summary>
		/// <param name="selector">A function on the scenario proofs; e.g. ".check(p=>File.Exists(p.fileName)."</param>
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> check (Func<TProofType, object> selector) {
			return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => selector(v));
		}

		/// <summary> Call a method on the scenario proof, which should make it's own assertions </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> check_proof (Action<TProofType> check) {
			return scen.Then(description, (s, r, p) => check(p));
		}

		/// <summary> Ignore a single "Then" clause in a scenario </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_ignored {
			get { return scen.Then(description, (s, r, p) => Assert.Ignore("Ignored")); }
		}
	}

	/// <summary>
	/// Uniary assertions, and the "actual" part of binary assertions
	/// </summary>
	public class SmartAssertion<TSubject, TResult, TProofType, TProofSource> where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		internal readonly string description;
		internal readonly Behaviour<TSubject, TResult, TProofType, TProofSource> scen;
		internal readonly Func<TSubject, TResult, TProofType, object> actualSelector;

		public SmartAssertion(string description, Behaviour<TSubject, TResult, TProofType, TProofSource> scen, Func<TSubject, TResult, TProofType, object> actualSelector) {
			this.description = description;
			this.scen = scen;
			this.actualSelector = actualSelector;
		}

		#region Uniary
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_false {
			get { return scen.Then(description, (s, r, p) => Assert.IsFalse((bool) actualSelector(s, r, p))); }
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_true {
			get { return scen.Then(description, (s, r, p) => Assert.IsTrue((bool) actualSelector(s, r, p))); }
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_null {
			get { return scen.Then(description, (s, r, p) => Assert.IsNull(actualSelector(s, r, p))); }
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_not_be_null {
			get { return scen.Then(description, (s, r, p) => Assert.IsNotNull(actualSelector(s, r, p))); }
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_empty {
			get {
				return scen.Then(description, (s, r, p) => {
				                              	var obj = actualSelector(s, r, p);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.False);
				                              	else Assert.That(obj, Is.Empty);
				                              });
			}
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_not_be_empty {
			get {
				return scen.Then(description, (s, r, p) => {
				                              	var obj = actualSelector(s, r, p);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.True);
				                              	else Assert.That(obj, Is.Not.Empty);
				                              });
			}
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_be_instance_of<T> () {
			return scen.Then(description, (s, r, p) => Assert.That(actualSelector(s, r, p), Is.InstanceOf(typeof (T))));
		}
		public Behaviour<TSubject, TResult, TProofType, TProofSource> should_not_be_instance_of<T>() {
			return scen.Then(description, (s, r, p) => Assert.That(actualSelector(s, r, p), Is.Not.InstanceOf(typeof(T))));
		}
		#endregion

		#region Binary
		#region object equality
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.AreEqual(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_not_be_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.AreNotEqual(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_same_as {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.AreSame(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_not_be_same_as {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.AreNotSame(expected, actual));}
		}
		#endregion
		#region numerical equality
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_greater_than {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.That(actual, Is.GreaterThan(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_greater_than_or_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.That(actual, Is.GreaterThanOrEqualTo(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_less_than {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.That(actual, Is.LessThan(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_less_than_or_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected)=> Assert.That(actual, Is.LessThanOrEqualTo(expected)));}
		}
		#endregion
		#region string equality
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_contain_substring {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>Assert.That(actual, Is.StringContaining((string)expected)));
			}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_not_contain_substring {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>Assert.That(actual, Is.Not.StringContaining((string)expected)));
			}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_be_case_insensitive_equal_to {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>Assert.That(actual, Is.EqualTo(expected).IgnoreCase));
			}
		}
		#endregion
		#region enumeration equality
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_contain_element {
			get { return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) => Assert.That(((IEnumerable<object>)actual).Contains(expected), Is.True, "should contain element "+expected) ); }
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_not_contain_element {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>Assert.That(((IEnumerable<object>)actual).Contains(expected), Is.False, "should not contain element " + expected));
			}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_contain_match<T>(Func<T, bool> predicate) {
			return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) => 
				Assert.That(((IEnumerable<T>)actual).Any(predicate), Is.True, "should contain element matching a predicate")
				
				); 
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_not_contain_match<T> (Func<T, bool> predicate) {
			return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>
				Assert.That(((IEnumerable<T>)actual).Any(predicate), Is.False, "should not contain element matching a predicate")

				);
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_contain_the_same_elements_as {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>
					Assert.That(((IEnumerable<object>)actual).All(((IEnumerable<object>)expected).Contains)
					&& ((IEnumerable<object>)expected).All(((IEnumerable<object>)actual).Contains), Is.True, actual + " should contain the same elements as " + expected)
				                                                                                   );
			}
		}
		public SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> should_contain_the_same_sequence_as {
			get {
				return new SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource>(this, (actual, expected) =>
					Assert.That(((IEnumerable<object>)actual).SequenceEqual((IEnumerable<object>)expected), Is.True, actual + " should be the same sequence as " + expected)
																								   );
			}
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Binary assertions' "expected" parts
	/// </summary>
	public class SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		private readonly SmartAssertion<TSubject, TResult, TProofType, TProofSource> src;
		private readonly Action<object, object> assertion;

		public SmartAssertionBinary(SmartAssertion<TSubject, TResult, TProofType, TProofSource> SmartAssertion, Action<object, object> assertion) {
			src = SmartAssertion;
			this.assertion = assertion;
		}

		/// <summary> Expect a specific value </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> value (object value) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), value));
		}
		/// <summary> Expect the scenario's subject object </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> subject {
			get { return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), s)); }
		}
		/// <summary> Select part of the scenario's subject as the test's expectation </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> subject_part (Func<TSubject, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(s)));
		}
		/// <summary> Expect the result returned by the "When" clause </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> result {
			get { return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), r)); }
		}
		/// <summary> Select part of the result returned by the "When" clause as the test's expectation</summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> result_part (Func<TResult, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(r)));
		}

		/// <summary> Select a value from the scenario's proof as the test's expectation </summary>
		public Behaviour<TSubject, TResult, TProofType, TProofSource> proof (Func<TProofType, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(p)));
		}
	}
	#endregion

	#region Assertions for Scenarios without proofs
	/// <summary>
	/// Base for assertions. These select the 'actual' values.
	/// Types mirror those of the source scenario.
	/// </summary>
	public class SmartAssertionBase<TSubject, TResult> {
		private readonly string description;
		private readonly Behaviour<TSubject, TResult> scen;
		public SmartAssertionBase (string description, Behaviour<TSubject, TResult> Scen) {
			this.description = description;
			scen = Scen;
		}

		/// <summary>Test that the scenario's subject matches an expectation</summary>
		public SmartAssertion<TSubject, TResult> subject { get { return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => s); } }
		
		/// <summary> Select something from the subject to test against an expectation </summary>
		public SmartAssertion<TSubject, TResult> subject_part (Func<TSubject, object> selector) {
			return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => selector(s));
		}
		/// <summary> Test that the result returned by the "When" clause matches an expectation </summary>
		public SmartAssertion<TSubject, TResult> result { get { return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => r); } }

		/// <summary> Select something from the result returned by the "When" clause to test against an expectation  </summary>
		public SmartAssertion<TSubject, TResult> result_part (Func<TResult, object> selector) {
			return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => selector(r));
		}

		/// <summary> Test that the scenario throws an exception of a matching type and message. </summary>
		/// <param name="ex">An example exception to match against. To ignore the message in tests, pass an example exception with an empty message string</param>
		public Behaviour<TSubject, TResult> should_throw (Exception ex) {
			return scen.Then(description, () => ex);
		}

		/// <summary> Ignore a single "Then" clause in a scenario </summary>
		public Behaviour<TSubject, TResult> should_be_ignored {
			get { return scen.Then(description, (s, r) => Assert.Ignore("Ignored")); }
		}
	}

	/// <summary>
	/// Uniary assertions, and the "actual" part of binary assertions
	/// </summary>
	public class SmartAssertion<TSubject, TResult> {
		internal readonly string description;
		internal readonly Behaviour<TSubject, TResult> scen;
		internal readonly Func<TSubject, TResult, object> actualSelector;

		public SmartAssertion(string description, Behaviour<TSubject, TResult> scen, Func<TSubject, TResult, object> actualSelector) {
			this.description = description;
			this.scen = scen;
			this.actualSelector = actualSelector;
		}

		#region Uniary
		public Behaviour<TSubject, TResult> should_be_false {
			get { return scen.Then(description, (s, r) => Assert.IsFalse((bool) actualSelector(s, r))); }
		}
		public Behaviour<TSubject, TResult> should_be_true {
			get { return scen.Then(description, (s, r) => Assert.IsTrue((bool) actualSelector(s, r))); }
		}
		public Behaviour<TSubject, TResult> should_be_null {
			get { return scen.Then(description, (s, r) => Assert.IsNull(actualSelector(s, r))); }
		}
		public Behaviour<TSubject, TResult> should_not_be_null {
			get { return scen.Then(description, (s, r) => Assert.IsNotNull(actualSelector(s, r))); }
		}
		public Behaviour<TSubject, TResult> should_be_empty {
			get {
				return scen.Then(description, (s, r) => {
				                              	var obj = actualSelector(s, r);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.False);
				                              	else Assert.That(obj, Is.Empty);
				                              });
			}
		}
		public Behaviour<TSubject, TResult> should_not_be_empty {
			get {
				return scen.Then(description, (s, r) => {
				                              	var obj = actualSelector(s, r);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.True);
				                              	else Assert.That(obj, Is.Not.Empty);
				                              });
			}
		}
		public Behaviour<TSubject, TResult> should_be_instance_of<T> () {
			return scen.Then(description, (s, r) => Assert.That(actualSelector(s, r), Is.InstanceOf(typeof (T))));
		}
		public Behaviour<TSubject, TResult> should_not_be_instance_of<T>() {
			return scen.Then(description, (s, r) => Assert.That(actualSelector(s, r), Is.Not.InstanceOf(typeof(T))));
		}
		#endregion

		#region Binary
		#region object equality
		public SmartAssertionBinary<TSubject, TResult> should_be_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.AreEqual(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_not_be_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.AreNotEqual(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_be_same_as {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.AreSame(expected, actual));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_not_be_same_as {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.AreNotSame(expected, actual));}
		}
		#endregion
		#region numerical equality
		public SmartAssertionBinary<TSubject, TResult> should_be_greater_than {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.That(actual, Is.GreaterThan(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_be_greater_than_or_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.That(actual, Is.GreaterThanOrEqualTo(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_be_less_than {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.That(actual, Is.LessThan(expected)));}
		}
		public SmartAssertionBinary<TSubject, TResult> should_be_less_than_or_equal_to {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected)=> Assert.That(actual, Is.LessThanOrEqualTo(expected)));}
		}
		#endregion
		#region string equality
		public SmartAssertionBinary<TSubject, TResult> should_contain_substring {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>Assert.That(actual, Is.StringContaining((string)expected)));
			}
		}
		public SmartAssertionBinary<TSubject, TResult> should_not_contain_substring {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>Assert.That(actual, Is.Not.StringContaining((string)expected)));
			}
		}
		public SmartAssertionBinary<TSubject, TResult> should_be_case_insensitive_equal_to {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>Assert.That(actual, Is.EqualTo(expected).IgnoreCase));
			}
		}
		#endregion
		#region enumeration equality
		public SmartAssertionBinary<TSubject, TResult> should_contain_element {
			get { return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) => Assert.That(((IEnumerable<object>)actual).Contains(expected), Is.True, "should contain element "+expected) ); }
		}
		public SmartAssertionBinary<TSubject, TResult> should_not_contain_element {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>Assert.That(((IEnumerable<object>)actual).Contains(expected), Is.False, "should not contain element " + expected));
			}
		}
		public SmartAssertionBinary<TSubject, TResult> should_contain_match<T>(Func<T, bool> predicate) {
			return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) => 
				Assert.That(((IEnumerable<T>)actual).Any(predicate), Is.True, "should contain element matching a predicate")
				
				); 
		}
		public SmartAssertionBinary<TSubject, TResult> should_not_contain_match<T> (Func<T, bool> predicate) {
			return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>
				Assert.That(((IEnumerable<T>)actual).Any(predicate), Is.False, "should not contain element matching a predicate")

				);
		}
		public SmartAssertionBinary<TSubject, TResult> should_contain_the_same_elements_as {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>
					Assert.That(((IEnumerable<object>)actual).All(((IEnumerable<object>)expected).Contains)
					&& ((IEnumerable<object>)expected).All(((IEnumerable<object>)actual).Contains), Is.True, actual + " should contain the same elements as " + expected)
				                                                                                   );
			}
		}
		public SmartAssertionBinary<TSubject, TResult> should_contain_the_same_sequence_as {
			get {
				return new SmartAssertionBinary<TSubject, TResult>(this, (actual, expected) =>
					Assert.That(((IEnumerable<object>)actual).SequenceEqual((IEnumerable<object>)expected), Is.True, actual + " should be the same sequence as " + expected)
																								   );
			}
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Binary assertions' "expected" parts
	/// </summary>
	public class SmartAssertionBinary<TSubject, TResult> {
		private readonly SmartAssertion<TSubject, TResult> src;
		private readonly Action<object, object> assertion;

		public SmartAssertionBinary(SmartAssertion<TSubject, TResult> SmartAssertion, Action<object, object> assertion) {
			src = SmartAssertion;
			this.assertion = assertion;
		}

		/// <summary> Expect a specific value </summary>
		public Behaviour<TSubject, TResult> value (object value) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), value));
		}
		/// <summary> Expect the scenario's subject object </summary>
		public Behaviour<TSubject, TResult> subject {
			get { return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), s)); }
		}
		/// <summary> Select part of the scenario's subject as the test's expectation </summary>
		public Behaviour<TSubject, TResult> subject_part (Func<TSubject, object> selector) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), selector(s)));
		}
		/// <summary> Expect the result returned by the "When" clause </summary>
		public Behaviour<TSubject, TResult> result {
			get { return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), r)); }
		}
		/// <summary> Select part of the result returned by the "When" clause as the test's expectation</summary>
		public Behaviour<TSubject, TResult> result_part (Func<TResult, object> selector) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), selector(r)));
		}
	}
	#endregion
}
