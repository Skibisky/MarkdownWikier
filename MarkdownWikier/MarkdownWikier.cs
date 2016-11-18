﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkdownWikier
{
	public partial class MarkdownWikier : Form
	{
		public HashSet<MDFile> files = new HashSet<MDFile>();
		public MDFile current = null;

		public string[] openFiles = null;

		public MarkdownWikier(string[] args = null)
		{
			InitializeComponent();
			if (args != null)
			{
				List<string> ofiles = new List<string>();
				foreach (var s in args)
				{
					if (File.Exists(s))
					{
						ofiles.Add(s);
					}
				}
				openFiles = ofiles.ToArray();
			}
		}

		public void AddTab(string name = "new")
		{
			var md = new MDFile(OnFileChange, OnFileEdit, OnFileSave, OnFileLoad);
			md.name = name;
			files.Add(md);
			AddTab(md);
		}

		public void OnFileChange(object sender, EventArgs args)
		{
			if (sender == current)
				DisplayFile((MDFile)sender);
		}

		public void OnFileSave(object sender, EventArgs args)
		{
			var tab = tabsPages.Controls.Cast<TabPage>().Where(t => t.Tag as MDFile == sender as MDFile).FirstOrDefault();
			if (tab != null)
				tab.Text = ((sender as MDFile).name ?? "new");// remove *
		}

		public void OnFileLoad(object sender, EventArgs args)
		{
			var tab = tabsPages.Controls.Cast<TabPage>().Where(t => t.Tag as MDFile == sender as MDFile).FirstOrDefault();
			if (tab != null)
				tab.Text = ((sender as MDFile).name ?? "new");// remove *
		}

		public void OnFileEdit(object sender, EventArgs args)
		{
			var tab = tabsPages.Controls.Cast<TabPage>().Where(t => t.Tag as MDFile == sender as MDFile).FirstOrDefault();
			if (tab != null)
				tab.Text = ((sender as MDFile).name ?? "new") + ((sender as MDFile)?.Changed ?? true ? "*" : "");
			// add *
			RenderMD((sender as MDFile).Lines);
		}

		public void RenderMD(string[] linesIn)
		{
			//богатыеइबारत匣1
			richTextBox1.DeselectAll();
			richTextBox1.Clear();
			richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Regular);
			richTextBox1.LinkClicked += (a, b) => {
				Console.WriteLine(a);
			};
			
			//okay so "a ** b * c ** d", should have "b * c" in bold, and a/d regular
			//[link](anything:really/)
			//[invalid] (http://www.google.com  ) but format the legit link
			//"   CODEBLOCK"
			//## HEAD
			//* list instead
			//*notlist
			//  * list level 2?
			//    * list level n

			var lines = new List<string>();
			char prev = '\0';
			// what state are we formatting text
			int state = 0;
			int nextstate = 0;
			// are we writing text
			int writing = 1;
			var str = new List<char>();

			var dКнñигиℳↈﬖשּאﮋﺸﺱﷺ = 3;
			
			foreach (var c in string.Join("\r\n",linesIn))
			{
				switch (c)
				{
					case '[':
						break;
					case ']':
						break;
					case '(':
						if (prev == ']')
						{

						}
						else
						{
							str.Add(c);
						}
						break;
					case ')':
						//var pos = richTextBox1.SelectionStart;
						//richTextBox1.SelectedRtf = @"{\rtf1\ansi " + new string(str.ToArray()) + @"\v #www.topkek.com\v0}";
						//richTextBox1.Select(pos, str.Count + "www.topkek.com".Length + 1);
						richTextBox1.InsertLink(new string(str.ToArray()), "www.google.com");
						str.Clear();
						break;
					case '*':
						if (writing == 1)
						{
							if (state == 0)
							{
								nextstate = 1;
							}
							else if (state == 1)
							{
								nextstate = 0;
							}
							else if (state == 2)
							{
								nextstate = 3;
							}
							else if (state == 3)
							{
								nextstate = 2;
							}
							else
							{
								throw new NotImplementedException();
							}
						}
						else
						{
							if (nextstate == 0)
							{
								str.Add(prev);
								str.Add(c);
								nextstate = state;
							}
							else if (nextstate == 1)
							{
								nextstate = 2;
							}
							else if (nextstate == 2)
							{
								if (state == 3)
								{
									str.Insert(0, '*');
									state = 2;
									nextstate = 0;
								}
								else
									nextstate = 0;
							}
							else if (nextstate == 3)
							{
								if (state == 2)
									nextstate = 0;
								else
									nextstate = 1;
							}
							else
							{ 
								throw new NotImplementedException();
							}
						}
						writing = 0;
						break;
					default:
						if (state != nextstate)
						{
							richTextBox1.AppendText(new string(str.ToArray()));
							str.Clear();
							richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, 
								((nextstate == 2 || nextstate == 3) ? FontStyle.Bold : FontStyle.Regular) | 
								((nextstate == 1 || nextstate == 3) ? FontStyle.Italic : FontStyle.Regular)
								);
							state = nextstate;
						}
						writing = 1;
						str.Add(c);
						break;
				}
				prev = c;
			}
			richTextBox1.AppendText(new string(str.ToArray()));

			//richTextBox1.Lines = lines.ToArray();
		}

		public void AddTab(MDFile md)
		{
			files.Add(md);
			TabPage tp = new TabPage(md.name);
			tp.Tag = md;
			tabsPages.Controls.Add(tp);
			tabsPages.SelectTab(tabsPages.TabCount - 1);
		}

		public void ChangeTab(TabPage tab)
		{
			// get MDFile from tag
			// load into lines
			current = tab.Tag as MDFile;
			if (current != null)
				DisplayFile(current);
		}

		public void DisplayFile(MDFile file)
		{
			this.Invoke((MethodInvoker) delegate
			{
				tabsPages.SelectedTab = tabsPages.Controls.Cast<TabPage>().Where(t => file == t.Tag as MDFile).FirstOrDefault();
				textBox_raw.TextChanged -= textBox_raw_TextChanged;
				textBox_raw.Lines = file.Lines;
				textBox_raw.TextChanged += textBox_raw_TextChanged;
			});
		}

		public void SaveFile(MDFile md, bool saveAs = false)
		{
			// check exists
			// open file
			// write all lines
			// subscribe to file changes?
			string path = md.file;
			if (saveAs || path == null)
			{
				using (var sfd = new SaveFileDialog())
				{
					sfd.InitialDirectory = path ?? Environment.CurrentDirectory;
					sfd.Filter = "Markdown *.md|*.md|All Files|*.*";
					var res = sfd.ShowDialog();
					if (res == DialogResult.OK)
					{
						path = sfd.FileName;
					}
					else
						return;
				}
			}

			md.Save();

			// reload the file, enabling the watcher
			LoadFile(path, md);
		}

		public void LoadFile(string file, MDFile md = null)
		{
			// read file
			// create tab
			// subscribe to file changes?
			if (md == null)
			{
				if (current != null && !current.Changed && current.file == null)
					md = current;
				else
				{
					md = new MDFile(OnFileChange, OnFileEdit, OnFileSave, OnFileLoad);
				}
			}

			var emd = files.Where(f => f.file == file).FirstOrDefault();
			if (emd != null)
				md = emd;

			md.Load(file);

			var tab = tabsPages.Controls.Cast<TabPage>().Where(t => t.Tag as MDFile == md).FirstOrDefault();
			if (tab == null)
			{
				AddTab(md);
			}
			else
			{
				tab.Text = md.name;
			}
			DisplayFile(md);
		}

		private void MarkdownWikier_Load(object sender, EventArgs e)
		{
			current = new MDFile(OnFileChange, OnFileEdit, OnFileSave, OnFileLoad);
			files.Add(current);
			tabPage1.Tag = current;
			DisplayFile(current);
			if (openFiles != null)
			{
				foreach (var f in openFiles)
				{
					LoadFile(f);
				}
			}
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = Environment.CurrentDirectory;
				ofd.Filter = "Markdown *.md|*.md|All Files|*.*";
				ofd.Multiselect = false;
				var res = ofd.ShowDialog();
				if (res == DialogResult.OK)
				{
					LoadFile(ofd.FileName);
				}
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFile(current);
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddTab();
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// close current file
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// TODO: check all unsaved files
			Application.Exit();
		}

		private void textBox_raw_TextChanged(object sender, EventArgs e)
		{
			if (current != null)
				current.Lines = ((TextBox)sender).Lines;
		}

		private void tabsPages_SelectedIndexChanged(object sender, EventArgs e)
		{
			ChangeTab(((TabControl)sender).SelectedTab);
		}

	}
}
