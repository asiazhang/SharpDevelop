﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading;

using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.Core;
using ICSharpCode.Decompiler;
using ICSharpCode.ILSpyAddIn;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor.Bookmarks;
using ICSharpCode.SharpDevelop.Workbench;

namespace ICSharpCode.ILSpyAddIn
{
	/// <summary>
	/// Hosts a decompiled type.
	/// </summary>
	class DecompiledViewContent : AbstractViewContentWithoutFile
	{
		/// <summary>
		/// Entity to jump to once decompilation has finished.
		/// </summary>
		string jumpToEntityIdStringWhenDecompilationFinished;
		
		bool decompilationFinished;
		
		readonly CodeEditor codeEditor = new CodeEditor();
		readonly CancellationTokenSource cancellation = new CancellationTokenSource();
		
		Dictionary<string, TextLocation> memberLocations;
		public Dictionary<string, MethodDebugSymbols> DebugSymbols { get; private set; }
		
		#region Constructor
		public DecompiledViewContent(DecompiledTypeReference typeName, string entityTag)
		{
			this.DecompiledTypeName = typeName;
			
			this.Services = codeEditor.GetRequiredService<IServiceContainer>();
			codeEditor.PrimaryTextEditor.TextArea.LeftMargins.RemoveAll(m => m is ChangeMarkerMargin);
			this.jumpToEntityIdStringWhenDecompilationFinished = entityTag;
			this.TitleName = "[" + ReflectionHelper.SplitTypeParameterCountFromReflectionName(typeName.Type.Name) + "]";
			
			InitializeView();
			
			SD.BookmarkManager.BookmarkRemoved += BookmarkManager_Removed;
			SD.BookmarkManager.BookmarkAdded += BookmarkManager_Added;
			
			this.codeEditor.FileName = this.DecompiledTypeName.ToFileName();
			this.codeEditor.ActiveTextEditor.IsReadOnly = true;
			this.codeEditor.ActiveTextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
		}
		#endregion
		
		#region Properties
		public DecompiledTypeReference DecompiledTypeName { get; private set; }
		
		public override FileName PrimaryFileName {
			get { return this.DecompiledTypeName.ToFileName(); }
		}
		
		public override object Control {
			get { return codeEditor; }
		}
		
		public override bool IsReadOnly {
			get { return true; }
		}
		#endregion
		
		#region Dispose
		public override void Dispose()
		{
			cancellation.Cancel();
			codeEditor.Dispose();
			SD.BookmarkManager.BookmarkAdded -= BookmarkManager_Added;
			SD.BookmarkManager.BookmarkRemoved -= BookmarkManager_Removed;
			base.Dispose();
		}
		#endregion
		
		#region Load/Save
		public override void Load()
		{
			// nothing to do...
		}
		
		public override void Save()
		{
			if (!decompilationFinished)
				return;
			// TODO: show Save As dialog to allow the user to save the decompiled file
		}
		#endregion
		
		public override INavigationPoint BuildNavPoint()
		{
			return codeEditor.BuildNavPoint();
		}
		
		#region JumpToEntity
		public void JumpToEntity(string entityIdString)
		{
			if (!decompilationFinished) {
				this.jumpToEntityIdStringWhenDecompilationFinished = entityIdString;
				return;
			}
			TextLocation location;
			if (entityIdString != null && memberLocations != null && memberLocations.TryGetValue(entityIdString, out location))
				codeEditor.JumpTo(location.Line, location.Column);
		}
		#endregion
		
		#region Decompilation
		async void InitializeView()
		{
			try {
				var parseInformation = await SD.ParserService.ParseAsync(DecompiledTypeName.ToFileName(), cancellationToken: cancellation.Token);
				if (parseInformation == null || !(parseInformation.UnresolvedFile is ILSpyUnresolvedFile)) return;
				var file = (ILSpyUnresolvedFile)parseInformation.UnresolvedFile;
				memberLocations = file.MemberLocations;
				DebugSymbols = file.DebugSymbols;
				OnDecompilationFinished(file.Output);
			} catch (OperationCanceledException) {
				// ignore cancellation
			} catch (Exception ex) {
				if (cancellation.IsCancellationRequested) {
					MessageService.ShowException(ex);
					return;
				}
				SD.AnalyticsMonitor.TrackException(ex);
				
				StringWriter writer = new StringWriter();
				writer.WriteLine(string.Format("Exception while decompiling {0} ({1})", DecompiledTypeName.Type, DecompiledTypeName.AssemblyFile));
				writer.WriteLine();
				writer.WriteLine(ex.ToString());
				OnDecompilationFinished(writer.ToString());
			}
		}
		
		void OnDecompilationFinished(string output)
		{
			if (cancellation.IsCancellationRequested)
				return;
			codeEditor.Document.Text = output;
			codeEditor.Document.UndoStack.ClearAll();
			
			this.decompilationFinished = true;
			JumpToEntity(this.jumpToEntityIdStringWhenDecompilationFinished);
			
			// update UI
			//UpdateIconMargin();
			
			// fire events
			OnDecompilationFinished(EventArgs.Empty);
		}
		#endregion
		
		#region Update UI
		/*
		void UpdateIconMargin()
		{
			codeView.IconBarManager.UpdateClassMemberBookmarks(
				ParserService.ParseFile(tempFileName, new AvalonEditDocumentAdapter(codeView.Document, null)),
				null);
			
			// load bookmarks
			foreach (SDBookmark bookmark in BookmarkManager.GetBookmarks(this.codeView.TextEditor.FileName)) {
				bookmark.Document = this.codeView.TextEditor.Document;
				codeView.IconBarManager.Bookmarks.Add(bookmark);
			}
		}
		 */
		#endregion
		
		#region Bookmarks
		void BookmarkManager_Removed(object sender, BookmarkEventArgs e)
		{
			var mark = e.Bookmark;
			if (mark != null && codeEditor.IconBarManager.Bookmarks.Contains(mark)) {
				codeEditor.IconBarManager.Bookmarks.Remove(mark);
				mark.Document = null;
			}
		}
		
		void BookmarkManager_Added(object sender, BookmarkEventArgs e)
		{
			var mark = e.Bookmark;
			if (mark != null && mark.FileName == PrimaryFileName) {
				codeEditor.IconBarManager.Bookmarks.Add(mark);
				mark.Document = this.codeEditor.Document;
			}
		}
		#endregion
		
		#region Events
		
		public event EventHandler DecompilationFinished;
		
		protected virtual void OnDecompilationFinished(EventArgs e)
		{
			if (DecompilationFinished != null) {
				DecompilationFinished(this, e);
			}
		}
		
		#endregion
	}
}
