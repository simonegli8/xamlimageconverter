using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Security;

namespace XamlImageConverter {

	public class ImageMagickEncoder: IEncoder {

		//string exe;
		BitmapEncoder intermediateEncoder;

		public static string GlobalOptions { get; set; }
		public string Options { get; set; }

		int jpegQ;
		public int JpegQuality {
			get { return jpegQ; }
			set {
				if (jpegQ != value && intermediateEncoder is JpegBitmapEncoder) {
					((JpegBitmapEncoder)intermediateEncoder).QualityLevel = jpegQ;
				}
				jpegQ = value;
			}
		}

		class IntermediateFile {
			public string Filename;
			public string Options;
		}

		string iformat;
		public string IntermediateFormat  {
			get { return iformat; }
			set {
				iformat = value;
				intermediateEncoder = EncoderFactory.CreateBitmapEncoder(iformat, JpegQuality);
			}
		}

		public ImageMagickEncoder() {
			//TODO get exact msbuildextensions path from registry.
			JpegQuality = 90;
			IntermediateFormat = "png";
			Options = string.Empty;
		}

		static Dictionary<string, List<IntermediateFile>> IntermediateFiles = new Dictionary<string, List<IntermediateFile>>();

		public void Save(string filename) {
			var ext1 = IntermediateFormat;
			if (!ext1.StartsWith(".")) ext1 = "." + ext1;

			var temp1 = Path.GetTempFileName() + ext1;
			var frames = Frames.ToList();
			intermediateEncoder.Frames.Clear();
			foreach (var frame in frames) intermediateEncoder.Frames.Add(frame);
			intermediateEncoder.Save(temp1);
			if (!IntermediateFiles.ContainsKey(filename)) IntermediateFiles[filename] = new List<IntermediateFile>();
			IntermediateFiles[filename].Add(new IntermediateFile { Options = this.Options, Filename = temp1 });
			
			/*
			 * var ext = Path.GetExtension(filename).ToUpper().TrimStart('.');
			var process = Process.Start(exe, Options + " \"" + temp1 + "\" \"" + filename + "\"");
			process.WaitForExit();
			File.Delete(temp1);
		*/
		}

		[SecurityCritical]
		public static void SaveAll(List<Process> Processes, Errors log) {
			foreach (var pdf in IntermediateFiles.Keys) {
				StringBuilder sb = new StringBuilder();
				foreach (var tempfile in IntermediateFiles[pdf]) {
					sb.Append(" ");
					sb.Append(tempfile.Options ?? "");
					sb.Append(" \"");
					sb.Append(tempfile.Filename);
					sb.Append("\" ");
				}
				var path = AppDomain.CurrentDomain.BaseDirectory;
				var exe = Path.Combine(path, "Lazy\\ImageMagick\\convert.exe");
				var ext = Path.GetExtension(pdf).ToUpper().TrimStart('.');
				var adjoin = " ";
				if (IntermediateFiles[pdf].Count > 1) adjoin = " -adjoin ";
				//var pinfo = new System.Diagnostics.ProcessStartInfo(exe, (GlobalOptions ?? "") + adjoin + sb.ToString() + "\"" + pdf + "\"");
				var process = Process.Start(exe, (GlobalOptions ?? "") + adjoin + sb.ToString() + "\"" + pdf + "\"");
				lock (Processes) Processes.Add(process);
				var tpdf = pdf;
				var tempFiles = IntermediateFiles[tpdf];
				process.Exited += (sedner, args) => {
					foreach (var tempfile in tempFiles) File.Delete(tempfile.Filename);
					log.Message("Created {0} ({1} page{2})", Path.GetFileName(tpdf), tempFiles.Count, (tempFiles.Count != 1) ? "s" : "");
					IntermediateFiles.Remove(tpdf);
				};
			}
			//IntermediateFiles.Clear();
		}

		public IList<BitmapFrame> Frames { get { return intermediateEncoder.Frames; } set { intermediateEncoder.Frames = value; } }

	}
}
