using System;
using System.IO;
using FluentBDD;

namespace UsageExample {
	[Behaviour("Contexts with tear-downs")]
	public class TearDown : Behaviours {
		public Scenario tear_downs =
			ProvedBy<values_of_files_to_create>()
				.Given<FileCreator, teardown_context>()
				.When("I call the create file method", (s, e) => s.CreateFile())
				.Then("file should exist").check(p => File.Exists(p.FileName)).should_be_true
				.Then("should tear down for each test case").check(p => File.Exists(p.FileName)).should_be_true
				.Then("these tests would fail if teardown wasn't being called").check(p => File.Exists(p.FileName)).should_be_true;
	}


	public class values_of_files_to_create:IProvide<values_of_files_to_create> {
		public string FileName;

		public values_of_files_to_create[] Data () {
			return new[] {
				new values_of_files_to_create {FileName = @"C:\temp\fbdd_example_1.txt"},
				new values_of_files_to_create {FileName = @"C:\temp\fbdd_example_2.txt"}
			};
		}

		public string StringRepresentation() {return FileName;}
	}

	public class teardown_context : Context<FileCreator>, IUse<values_of_files_to_create> {
		public values_of_files_to_create Values { get; set; }
		public override void SetupContext() {
			Given("a file creator with a file name", () => new FileCreator(Values.FileName));
		}

		public override void TearDown () {
			if (File.Exists(Values.FileName)) File.Delete(Values.FileName);
			else throw new Exception("Expected to remove \"" + Values.FileName + "\" but it wasn't there.");
		}
	}
	public class file_creator_without_teardown:teardown_context {
		public override void TearDown () {}
	}

	public class FileCreator {
		private readonly string fileName;

		public FileCreator(string fileName) {
			this.fileName = fileName;
		}

		public void CreateFile() {
			if (!File.Exists(fileName)) File.WriteAllText(fileName, "This is an example file that should be deleted by FluentBDD's tear-down process");
			else throw new Exception("Expected to write \"" + fileName + "\" but it was already there");
		}
	}
}
