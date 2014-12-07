using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public class ProgramBanks
	{
		public class PluginProgram
		{
			public string Name { get; set; }
			public string Library { get; set; }
			public string Path { get; set; }
			public string Data { get; set; }
		}

		private static ProgramBanks bank;
		public static ProgramBanks Bank
		{
			get
			{
				if (bank == null)
					bank = new ProgramBanks();

				return bank;
			}
		}

		private ProgramBanks()
		{
			var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
			FactoryDir = Path.Combine(Path.GetDirectoryName(assemblyLocation), "Programs", "Factory Programs");
			UserDir = Path.Combine(Path.GetDirectoryName(assemblyLocation), "Programs", "User Programs");

			FactoryPrograms = GetProgramFiles(FactoryDir);
			UserPrograms = GetProgramFiles(UserDir);
		}

		public bool CanDeleteProgram(PluginProgram program)
		{
			return FactoryPrograms.Any(x => 
				x.Library == program.Library && 
				x.Name == program.Name && 
				x.Path == program.Path && 
				File.Exists(program.Path));
		}

		public void DeleteProgram(PluginProgram program)
		{
			if (CanDeleteProgram(program))
				File.Delete(program.Path);

			// Reload programs
			UserPrograms = GetProgramFiles(FactoryDir);
		}

		public bool SaveProgram(string name, string data, bool overwrite)
		{
			var programPath = Path.Combine(UserDir, name + ".json");
			if (!Directory.Exists(Path.GetDirectoryName(programPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(programPath));

			if (File.Exists(programPath) && !overwrite)
				return false;

			File.WriteAllText(programPath, data);

			// Reload programs
			UserPrograms = GetProgramFiles(UserDir);
			return true;
		}

		private PluginProgram[] GetProgramFiles(string dir)
		{
			if (!Directory.Exists(dir))
				return new PluginProgram[0];

			var programs = Directory.GetFiles(dir).Select(path => new PluginProgram
			{
				Data = File.ReadAllText(path),
				Library = Path.GetFileName(Path.GetDirectoryName(path)),
				Name = Path.GetFileNameWithoutExtension(path),
				Path = path
			}).ToArray();

			return programs;
		}

		public string FactoryDir { get; private set; }
		public string UserDir { get; private set; }

		public PluginProgram[] FactoryPrograms { get; set; }
		public PluginProgram[] UserPrograms { get; set; }
	}
}
