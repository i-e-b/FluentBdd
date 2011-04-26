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
		private readonly Scenario<TSubject, TResult, TProofType, TProofSource> scen;
		public SmartAssertionBase (string description, Scenario<TSubject, TResult, TProofType, TProofSource> Scen) {
			this.description = description;
			scen = Scen;
		}

		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> subject { get { return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => s); } }
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> subject_part (Func<TSubject, object> selector) {
			return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => selector(s));
		}
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> result { get { return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => r); } }
		public SmartAssertion<TSubject, TResult, TProofType, TProofSource> result_part (Func<TResult, object> selector) {
			return new SmartAssertion<TSubject, TResult, TProofType, TProofSource>(description, scen, (s, r, v) => selector(r));
		}

		public Scenario<TSubject, TResult, TProofType, TProofSource> should_throw (Exception ex) {
			return scen.Then(description, p => ex);
		}

		public Scenario<TSubject, TResult, TProofType, TProofSource> check_proof (Action<TProofType> check) {
			return scen.Then(description, (s, r, p) => check(p));
		}

		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_ignored {
			get { return scen.Then(description, (s, r, p) => Assert.Ignore("Ignored")); }
		}
	}

	/// <summary>
	/// Uniary assertions, and the first halfs of binary assertions
	/// </summary>
	public class SmartAssertion<TSubject, TResult, TProofType, TProofSource> where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		internal readonly string description;
		internal readonly Scenario<TSubject, TResult, TProofType, TProofSource> scen;
		internal readonly Func<TSubject, TResult, TProofType, object> actualSelector;

		public SmartAssertion(string description, Scenario<TSubject, TResult, TProofType, TProofSource> scen, Func<TSubject, TResult, TProofType, object> actualSelector) {
			this.description = description;
			this.scen = scen;
			this.actualSelector = actualSelector;
		}

		#region Uniary
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_false {
			get { return scen.Then(description, (s, r, p) => Assert.IsFalse((bool) actualSelector(s, r, p))); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_true {
			get { return scen.Then(description, (s, r, p) => Assert.IsTrue((bool) actualSelector(s, r, p))); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_null {
			get { return scen.Then(description, (s, r, p) => Assert.IsNull(actualSelector(s, r, p))); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_not_be_null {
			get { return scen.Then(description, (s, r, p) => Assert.IsNotNull(actualSelector(s, r, p))); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_empty {
			get {
				return scen.Then(description, (s, r, p) => {
				                              	var obj = actualSelector(s, r, p);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.False);
				                              	else Assert.That(obj, Is.Empty);
				                              });
			}
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_not_be_empty {
			get {
				return scen.Then(description, (s, r, p) => {
				                              	var obj = actualSelector(s, r, p);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.True);
				                              	else Assert.That(obj, Is.Not.Empty);
				                              });
			}
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_be_instance_of<T> () {
			return scen.Then(description, (s, r, p) => Assert.That(actualSelector(s, r, p), Is.InstanceOf(typeof (T))));
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> should_not_be_instance_of<T>() {
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
	/// Binary assertions' second halfs
	/// </summary>
	public class SmartAssertionBinary<TSubject, TResult, TProofType, TProofSource> where TProofSource : class, TProofType, IProvide<TProofType>, new() {
		private readonly SmartAssertion<TSubject, TResult, TProofType, TProofSource> src;
		private readonly Action<object, object> assertion;

		public SmartAssertionBinary(SmartAssertion<TSubject, TResult, TProofType, TProofSource> SmartAssertion, Action<object, object> assertion) {
			src = SmartAssertion;
			this.assertion = assertion;
		}

		public Scenario<TSubject, TResult, TProofType, TProofSource> value (object value) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), value));
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> subject {
			get { return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), s)); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> subject_part (Func<TSubject, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(s)));
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> result {
			get { return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), r)); }
		}
		public Scenario<TSubject, TResult, TProofType, TProofSource> result_part (Func<TResult, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(r)));
		}

		public Scenario<TSubject, TResult, TProofType, TProofSource> proof (Func<TProofType, object> selector) {
			return src.scen.Then(src.description, (s, r, p) => assertion(src.actualSelector(s, r, p), selector(p)));
		}
	}
	#endregion

	#region Assertions for Scenarios without proofs
	public class SmartAssertionBase<TSubject, TResult> {
		private readonly string description;
		private readonly Scenario<TSubject, TResult> scen;
		public SmartAssertionBase (string description, Scenario<TSubject, TResult> Scen) {
			this.description = description;
			scen = Scen;
		}

		public SmartAssertion<TSubject, TResult> subject { get { return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => s); } }
		public SmartAssertion<TSubject, TResult> subject_part (Func<TSubject, object> selector) {
			return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => selector(s));
		}
		public SmartAssertion<TSubject, TResult> result { get { return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => r); } }
		public SmartAssertion<TSubject, TResult> result_part (Func<TResult, object> selector) {
			return new SmartAssertion<TSubject, TResult>(description, scen, (s, r) => selector(r));
		}

		public Scenario<TSubject, TResult> should_throw (Exception ex) {
			return scen.Then(description, () => ex);
		}
		public Scenario<TSubject, TResult> should_be_ignored {
			get { return scen.Then(description, (s, r) => Assert.Ignore("Ignored")); }
		}
	}
	
	public class SmartAssertion<TSubject, TResult> {
		internal readonly string description;
		internal readonly Scenario<TSubject, TResult> scen;
		internal readonly Func<TSubject, TResult, object> actualSelector;

		public SmartAssertion(string description, Scenario<TSubject, TResult> scen, Func<TSubject, TResult, object> actualSelector) {
			this.description = description;
			this.scen = scen;
			this.actualSelector = actualSelector;
		}

		#region Uniary
		public Scenario<TSubject, TResult> should_be_false {
			get { return scen.Then(description, (s, r) => Assert.IsFalse((bool) actualSelector(s, r))); }
		}
		public Scenario<TSubject, TResult> should_be_true {
			get { return scen.Then(description, (s, r) => Assert.IsTrue((bool) actualSelector(s, r))); }
		}
		public Scenario<TSubject, TResult> should_be_null {
			get { return scen.Then(description, (s, r) => Assert.IsNull(actualSelector(s, r))); }
		}
		public Scenario<TSubject, TResult> should_not_be_null {
			get { return scen.Then(description, (s, r) => Assert.IsNotNull(actualSelector(s, r))); }
		}
		public Scenario<TSubject, TResult> should_be_empty {
			get {
				return scen.Then(description, (s, r) => {
				                              	var obj = actualSelector(s, r);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.False);
				                              	else Assert.That(obj, Is.Empty);
				                              });
			}
		}
		public Scenario<TSubject, TResult> should_not_be_empty {
			get {
				return scen.Then(description, (s, r) => {
				                              	var obj = actualSelector(s, r);
				                              	if (obj is IEnumerable<object>) Assert.That((obj as IEnumerable<object>).Any(), Is.True);
				                              	else Assert.That(obj, Is.Not.Empty);
				                              });
			}
		}
		public Scenario<TSubject, TResult> should_be_instance_of<T> () {
			return scen.Then(description, (s, r) => Assert.That(actualSelector(s, r), Is.InstanceOf(typeof (T))));
		}
		public Scenario<TSubject, TResult> should_not_be_instance_of<T>() {
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
	
	public class SmartAssertionBinary<TSubject, TResult> {
		private readonly SmartAssertion<TSubject, TResult> src;
		private readonly Action<object, object> assertion;

		public SmartAssertionBinary(SmartAssertion<TSubject, TResult> SmartAssertion, Action<object, object> assertion) {
			src = SmartAssertion;
			this.assertion = assertion;
		}
		
		public Scenario<TSubject, TResult> value (object value) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), value));
		}
		public Scenario<TSubject, TResult> subject {
			get { return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), s)); }
		}
		public Scenario<TSubject, TResult> subject_part (Func<TSubject, object> selector) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), selector(s)));
		}
		public Scenario<TSubject, TResult> result {
			get { return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), r)); }
		}
		public Scenario<TSubject, TResult> result_part (Func<TResult, object> selector) {
			return src.scen.Then(src.description, (s, r) => assertion(src.actualSelector(s, r), selector(r)));
		}
	}
	#endregion
}
