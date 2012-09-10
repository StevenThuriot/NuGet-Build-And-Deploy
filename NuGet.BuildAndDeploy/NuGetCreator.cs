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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace NuGet.BuildAndDeploy
{
	public class NuGetCreator
	{
		private readonly string[] _Arguments;
		private FileVersionInfo _FileVersionInfo;
	    private readonly string _WorkingDirectory;
	    private XmlDocument _NuSpecXmlDocument;

	    /// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public NuGetCreator(string[] arguments)
	    {
	        _Arguments = arguments;

	        _WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	        var packageDir = _WorkingDirectory + "\\NuGet Packages";

	        if (_WorkingDirectory != null)
	        {
                if (!Directory.Exists(packageDir))
                {
                    Directory.CreateDirectory(packageDir);
                }

                _WorkingDirectory = packageDir;   
	        }
	    }

	    /// <summary>
		/// Create an up to date NuSpec file and pack it into a NuGet file.
		/// </summary>
		public void Pack()
		{
			var dict = ArgumentParser.ParseMultiValues(_Arguments);ArgumentParser.Parse(_Arguments);

			CheckForRequiredCommands(dict);

            var update = Update(dict);

            if (update)
            {
                Console.WriteLine("Trying to update NuGet...");
                Console.WriteLine();

                StartProcess("NuGet.exe", "update -self");
            }

            if (update && dict.Count == 1)
                return;

			var outputdir = Join(dict["OUTPUTDIR"]);

			if (!outputdir.EndsWith("/") && !outputdir.EndsWith("\\"))
				outputdir = outputdir + "\\";

			var dll = outputdir + Join(dict["DLL"]);
			_FileVersionInfo = FileVersionInfo.GetVersionInfo(dll);

			string nuspec = CreateNuSpec(dict, outputdir);

			PackNuGet(nuspec);
		}

        private static bool Update(IDictionary<string, IList<string>> dict)
        {
            return dict.ContainsKey("UPDATENUGET") &&
                   Join(dict["UPDATENUGET"]).ToUpperInvariant() == Boolean.TrueString.ToUpperInvariant();
        }

		private void PackNuGet(string nuspec)
		{
			Console.WriteLine("Starting to build the NuGet pack...");
			Console.WriteLine();

			StartProcess("NuGet.exe", "pack \"" + nuspec + "\"");
		}

        private string CreateNuSpec(IDictionary<string, IList<string>> dict, string outputdir)
        {
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Generating the NuSpec file for {0} version {1}.", _FileVersionInfo.ProductName, _FileVersionInfo.FileVersion));
            Console.WriteLine();

            _NuSpecXmlDocument = new XmlDocument();

            var declaration = _NuSpecXmlDocument.CreateXmlDeclaration("1.0", null, null);
            _NuSpecXmlDocument.AppendChild(declaration);

            var root = _NuSpecXmlDocument.CreateElement("package");
            root.SetAttribute("xmlns" , "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
            
            var metaData = _NuSpecXmlDocument.CreateElement("metadata");

            AppendElement(metaData, "id", dict, _FileVersionInfo.ProductName);
            
            AppendElement(metaData, "version", _FileVersionInfo.FileVersion);
            
            AppendElement(metaData, "authors", dict, _FileVersionInfo.CompanyName);

            AppendElement(metaData, "owners", dict, _FileVersionInfo.CompanyName);

            AppendElement(metaData, "licenseUrl", dict);
            AppendElement(metaData, "projectUrl", dict);
            AppendElement(metaData, "iconUrl", dict);

            AppendElement(metaData, "requireLicenseAcceptance", dict, "false");

            AppendElement(metaData, "description", dict, _FileVersionInfo.Comments);
            AppendElement(metaData, "tags", dict);

            if (dict.ContainsKey("DEPENDENCIES"))
            {
                var dependencies = _NuSpecXmlDocument.CreateElement("dependencies");
                
                foreach (var dependency in dict["DEPENDENCIES"])
                {
                    var dependencyArray = ReverseString(dependency).Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var version = ReverseString(dependencyArray[0]);
                    var id = ReverseString(dependencyArray[1]);

                    var dependencyElement = _NuSpecXmlDocument.CreateElement("dependency");
                    dependencyElement.SetAttribute("id", id);
                    dependencyElement.SetAttribute("version", version);

                    dependencies.AppendChild(dependencyElement);
                }

                metaData.AppendChild(dependencies);
            }

            root.AppendChild(metaData);
            _NuSpecXmlDocument.AppendChild(root);
            

            Console.WriteLine("Finished generating the NuSpec file succesfully.");
            Console.Write("Writing NuSpec file... ");
            
            var nuspec = string.Format(CultureInfo.CurrentCulture, "{0}{1}.nuspec", outputdir, Join(dict["ID"], "_"));
            _NuSpecXmlDocument.Save(nuspec);

            Console.WriteLine("NuSpec file saved succesfully.");
            Console.WriteLine();

            return nuspec;
        }

	    private void AppendElement(XmlNode metaData, string key, IDictionary<string, IList<string>> dictionary, string defaultValue = null)
        {
            var upperKey = key.ToUpperInvariant();

	        string value;

            if (dictionary.ContainsKey(upperKey))
            {
                value = Join(dictionary[upperKey]);
            }
            else
            {
                value = defaultValue;
                if (!string.IsNullOrEmpty(value) && value.Trim().Length != 0)
                    dictionary.Add(upperKey, new List<string> { value.Replace(' ', '_') });
            }

            AppendElement(metaData, key, value);
        }

        private void AppendElement(XmlNode metaData, string key, string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
                return;

            if (key.ToUpperInvariant() == "ID")
                value = value.Replace(' ', '_'); //Required by NuGet

            var element = _NuSpecXmlDocument.CreateElement(key);
            element.InnerText = value;

            metaData.AppendChild(element);
        }

		private static void CheckForRequiredCommands(IDictionary<string, IList<string>> dict)
		{
            if (Update(dict) && dict.Count == 1) return;

			var requiredKeys = new[] { "OutPutDir", "dll" };

			foreach (var requiredKey in requiredKeys)
			{
				if (!dict.ContainsKey(requiredKey.ToUpperInvariant()))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "/{0} is a required command.", requiredKey));
			}
		}

        private void StartProcess(string filename, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            if (_WorkingDirectory != null)
                processStartInfo.WorkingDirectory = _WorkingDirectory;

            var process = new Process { StartInfo = processStartInfo };

            process.Start();

            Console.WriteLine(process.StandardOutput.ReadToEnd());

            process.WaitForExit();
        }

        private static string Join(IEnumerable<string> enumeration, string delimiter = " ")
        {
            return string.Join(delimiter, enumeration.ToArray());
        }

        private static string ReverseString(string value)
        {
            char[] arr = value.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
	}
}
