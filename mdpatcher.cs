using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using DiscUtils;
using DiscUtils.Common;
using DiscUtils.Iso9660;

class MDPatcher : Form
{	
	public static string versionString = "1.0";
	public static string copyrightString = "(c) 2017 Giuseppe Gatta / nextvolume";
	
	MainMenu mainMenu;
	TextBox tbox;
	
	RawImageStream imageStream;
	MenuItem	fileItem;
	MenuItem	helpItem;
	
	public static Pattern[] patterns = 
	{
		new Pattern1(), new Pattern2(), new Pattern3(), new Pattern4(), new Pattern5(), new Pattern6(), new Pattern7()
	};
		
	public int Scan(CDReader cd, DiscDirectoryInfo d, RawImageStream fstream)
	{
		int numPatches=0;
		
		foreach(DiscFileInfo fi in d.GetFiles())
		{
			using(Stream file = cd.OpenFile(fi.FullName, FileMode.Open))
			{					
				// Scan file for patterns					
				byte[] readBuffer = new byte[file.Length];
				file.Seek(0, SeekOrigin.Begin);

				long filePos;
				
				file.Read(readBuffer, 0, 1);
				filePos = fstream.Seek(0, SeekOrigin.Current) - 1;
				file.Read(readBuffer, 1, (int)file.Length - 1);	

				foreach(Pattern p in patterns)
				{
					byte[] patternBytes = p.GetPattern();
					byte[] patchedPatternBytes = p.GetPatchedPattern();
						
					int length = patternBytes.Length;
						
					for(int i = 0; i < file.Length - length; i++)
					{
						int x;
							
						for(x = 0; x < length; x++)
						{
							if(readBuffer[i+x] != patternBytes[x])
								break;
						}
							
						if(x == length)
						{
							// Found the bytes for the pattern
							TextBoxWriteLine(p.GetName() + " found at 0x" + i.ToString("X") + " in " + fi.FullName);
							
							// Patch!
							long old = fstream.Seek(0, SeekOrigin.Begin);
							
							fstream.Seek(filePos+i, SeekOrigin.Begin);
							fstream.Write(patchedPatternBytes, 0, length);
							
							fstream.Seek(old, SeekOrigin.Begin);
							
							numPatches++;
						}
						else
						{
							for(x = 0; x < length; x++)
							{
								if(readBuffer[i+x] != patchedPatternBytes[x])
									break;
							}
							
							if(x == length)
								// Found the bytes for the patched pattern
								TextBoxWriteLine(p.GetName() + " (already patched) found at 0x" + i.ToString("X") + " in " + fi.FullName);
						}

					}
				}
			}
		}
		
		foreach (DiscDirectoryInfo di in d.GetDirectories())
			numPatches+=Scan(cd, di, fstream);
		
		return numPatches;
	}
	
	void ResetProgramText()
	{
		tbox.Text = "MDPatcher " + versionString + " " + copyrightString + "\n";
	}
	
	public MDPatcher()
	{
		Text = "MDPatcher";
		Size = new Size(500, 400);

		mainMenu = new MainMenu();
		fileItem = mainMenu.MenuItems.Add("&File");

		fileItem.MenuItems.Add(new MenuItem("&Open...",
			new EventHandler(this.OnOpenFile), Shortcut.CtrlO));
		
		fileItem.MenuItems.Add(new MenuItem("E&xit",
			new EventHandler(this.OnExit), Shortcut.CtrlX));
		
		helpItem = mainMenu.MenuItems.Add("&Help");
	
		helpItem.MenuItems.Add(new MenuItem("&About...", new EventHandler(this.OnAbout)));

		Menu = mainMenu;

		
		tbox = new TextBox();
		
		tbox.Parent = this;
		tbox.Dock = DockStyle.Fill;
		tbox.Multiline = true;
		tbox.ScrollBars = ScrollBars.Vertical;
		ResetProgramText();
		tbox.ReadOnly = true;
		
		CenterToScreen();
	}
	
	void OnOpenFile(object sender, EventArgs e)
	{
		OpenFileDialog dialog = new OpenFileDialog();
		
		dialog.Filter = 
			"CDROM Image Files (*.iso;*.bin)|*.iso;*.bin;*.GIF|All files (*.*)|*.*";
		dialog.FilterIndex = 1;
		dialog.RestoreDirectory = true;
		

		if(dialog.ShowDialog() == DialogResult.OK)
		{	
			try
			{
				imageStream = new RawImageStream(dialog.FileName, FileMode.Open, FileAccess.ReadWrite);
				
				StartThread();
			}
		
			catch (Exception ex)
			{
				MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
			}
		}
		
	}
	
	void OnExit(object sender, EventArgs e) 
	{
		Close();
	}
	
	void OnAbout(object sender, EventArgs e)
	{
		MessageBox.Show("MDPatcher " + versionString + "\n" +
						"Patches SEGA Saturn games to make them work with a Megadrive/Genesis Joypad\n"+
						copyrightString+"\n\n"+
						"http://unhaut.x10host.com/mdpatcher");
	}
	
	void StartThread ()
	{
		
		fileItem.Enabled = false;
		helpItem.Enabled = false;
		ResetProgramText();
		tbox.Text += "Do not close until the scan is completed!\n";
		tbox.Text += "This operation may take a while...\n";
		Text = "MDPatcher (Scanning...)";

		Thread t = new Thread(new ThreadStart(ThreadJob));
		t.IsBackground = true;
		t.Start();
	}
    
	delegate void StringParameterDelegate (string value);

	
	void ThreadJob()
	{
		int numPatches=0;
		
		using(imageStream)
		{
			for(int x = 0; x < 3; x++)
			{
				imageStream.Seek(0, SeekOrigin.Begin);
				imageStream.SetMode(x);
			
				try
				{
					CDReader cd = new CDReader(imageStream, true);
				
					numPatches=Scan(cd, cd.Root, imageStream);
					
					break;
				}
			
				catch(Exception ex)
				{
					TextBoxWriteLine("Trying with MODE " + (x+1) + " ("+ex.Message+")"); 
				}
			}
		}
		
		TextBoxWriteLine(numPatches + " patch(es) applied");
		
		Invoke(new StringParameterDelegate(ThreadEnd), 
			new object[]{"Scan completed!\n"+
				numPatches + " patch(es) applied"});
	}
	
	void ThreadEnd(String message)
	{
		MessageBox.Show(message);
		
		fileItem.Enabled = true;
		helpItem.Enabled = true;
		Text = "MDPatcher";
	}	
	
	void TextBoxWriteLine(String line)
	{
		Invoke(new StringParameterDelegate(TextBoxWriteLine2), new object[]{line});	
	}
	
	void TextBoxWriteLine2(String stringToAppend)
	{
		tbox.Text += stringToAppend + "\n";
		tbox.ScrollToCaret();
	}
		
	public static void Main(string[] args)
	{
		Application.Run(new MDPatcher());
	}
}
