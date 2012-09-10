#region License
// 
//  Copyright 2012 Steven Thuriot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
#endregion
using System;
using System.Reflection;

namespace NuGet.BuildAndDeploy
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0 || args[0] == "/?")
			{
				ShowHelp();
				return;
			}

			var creator = new NuGetCreator(args);

			try
			{
				creator.Pack();
			}
			catch (ArgumentException ex)
			{
				ShowError(ex.Message);
			}
		}

		private static void ShowHelp()
		{
			Console.Clear();

			Console.WriteLine();
			Console.WriteLine("  Help for the NuGet Build and Deploy script");
			Console.WriteLine("  ------------------------------------------");
			Console.WriteLine();
			Console.WriteLine("  /OutPutDir [dir]");
			Console.WriteLine("Sets the root of the to be packed files. Required.");
			Console.WriteLine();
			Console.WriteLine("  /dll [path]");
			Console.WriteLine("Sets the location of the dll in the output dir. The path is relative in relation to the outputdir. Required.");
			Console.WriteLine();
			Console.WriteLine("  /id [Name]");
			Console.WriteLine("Sets the package, NuSpec and NuGet file names. Default it uses the assembly info.");
			Console.WriteLine();
			Console.WriteLine("  /authors [Author1 Author2]");
			Console.WriteLine("Sets the authors. Default it uses the assembly info.");
			Console.WriteLine();
			Console.WriteLine("  /owners [owner1 owner2]");
			Console.WriteLine("Sets the owners. Default it uses the assembly info, same as authors.");
			Console.WriteLine();
			Console.WriteLine("  /licenseUrl [http://example.com]");
			Console.WriteLine("Sets the license Url.");
			Console.WriteLine();
			Console.WriteLine("  /projectUrl [http://example.com]");
			Console.WriteLine("Sets the project Url.");
			Console.WriteLine();
			Console.WriteLine("  /iconUrl [http://example.com/icon.ico]");
			Console.WriteLine("Sets the icon.");
			Console.WriteLine();
			Console.WriteLine("  /requireLicenseAcceptance [true|false]");
			Console.WriteLine("Default: false.");
			Console.WriteLine();
			Console.WriteLine("  /description [text]");
			Console.WriteLine("Sets the description. Default it uses the assembly info.");
			Console.WriteLine();
			Console.WriteLine("  /tags [text]");
			Console.WriteLine("Sets the tags.");
			Console.WriteLine();
			Console.WriteLine("  /dependencies [\"Dependency1 1.0\" \"Dependency2 2.0\"]");
			Console.WriteLine("Sets the dependencies.");
			Console.WriteLine();
			Console.WriteLine("  /updateNuGet [true|false]");
			Console.WriteLine("Will try to update NuGet to the latest version if true. Default: false.");
			Console.WriteLine();
			Console.WriteLine("  /?");
			Console.WriteLine("Shows help.");
			Console.WriteLine();
			Console.WriteLine();

			Console.ReadKey();
		}

		private static void ShowError(string message)
		{
			Console.WriteLine();
			Console.WriteLine("  The NuGet Build and Deploy script");
			Console.WriteLine("  ---------------------------------");
			Console.WriteLine();
			Console.WriteLine("  An error has occurred. Please check if you supplied all the needed parameters.");
			Console.Write("  Error message: ");
			Console.WriteLine(message);
			Console.WriteLine();
			Console.WriteLine(string.Format("  Type \"{0} /?\" (without the quotes) for more info.", Assembly.GetExecutingAssembly().ManifestModule.Name));
			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
