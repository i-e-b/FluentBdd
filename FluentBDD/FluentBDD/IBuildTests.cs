using System.Collections.Generic;

namespace FluentBDD {
	internal interface IBuildTests {
		IEnumerable<TestClosure> BuildTests();
	}
}