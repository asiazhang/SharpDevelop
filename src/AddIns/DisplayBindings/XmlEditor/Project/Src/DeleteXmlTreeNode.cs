﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.WinForms;

namespace ICSharpCode.XmlEditor
{
	public class DeleteXmlTreeNode : XmlTreeNodeClipboardCommand
	{
		protected override bool GetEnabled(IClipboardHandler editable)
		{
			return editable.EnableDelete;
		}
		
		protected override void Run(IClipboardHandler editable)
		{
			editable.Delete();
		}
	}
}
