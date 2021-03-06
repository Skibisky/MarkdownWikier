﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkdownWikier {
	static class Program {
		// https://stackoverflow.com/questions/19147/what-is-the-correct-way-to-create-a-single-instance-application
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Application.Run(new MarkdownWikier(args));
		}
	}
}
