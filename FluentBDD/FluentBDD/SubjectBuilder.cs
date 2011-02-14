using System;
using System.Collections.Generic;

namespace FluentBDD {
	public class SubjectBuilder<TSubject> {
		internal string Description;
		internal readonly Func<TSubject> SubjectSource;
		internal List<Action<TSubject>> Mutators;

		public SubjectBuilder (string description, Func<TSubject> createSubject) {
			Description = description;
			SubjectSource = createSubject;
			Mutators = new List<Action<TSubject>>();
		}

		public SubjectBuilder<TSubject> And(string description, Action<TSubject> mutator) {
			Description += " and " + description;
			Mutators.Add(mutator);
			return this;
		}

		internal TSubject Build() {
			var subject = SubjectSource();
			foreach (var mutator in Mutators) {
				mutator(subject);
			}
			return subject;
		}

		public override string ToString () {
			return base.ToString() + "with " + Mutators.Count + " sideffects; Subject looks like " + SubjectSource();
		}
	}
}