using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TemplateLabelConfig
{
	public const string LabelFileName = "_Label.txt";
	public const string Delimiter = ":";
	private readonly List<string> _commentStartList = new List<string> () { "#", "//" };
	public readonly Dictionary<string, string> _fileNameToLabelMap = new Dictionary<string, string> ();

	public bool IsValid { get; private set; }

	public TemplateLabelConfig ()
	{
	}

	public TemplateLabelConfig (string labelPath) : this()
	{
		Parse (labelPath);
	}

	public bool HasLabel (string templateFileName)
	{
		return _fileNameToLabelMap.ContainsKey (templateFileName);
	}

	public string GetLabel (string templateFileName)
	{
		return _fileNameToLabelMap.ContainsKey (templateFileName)
			? _fileNameToLabelMap [templateFileName] : string.Empty;
	}

	public void Parse (string labelPath)
	{
		IsValid = false;
		if (File.Exists (labelPath) == false) {
			return;
		}
		foreach (var line in File.ReadAllLines(labelPath)) {
			string reducedLine = line;
			reducedLine = ReduceLineHeadBlank (reducedLine);
			reducedLine = ReduceLineEndBlank (reducedLine);
			if (string.IsNullOrEmpty (reducedLine) || IsCommentLine (reducedLine)) {
				continue;
			}
			string templateFileName = ParseTemplateFileName (reducedLine);
			string templateLabel = ParseTemplateLabel (reducedLine);
			if (string.IsNullOrEmpty (templateFileName) || string.IsNullOrEmpty (templateLabel)) {
				continue;
			}
			_fileNameToLabelMap [templateFileName] = templateLabel;
		}
		IsValid = true;
	}

	private string ReduceLineHeadBlank (string src)
	{
		if (string.IsNullOrEmpty (src)) {
			return src;
		}
		int length = src.Length;
		for (var i = 0; i < length; ++i) {
			if (IsBlankChar (src [i]) == false) {
				return (i == 0) ? src : src.Substring (i);
			}
		}
		return string.Empty;
	}

	private string ReduceLineEndBlank (string src)
	{
		if (string.IsNullOrEmpty (src)) {
			return src;
		}
		int lastIndex = src.Length - 1;
		for (var i = lastIndex; 0 < i; --i) {
			if (IsBlankChar (src [i]) == false) {
				return (i == lastIndex) ? src : src.Substring (0, src.Length - i);
			}
		}
		return string.Empty;
	}

	private bool IsBlankChar (char c)
	{
		return (c == ' ') || (c == '\t');
	}

	private bool IsCommentLine (string line)
	{
		if (string.IsNullOrEmpty (line)) {
			return false;
		}
		return _commentStartList.Any ((commentStarts) => line.StartsWith (commentStarts));
	}

	private string ParseTemplateFileName (string line)
	{
		int delimiterIndex;
		if (TryGetDelimiterIndex (line, out delimiterIndex) == false) {
			return string.Empty;
		}
		return line.Substring (0, delimiterIndex);
	}

	private string ParseTemplateLabel (string line)
	{
		int delimiterIndex;
		if (TryGetDelimiterIndex (line, out delimiterIndex) == false) {
			return string.Empty;
		}
		return line.Substring (delimiterIndex + 1);
	}

	private bool TryGetDelimiterIndex (string line, out int delimiterIndex)
	{
		if (string.IsNullOrEmpty (line)) {
			delimiterIndex = -1;
			return false;
		}
		delimiterIndex = line.IndexOf (Delimiter);
		if (delimiterIndex < 1) { // not 0
			delimiterIndex = -1;
			return false;
		}
		if (delimiterIndex == (line.Length - 1)) {
			delimiterIndex = -1;
			return false;
		}
		return true;
	}
}

