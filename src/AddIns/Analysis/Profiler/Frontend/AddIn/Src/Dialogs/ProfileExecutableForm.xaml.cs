﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

using ICSharpCode.Core;
using ICSharpCode.Profiler.Controller;
using ICSharpCode.Profiler.Controller.Data;
using ICSharpCode.SharpDevelop;
using Microsoft.Win32;

namespace ICSharpCode.Profiler.AddIn.Dialogs
{
	/// <summary>
	/// Interaktionslogik für ProfileExecutableForm.xaml
	/// </summary>
	public partial class ProfileExecutableForm : Window
	{
		public ProfileExecutableForm()
		{
			InitializeComponent();
		}
		
		void btnCancelClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
		
		void btnStartClick(object sender, RoutedEventArgs e)
		{
			try {
				if (!File.Exists(txtExePath.Text))
					throw new FileNotFoundException("file '" + txtExePath.Text + "' was not found!");
				if (!Directory.Exists(txtWorkingDir.Text))
					throw new DirectoryNotFoundException("directory '" + txtWorkingDir.Text + "' was not found!");
				
				string outputName = "Session" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".sdps";
				string outputPath = "";
				
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.InitialDirectory = Path.GetDirectoryName(txtExePath.Text);
				sfd.Filter = StringParser.Parse("${res:AddIns.Profiler.FileExtensionDescription}|*.sdps");
				sfd.FileName = outputName;
				if (sfd.ShowDialog() == true)
					outputPath = sfd.FileName;
				else
					return;
				
				try {
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
					
					var runner = CreateRunner(txtExePath.Text, txtWorkingDir.Text, txtArgs.Text, new ProfilingDataSQLiteWriter(outputPath));

					if (runner != null) {
						runner.RunFinished += delegate {
							SD.MainThread.InvokeIfRequired(() => FileService.OpenFile(outputPath));
						};
						
						runner.Run();
					}
					
					Close();
				} catch (ProfilerException ex) {
					MessageService.ShowError(ex.Message);
				}
			} catch (ArgumentNullException) {
				MessageService.ShowError(StringParser.Parse("${res:AddIns.Profiler.ProfileExecutable.ErrorMessage}"));
			} catch (FileNotFoundException ex) {
				MessageService.ShowError(ex.Message);
			} catch (DirectoryNotFoundException ex2) {
				MessageService.ShowError(ex2.Message);
			} catch (UnauthorizedAccessException ex4) {
				MessageService.ShowError(ex4.Message);
			} catch (Exception ex3) {
				MessageService.ShowException(ex3);
			}
		}
		
		void btnSelectFileClick(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "Programs|*.exe|All files|*.*";
			dlg.DefaultExt = ".exe";
			if (!(dlg.ShowDialog() ?? false))
				return;
			txtExePath.Text = dlg.FileName;
			
			if (File.Exists(dlg.FileName))
				txtWorkingDir.Text = Path.GetDirectoryName(dlg.FileName);
		}
		
		void btnSelectDirClick(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			
			if (!(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK))
				return;
			txtWorkingDir.Text = dlg.SelectedPath;
		}
		
		ProfilerRunner CreateRunner(string path, string workingDirectory, string args, IProfilingDataWriter writer)
		{
			if (args == null)
				throw new ArgumentNullException("args");
			if (workingDirectory == null)
				throw new ArgumentNullException("workingdirectory");
			if (path == null)
				throw new ArgumentNullException("path");
			
			if (!File.Exists(path))
				throw new FileNotFoundException("file '" + path + "' was not found!");
			if (!Directory.Exists(workingDirectory))
				throw new DirectoryNotFoundException("directory '" + workingDirectory + "' was not found!");
			
			ProcessStartInfo info = new ProcessStartInfo(path, args);
			info.WorkingDirectory = workingDirectory;
			
			ProfilerRunner runner = new ProfilerRunner(info, true, writer);
			
			return runner;
		}
	}
}
