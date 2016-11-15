using System;
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

		public MarkdownWikier()
		{
			InitializeComponent();

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
				tab.Text = ((sender as MDFile).name ?? "new") + "*";
			// add *
			RenderMD((sender as MDFile).Lines);
		}

		public void RenderMD(string[] linesIn)
		{
			richTextBox1.DeselectAll();
			richTextBox1.Clear();
			richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Regular);
			var lines = new List<string>();
			char prev = '\0';
			int state = 0;
			var str = new List<char>();
			/*
			foreach (var c in string.Join('\r\n',linesIn))
			{
				switch (c)
				{
					case '*':
						if (state == 0)
						{

						}
						break;
					default:
						str.Add(c);
						break;
				}
				prev = c;
			}*/
			richTextBox1.Lines = lines.ToArray();
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
				textBox_raw.Lines = file.Lines;
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
				if (!current.Changed && current.file == null)
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
