﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Workbench;

namespace ICSharpCode.Reports.Addin
{
	/// <summary>
	/// Description of AbstractDesignerLoader.
	/// </summary>
	
	public class ReportDesignerGenerator:IDesignerGenerator
	{
		private ReportDesignerView viewContent;
		
		public ReportDesignerGenerator()
		{
			Console.WriteLine("Create ReportDesignerGenerator()");
		}
		
		public CodeDomProvider CodeDomProvider {
			get {
				throw new NotImplementedException();
			}
		}
		
		
		public ReportDesignerView ViewContent {
			get { return this.viewContent; }
		}
		
		
		public void Attach(ReportDesignerView viewContent)
		{
			Console.WriteLine("ReportDesignerGenerator:Attach");
			if (viewContent == null) {
				throw new ArgumentNullException("viewContent");
			}
			this.viewContent = viewContent;
		}
		
		
		public void Detach()
		{
			this.viewContent = null;
		}
		
		
		public IEnumerable<OpenedFile> GetSourceFiles(out OpenedFile designerCodeFile)
		{
			Console.WriteLine("ReportDesignerGenerator:getSourceFile");
			designerCodeFile = this.viewContent.PrimaryFile;
			return new [] {designerCodeFile};
		}
		
		
		public void MergeFormChanges(CodeCompileUnit unit){
			Console.WriteLine("ReportDesignerGenerator:MergeFormChanges");
				System.Diagnostics.Trace.WriteLine("Generator:MergeFormChanges");
				StringWriterWithEncoding writer = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
				XmlTextWriter xml = XmlHelper.CreatePropperWriter(writer);
				this.InternalMergeFormChanges(xml);
				viewContent.ReportFileContent = writer.ToString();
		}
		
		
		
		private void InternalMergeFormChanges(XmlTextWriter xml)
		{
			if (xml == null) {
				throw new ArgumentNullException("xml");
			}
			Console.WriteLine("ReportDesignerGenerator:internalMergeFormChanges");
			ReportDesignerWriter rpd = new ReportDesignerWriter();
			XmlHelper.CreatePropperDocument(xml);
			
			foreach (IComponent component in viewContent.Host.Container.Components) {
				if (!(component is Control)) {
					rpd.Save(component,xml);
				}
			}
			xml.WriteEndElement();
			xml.WriteStartElement("SectionCollection");
			
			// we look only for Sections
			foreach (IComponent component in viewContent.Host.Container.Components) {
				BaseSection b = component as BaseSection;
				if (b != null) {
					rpd.Save(component,xml);
				}
			}
			//SectionCollection
			xml.WriteEndElement();
			//Reportmodel
			xml.WriteEndElement();
			xml.WriteEndDocument();
			xml.Close();
		}
	
		
		public bool InsertComponentEvent(IComponent component, EventDescriptor edesc, string eventMethodName, string body, out string file, out int position)
		{
			throw new NotImplementedException();
		}
	}
}
