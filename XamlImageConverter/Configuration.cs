using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.Diagnostics;
using System.Threading;

namespace XamlImageConverter.Configuration {

	public class ConfigurationSectionAttribute: Attribute {
		public string Name { get; set; }
		public string Group { get; set; }
		public ConfigurationSectionAttribute() { }
		public ConfigurationSectionAttribute(string name) { Name = name; }
	}

	public class ConfigurationSection: System.Configuration.ConfigurationSection {

		[ConfigurationProperty("xmlns", IsRequired = false)]
		public string Xmlns { get { return this["xmlns"] as string; } set { this["xmlns"] = value; } }

		protected virtual void CopyFrom(ConfigurationSection config) {
			if (config != null && config != this) {
				foreach (ConfigurationProperty key in config.Properties) {
					this[key] = config[key];
				}
			}
		}

		string TypeName {
			get {
				var name = this.GetType().Name;
				if (name.EndsWith("Section")) name = name.Remove(name.Length - "Section".Length);
				if (name.EndsWith("Configuration")) name = name.Remove(name.Length - "Configuration".Length);
				return name.TrimEnd('.');
			}
		}

		public string SectionName {
			get {
				string name = "";
				string group = "";

				var t = this.GetType();
				var a = (ConfigurationSectionAttribute)t.GetCustomAttributes(typeof(ConfigurationSectionAttribute), true).FirstOrDefault();
				if (a != null) {
					if (!string.IsNullOrEmpty(a.Name)) name = a.Name;
					if (!string.IsNullOrEmpty(a.Group)) group = a.Group;
				}

				if (string.IsNullOrEmpty(name)) name = TypeName;

				if (string.IsNullOrEmpty(group)) return name;
				else return group + "/" + name;
			}
		}

		[ThreadStatic]
		static bool reading = false;

		public static ConfigurationSection Read(string section) {
			ConfigurationSection config = null;
			try {
				if (!reading) {
					reading = true;
					config = WebConfigurationManager.GetSection(section) as ConfigurationSection;
					reading = false;
				}
			} catch (Exception) {
				// Error("Fehler beim lesen der Konfiguration.", ex);
			}
			return config;
		}

		public virtual void Reset() {
			CopyFrom(Read(SectionName));
		}

		public ConfigurationSection(): base() {
			Reset();
		}
	}
}