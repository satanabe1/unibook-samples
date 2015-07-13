using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

/// <summary>
/// コード生成系補助
/// </summary>
namespace CodeBuilder
{
	public class Modifier : System.IComparable
	{
		public static readonly Modifier Abstract = new Modifier ("abstract", 3);
		public static readonly Modifier Pubic = new Modifier ("public", 5);
		public static readonly Modifier Protected = new Modifier ("protected", 5);
		public static readonly Modifier Private = new Modifier ("private", 5);
		public static readonly Modifier Static = new Modifier ("static", 10);
		public static readonly Modifier Const = new Modifier ("const", 10);
		public static readonly Modifier Readonly = new Modifier ("readonly", 10);

		public string Name { get; private set; }

		public int Priority { get; private set; }

		private Modifier (string name, int priority)
		{
			Name = name;
			Priority = priority;
		}

		public override string ToString ()
		{
			return string.Format ("[Modifier: Name={0}, Priority={1}]", Name, Priority);
		}

		public int CompareTo (object obj)
		{
			if ((obj is Modifier) == false) {
				return 1;
			}
			return this.Priority - ((Modifier)obj).Priority;
		}
	}

	/// <summary>
	/// Using declaration.
	/// </summary>
	public class UsingDeclaration
	{
		/// <summary>
		/// ex) UsingTarget = "System.Text"; => using System.Text;
		/// </summary>
		public string UsingTarget;
		/// <summary>
		/// ex) UsingTarget = "System.Text"; UsingName = "Txt"; // => using Txt = System.Text;
		/// </summary>
		public string UsingName;

		public UsingDeclaration ()
		{
		}

		public string BuildCode ()
		{
			StringBuilder sb = new StringBuilder ();
			if (string.IsNullOrEmpty (UsingTarget)) {
				return string.Empty;
			}
			sb.Append ("using ");
			if (string.IsNullOrEmpty (UsingName)) {
				sb.Append (UsingTarget);
			} else {
				sb.Append (UsingName).Append (" = ").Append (UsingTarget);
			}
			sb.Append (";");
			return sb.ToString ();
		}
	}

	/// <summary>
	/// Attribute text.
	/// </summary>
	public class AttributeText : System.IComparable
	{
		public string Name { get; set; }

		public List<object> ParameterList { get; private set; }

		public AttributeText ()
		{
			ParameterList = new List<object> ();
		}

		public string BuildCode ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("[").Append (Name).Append ("(");
			var enumerator = ParameterList.GetEnumerator ();
			bool moveNext = enumerator.MoveNext ();
			while (moveNext) {
				if (enumerator.Current is System.Boolean) {
					sb.Append (enumerator.Current.ToString ().ToLower ());
				} else if (enumerator.Current == null) {
					sb.Append ("null");
				} else if (enumerator.Current is string) {
					sb.Append ("\"").Append (enumerator.Current).Append ("\"");
				} else {
					sb.Append (enumerator.Current);
				}
				moveNext = enumerator.MoveNext ();
				if (moveNext) {
					sb.Append (", ");
				}
			}
			sb.Append (")]");
			return sb.ToString ();
		}

		public int CompareTo (object obj)
		{
			if ((obj is AttributeText) == false) {
				return 1;
			}
			string name = Name ?? string.Empty;
			string otherName = ((AttributeText)obj).Name ?? string.Empty;
			return name.CompareTo (otherName);
		}
	}

	/// <summary>
	/// Method declaration.
	/// </summary>
	public class MethodDeclaration
	{
		public string Name { get; set; }

		public List<AttributeText> AttributeList { get; private set; }

		public List<Modifier> ModifierList { get; private set; }

		public string ReturnTypeName { get; set; }

		public List<KeyValuePair<string, string>> ParameterList { get; private set; }

		public string MethodBody { get; set; }

		public MethodDeclaration ()
		{
			AttributeList = new List<AttributeText> ();
			ModifierList = new List<Modifier> ();
			ParameterList = new List<KeyValuePair<string, string>> ();
		}

		public string BuildCode ()
		{
			StringBuilder sb = new StringBuilder ();
			// attributes
			AttributeList.Sort ();
			AttributeList.ForEach ((attribute) => sb.AppendLine (attribute.BuildCode ()));
			// modifiers
			ModifierList.Sort ();
			foreach (var modifier in ModifierList) {
				sb.Append (modifier.Name).Append (" ");
			}
			// return type and method name
			if (string.IsNullOrEmpty (ReturnTypeName)) {
				sb.Append ("void ");
			} else {
				sb.Append (ReturnTypeName).Append (" ");
			}
			sb.Append (Name);
			// parameters
			sb.Append ("(");
			var enumerator = ParameterList.GetEnumerator ();
			bool moveNext = enumerator.MoveNext ();
			while (moveNext) {
				sb.Append (enumerator.Current.Key).Append (" ").Append (enumerator.Current.Value);
				moveNext = enumerator.MoveNext ();
				if (moveNext) {
					sb.Append (", ");
				}
			}
			sb.Append (")");
			// method body
			if (ModifierList.Contains (Modifier.Abstract) == false) {
				sb.AppendLine ("{");
				sb.AppendLine (MethodBody);
				sb.Append ("}");
			} else {
				sb.Append (";");
			}
			return sb.ToString ();
		}
	}

	/// <summary>
	/// Type declaration.
	/// </summary>
	public class TypeDeclaration
	{
		public string Name { get; set; }

		public List<UsingDeclaration> UsingDeclarationList { get; private set; }

		public List<Modifier> ModifierList { get; private set; }

		public List<MethodDeclaration> MethodDeclarationList { get; private set; }

		public TypeDeclaration ()
		{
			UsingDeclarationList = new List<UsingDeclaration> ();
			ModifierList = new List<Modifier> ();
			MethodDeclarationList = new List<MethodDeclaration> ();
		}

		public string BuildCode ()
		{
			StringBuilder sb = new StringBuilder ();
			// usings
			UsingDeclarationList.ForEach ((usingDeclaration) => sb.AppendLine (usingDeclaration.BuildCode ()));
			// modifiers
			ModifierList.Sort ();
			foreach (var modifier in ModifierList) {
				sb.Append (modifier.Name).Append (" ");
			}
			sb.Append ("class ").AppendLine (Name);
			sb.AppendLine ("{");
			foreach (var method in MethodDeclarationList) {
				sb.AppendLine (method.BuildCode ());
			}
			sb.AppendLine ("}");
			return sb.ToString ();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class CodePretty
	{
		public string Pretty (string plain, int baseIndent = 0)
		{
			if (string.IsNullOrEmpty (plain)) {
				return plain;
			}
			int indentLevel = baseIndent;
			StringBuilder sb = new StringBuilder ();
			string[] lines = plain.Split (System.Environment.NewLine.ToCharArray ());
			foreach (var line in lines) {
				indentLevel -= CountChar (line, '}');
				sb.AppendLine (Indent (line, indentLevel));
				indentLevel += CountChar (line, '{');
			}
			return sb.ToString ();
		}

		private int CountChar (string txt, char findChar)
		{
			int hitCount = 0;
			if (string.IsNullOrEmpty (txt)) {
				return 0;
			}
			foreach (var c in txt) {
				hitCount += ((c == findChar) ? 1 : 0);
			}
			return hitCount;
		}

		private string Indent (string txt, int indentLevel)
		{
			if (string.IsNullOrEmpty (txt)) {
				return txt;
			}
			StringBuilder sb = new StringBuilder (txt);
			for (var i = 0; i < indentLevel; ++i) {
				sb.Insert (0, "\t");
			}
			return sb.ToString ();
		}
	}
}
