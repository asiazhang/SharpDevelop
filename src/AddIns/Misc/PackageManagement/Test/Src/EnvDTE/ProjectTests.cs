﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.Core;
using ICSharpCode.PackageManagement;
using ICSharpCode.PackageManagement.EnvDTE;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;
using Rhino.Mocks;

namespace PackageManagement.Tests.EnvDTE
{
	[TestFixture]
	public class ProjectTests
	{
		Project project;
		TestableProject msbuildProject;
		//ProjectContentHelper helper;
		IPackageManagementProjectService fakeProjectService;
		IPackageManagementFileService fakeFileService;
		
		void CreateProject(string fileName = @"d:\projects\MyProject\MyProject.csproj", string language = "C#")
		{
			msbuildProject = ProjectHelper.CreateTestProject();
			msbuildProject.FileName = new FileName(fileName);
			//helper = new ProjectContentHelper();
			
			fakeProjectService = MockRepository.GenerateStub<IPackageManagementProjectService>();
			//fakeProjectService.Stub(service => service.GetProjectContent(msbuildProject)).Return(helper.ProjectContent);
			
			fakeFileService = MockRepository.GenerateStub<IPackageManagementFileService>();
			project = new Project(msbuildProject, fakeProjectService, fakeFileService);
		}
		
		void AddClassToProjectContent(string className)
		{
//			helper.AddClassToProjectContent(className);
		}
		
		void SetProjectForProjectContent()
		{
//			helper.SetProjectForProjectContent(msbuildProject);
		}
		
		void SetDifferentProjectForProjectContent()
		{
//			helper.SetProjectForProjectContent(ProjectHelper.CreateTestProject());
		}
		
		[Test]
		public void Name_ProjectNameIsMyApp_ReturnsMyApp()
		{
			CreateProject();
			msbuildProject.Name = "MyApp";
			
			string name = project.Name;
			
			Assert.AreEqual("MyApp", name);
		}
		
		[Test]
		public void FullName_ProjectFileNameIsSet_ReturnsFullFileName()
		{
			string expectedFullName = @"d:\projects\myproject\myproject.csproj";
			CreateProject(expectedFullName);
			
			string fullName = project.FullName;
			
			Assert.AreEqual(expectedFullName, fullName);
		}
		
		[Test]
		public void FileName_ProjectFileNameIsSet_ReturnsFullFileName()
		{
			string expectedFileName = @"d:\projects\myproject\myproject.csproj";
			CreateProject(expectedFileName);
			
			string fileName = project.FileName;
			
			Assert.AreEqual(expectedFileName, fileName);
		}
		
		[Test]
		public void Type_ProjectIsCSharpProject_ReturnsCSharp()
		{
			CreateProject(@"c:\projects\myproject\test.csproj", "C#");
			
			string projectType = project.Type;
			
			Assert.AreEqual("C#", projectType);
		}
		
		[Test]
		public void Type_ProjectIsCSharpProjectWithFileNameInUpperCase_ReturnsCSharp()
		{
			CreateProject(@"c:\projects\myproject\TEST.CSPROJ");
			
			string projectType = project.Type;
			
			Assert.AreEqual("C#", projectType);
		}
		
		[Test]
		public void Type_ProjectIsVBProject_ReturnsVBNet()
		{
			CreateProject(@"c:\projects\myproject\test.vbproj");
			
			string projectType = project.Type;
			
			Assert.AreEqual("VB.NET", projectType);
		}
		
		[Test]
		public void Type_ProjectHasUnknownProjectExtension_ReturnsEmptyString()
		{
			CreateProject(@"c:\projects\myproject\test.unknown");
			
			string projectType = project.Type;
			
			Assert.AreEqual(String.Empty, projectType);
		}
		
		[Test]
		public void Kind_ProjectIsCSharpProject_ReturnsCSharpProjectTypeGuid()
		{
			CreateProject(@"d:\projects\myproject\test.csproj");
			
			string kind = project.Kind;
			
			Assert.AreEqual(ProjectTypeGuids.CSharp.ToString(), kind);
		}
		
		[Test]
		public void Kind_ProjectIsVBNetProject_ReturnsCSharpProjectTypeGuid()
		{
			CreateProject( @"d:\projects\myproject\test.vbproj");
			
			string kind = project.Kind;
			
			Assert.AreEqual(ProjectTypeGuids.VBNet.ToString(), kind);
		}
		
		[Test]
		public void Kind_ProjectHasUnknownFileExtension_ReturnsEmptyString()
		{
			CreateProject(@"d:\projects\myproject\test.unknown");
			
			string kind = project.Kind;
			
			Assert.AreEqual(String.Empty, kind);
		}
		
		[Test]
		public void UniqueName_ProjectInSameFolderAsSolution_ReturnsProjectFileNameWithoutDirectoryPart()
		{
			CreateProject(@"d:\projects\myproject\MyProject.csproj");
			SetParentSolutionFileName(@"d:\projects\myproject\MyProject.sln");
			
			string name = project.UniqueName;
			
			Assert.AreEqual("MyProject.csproj", name);
		}
		
		[Test]
		public void UniqueName_ProjectInSubDirectoryOfSolutionFolder_ReturnsProjectFileNameWithContainsSubFolder()
		{
			CreateProject(@"d:\projects\myproject\SubFolder\MyProject.csproj");
			SetParentSolutionFileName(@"d:\projects\myproject\MyProject.sln");

			
			string name = project.UniqueName;
			
			Assert.AreEqual(@"SubFolder\MyProject.csproj", name);
		}
		
		void SetParentSolutionFileName(string fileName)
		{
			var solutionFileName = new FileName(fileName);
			msbuildProject.ParentSolution.Stub(s => s.FileName).Return(solutionFileName);
			msbuildProject.ParentSolution.Stub(s => s.Directory).Return(solutionFileName.GetParentDirectory());
		}
		
		[Test]
		public void ProjectItemsParent_ParentOfProjectsProjectItems_ReturnsTheProject()
		{
			CreateProject();
			
			object parent = project.ProjectItems.Parent;
			
			Assert.AreEqual(project, parent);
		}
		
		[Test]
		public void ConfigurationManager_ActiveConfigurationOutputPathProperty_ReturnsOutputPathForProject()
		{
			CreateProject();
			msbuildProject.SetProperty("OutputPath", @"bin\debug\");
			global::EnvDTE.Configuration activeConfig = project.ConfigurationManager.ActiveConfiguration;
			
			string outputPath = (string)activeConfig.Properties.Item("OutputPath").Value;
			
			Assert.AreEqual(@"bin\debug\", outputPath);
		}
		
		[Test]
		[Ignore("TODO")]
		public void CodeModel_NoTypesInProjectAndCallCodeTypeFromFullName_ReturnsNull()
		{
			CreateProject();
			
			global::EnvDTE.CodeType codeType = project.CodeModel.CodeTypeFromFullName("UnknownTypeName");
			
			Assert.IsNull(codeType);
		}
		
		[Test]
		[Ignore("TODO")]
		public void CodeModel_ClassExistsInProjectContentAndCallCodeTypeFromFullName_ReturnsNonCodeType()
		{
			CreateProject();
			AddClassToProjectContent("Tests.MyClass");
			
			global::EnvDTE.CodeType codeType = project.CodeModel.CodeTypeFromFullName("Tests.MyClass");
			
			Assert.IsNotNull(codeType);
		}
		
		[Test]
		[Ignore("TODO")]
		public void CodeModel_ClassExistsInProjectContentForProject_ReturnsClassWithLocationSetToInProject()
		{
			CreateProject();
			SetProjectForProjectContent();
			//helper.AddClassToCompletionEntries(String.Empty, "MyClass");
			
			//CodeElement element = project.CodeModel.CodeElements.FirstOrDefault();
			
			//Assert.AreEqual(global::EnvDTE.vsCMInfoLocation.vsCMInfoLocationProject, element.InfoLocation);
		}
		
		[Test]
		[Ignore("TODO")]
		public void CodeModel_ClassExistsInProjectContentForDifferentProject_ReturnsClassWithLocationSetToExternal()
		{
			CreateProject();
			//SetProjectForProjectContent();
			//helper.AddClassCompletionEntriesInDifferentProjectContent(String.Empty, "MyClass");
			
			//CodeElement element = project.CodeModel.CodeElements.FirstOrDefault();
			
			//Assert.AreEqual(global::EnvDTE.vsCMInfoLocation.vsCMInfoLocationExternal, element.InfoLocation);
		}
	}
}
