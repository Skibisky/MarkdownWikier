using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MarkdownWikier
{
	public class MDFile
	{
		public string file = null;
		public string name = null;

		public EventHandler<EventArgs> onChange = null;
		public EventHandler<EventArgs> onEdit = null;
		public EventHandler<EventArgs> onSave = null;
		public EventHandler<EventArgs> onLoad = null;

		public string[] Lines
		{
			get
			{
				return lines;
			}
			set
			{
				changed = true;
				lines = value;
				if (onEdit != null)
					onEdit(this, new EventArgs());
			}
		}

		public bool Changed
		{
			get
			{
				return changed;
			}
		}

		private string[] lines = { "" };
		private bool changed = false;

		private FileSystemWatcher watcher;
		private Timer reload;


		public MDFile(EventHandler<EventArgs> change, EventHandler<EventArgs> edit, EventHandler<EventArgs> save, EventHandler<EventArgs> load)
		{
			watcher = new FileSystemWatcher();
			watcher.Changed += OnChange;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			reload = new Timer(10);
			reload.Elapsed += onTimer;
			onChange = change;
			onEdit = edit;
			onSave = save;
			onLoad = load;
		}

		public void Load(string path)
		{
			file = new FileInfo(path).FullName;
			name = new FileInfo(file).Name;
			lines = File.ReadAllLines(file);
			changed = false;
			watcher.Path = Path.GetDirectoryName(file);
			watcher.Filter = Path.GetFileName(file);
			watcher.EnableRaisingEvents = true;
			if (onEdit != null)
				onEdit(this, new EventArgs());
			if (onLoad != null)
				onLoad(this, new EventArgs());
		}

		public void Save()
		{
			if (file == null)
				return;

			watcher.EnableRaisingEvents = false;
			File.WriteAllLines(file, lines);
			watcher.EnableRaisingEvents = false;
			changed = false;
			if (onSave != null)
				onSave(this, new EventArgs());
		}


		private void onTimer(object sender, ElapsedEventArgs e)
		{
			reload.Stop();
			lines = File.ReadAllLines(file);
			if (onChange != null)
				onChange(this, new EventArgs());
		}

		public void OnChange(object sender, FileSystemEventArgs e)
		{
			// consider prompting?
			// check if current?
			reload.Start();
		}
	}
}
