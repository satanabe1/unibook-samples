using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CustomTemplate
{
	public static class TemplateKeyword
	{
		public const string ClassName = "$ClassName$";
		public const string Namespace = "$Namespace$";
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

	public class NamespaceNameNode : VariableTextNode
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
			parseResult.Add (new PlainTextNode () { Text = line });
			parseResult = ParseLineNodes<ClassNameNode> (parseResult, TemplateKeyword.ClassName);
			parseResult = ParseLineNodes<NamespaceNameNode> (parseResult, TemplateKeyword.Namespace);
			return parseResult;
		}

		protected List<INode> ParseLineNodes<T> (List<INode> nodes, string keyword)
			where T : VariableTextNode, new ()
		{
			var retList = new List<INode> ();
			for (var i = 0; i < nodes.Count; ++i) {
				if ((nodes [i] is PlainTextNode) == false) {
					retList.Add (nodes [i]);
					continue;
				}
				string nodeText = nodes [i].Text;
				int keywordIndex = -1;
				while ((keywordIndex = nodeText.IndexOf(keyword)) > -1) {
					if (keywordIndex == 0) {
						T classNameNode = new T () { PlainText = keyword, Text = keyword };
						retList.Add (classNameNode);
						nodeText = nodeText.Substring (keyword.Length);
					} else {
						string text = nodeText.Substring (0, keywordIndex);
						retList.Add (new PlainTextNode () { Text = text });
						nodeText = nodeText.Substring (keywordIndex);
					}
				}
				if (string.IsNullOrEmpty (nodeText) == false) {
					retList.Add (new PlainTextNode () { Text = nodeText });
				}
			}
			return retList;
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
			typeName = typeName.Replace (" ", "_").Replace ("\\t", "_");
			ChangeVariableTextNodeValue<ClassNameNode> (typeName);
		}

		public void RenameNamespace (string namespaceName)
		{
			namespaceName = namespaceName.Replace (' ', '_').Replace ("\\t", "_").Replace (System.IO.Path.DirectorySeparatorChar, '.');
			ChangeVariableTextNodeValue<NamespaceNameNode> (namespaceName);
		}

		public void ChangeVariableTextNodeValue<T> (string text)
			where T : VariableTextNode
		{
			if (_templateData == null) {
				return;
			}
			text = text ?? string.Empty;
			foreach (var line in _templateData) {
				if (line == null) {
					continue;
				}
				foreach (var node in line) {
					if (node == null) {
						continue;
					}
					if (node is T) {
						var classNameNode = (T)node;
						classNameNode.Text = text;
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
