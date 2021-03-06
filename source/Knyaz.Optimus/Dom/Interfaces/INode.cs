using Knyaz.Optimus.Dom.Elements;
using System.Collections.Generic;

namespace Knyaz.Optimus.Dom.Interfaces
{ 
	public interface INode
 	{
		Document OwnerDocument { get; }
		Node AppendChild(Node node);
		Node RemoveChild(Node node);
		Node InsertBefore(Node newChild, Node refNode);
		bool HasChildNodes { get; }
		Node ReplaceChild(Node newChild, Node oldChild);
		Node FirstChild { get; }
		Node LastChild { get; }
		Node NextSibling { get; }
		Node PreviousSibling { get; }
		Node ParentNode { get; }
		Node CloneNode();
		int NodeType { get; }
		string NodeName { get; }
		int CompareDocumentPosition(Node node);
		IList<Node> ChildNodes { get; }
 	}
 }
