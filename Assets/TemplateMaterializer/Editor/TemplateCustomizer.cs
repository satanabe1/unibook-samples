using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CustomTemplate
{
	public static class TemplateKeyword
	{
		public const string ClassName = "$ClassName$";
	}

	public interface INode
	{
		string Text { get; }
	}

	public class PlainTextNode : INode
	{
		public virtual string Text { get; set; }
	}

	public class VariableTextNode : INode
	{
		public virtual string Text { get; set; }

		public virtual string PlainText { get; set; }
	}

	public class ClassNameNode : VariableTextNode
	{
	}

	public interface ITemplateParser
	{
		List<List<INode>> ParseTemplate (string templateText);
	}

	public class RoughlyTemplateParser : ITemplateParser
	{
		public List<List<INode>> ParseTemplate (string templateText)
		{
			if (string.IsNullOrEmpty (templateText)) {
				return new List<List<INode>> (0);
			}
			return ParseTemplate (templateText.Split (System.Environment.NewLine.ToCharArray ()));
		}

		public List<List<INode>> ParseTemplate (string[] templateTextLines)
		{
			if ((templateTextLines == null) || (templateTextLines.Length == 0)) {
				return new List<List<INode>> (0);
			}
			var parseResultList = new List<List<INode>> ();
			foreach (var line in templateTextLines) {
				parseResultList.Add (ParseLine (line));
			}
			return parseResultList;
		}

		protected List<INode> ParseLine (string line)
		{
			if (string.IsNullOrEmpty (line)) {
				return new List<INode> (0);
			}
			var parseResult = new List<INode> ();
			int classNameIndex = -1;
			while ((classNameIndex = line.IndexOf(TemplateKeyword.ClassName)) > -1) {
				if (classNameIndex == 0) {
					ClassNameNode classNameNode = new ClassNameNode () {
						PlainText = TemplateKeyword.ClassName,
						Text = TemplateKeyword.ClassName,
					};
					parseResult.Add (classNameNode);
					line = line.Substring (TemplateKeyword.ClassName.Length);
				} else {
					PlainTextNode plainTextNode = new PlainTextNode () {
						Text = line.Substring(0, classNameIndex),
					};
					parseResult.Add (plainTextNode);
					line = line.Substring (classNameIndex);
				}
			}
			if (string.IsNullOrEmpty(line) == false)
			{
				PlainTextNode lastTextNode = new PlainTextNode () { Text = line };
				parseResult.Add (lastTextNode);
			}
			return parseResult;
		}
	}

	public class TemplateCustomizer
	{
		private List<List<INode>> _templateData;

		public TemplateCustomizer (List<List<INode>> templateData)
		{
			_templateData = templateData;
		}

		public void RenameClassName (string typeName)
		{
			if (_templateData == null) {
				return;
			}
			typeName = typeName.Replace (" ", "_").Replace ("\\t", "_");
			foreach (var line in _templateData) {
				if (line == null) {
					continue;
				}
				foreach (var node in line) {
					if (node == null) {
						continue;
					}
					if (node is ClassNameNode) {
						var classNameNode = (ClassNameNode)node;
						classNameNode.Text = typeName;
					}
				}
			}
		}

		public string BuildCode ()
		{
			if (_templateData == null) {
				return string.Empty;
			}
			var codeBuilder = new System.Text.StringBuilder ();
			for (var i = 0; i < _templateData.Count; ++i) {
				codeBuilder.AppendLine (BuildCode (_templateData [i]));
			}
			return codeBuilder.ToString ();
		}

		private string BuildCode (List<INode> line)
		{
			if (line == null) {
				return string.Empty;
			}
			var codeBuilder = new System.Text.StringBuilder ();
			for (var i = 0; i < line.Count; ++i) {
				codeBuilder.Append (line [i].Text);
			}
			return codeBuilder.ToString ();
		}
	}
}
