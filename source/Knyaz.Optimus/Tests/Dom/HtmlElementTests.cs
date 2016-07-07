﻿#if NUNIT
using Knyaz.Optimus.Dom;
using Knyaz.Optimus.Dom.Elements;
using NUnit.Framework;

namespace Knyaz.Optimus.Tests.Dom
{
	[TestFixture]
	public class HtmlElementTests
	{
		[Test]
		public void SetStyleFromAttribute()
		{
			var doc = new Document();
			var elem = doc.CreateElement("div") as HtmlElement;
			Assert.IsNotNull(elem);
			elem.SetAttribute("style","color:black");
			Assert.AreEqual("black", elem.Style.GetPropertyValue("color"));
		}

		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void CloneNode(bool deep, int expectedChildCount)
		{
			var document = new Document();
			document.Write("<html><body><div id='p1'><span id='s'>Span text</span></div></body></html>");
			var span = document.GetElementById("s");

			document.Assert(doc => 
				doc.Body.ChildNodes.Count == 1 &&
				doc.GetElementById("p1").ChildNodes.Count == 1 &&
				doc.GetElementById("s").ChildNodes.Count == 1);

			var clone = span.CloneNode(deep) as HtmlElement;
			Assert.IsNotNull(clone);

			//sate of all old elements should be the same as before
			document.Assert(doc =>
				doc.Body.ChildNodes.Count == 1 &&
				doc.GetElementById("p1").ChildNodes.Count == 1 &&
				doc.GetElementById("s").ChildNodes.Count == 1);

			clone.Assert(c => 
				c.ChildNodes.Count == expectedChildCount &&
				c.OwnerDocument == document &&
				c.ParentNode == null &&
				c.Id == "s");
		}

		[Test]
		public void DeepClone()
		{
			var document = new Document();
			document.Write("<html><body><div id='p1'><div id=p2><span id='s'>Span text</span></div></div></body></html>");

			var p1 = document.GetElementById("p1");

			var clone = p1.CloneNode(true);
			Assert.AreEqual(1, clone.ChildNodes.Count);
			Assert.AreEqual(1, clone.ChildNodes[0].ChildNodes.Count);
		}

		[Test]
		public void Contains()
		{
			var document = new Document();
			var elt = document.CreateElement("div");
			var sub = document.CreateElement("span");
			elt.AppendChild(sub);
			var subsub = document.CreateElement("script");
			sub.AppendChild(subsub);
			Assert.IsTrue(elt.Contains(sub));
			Assert.IsTrue(elt.Contains(subsub));
			Assert.IsFalse(sub.Contains(elt));
		}

		[Test]
		public void TextContent()
		{
			var document = new Document();
			document.Write("<html><body><ul id=\"myList\"><li id=\"item1\">Coffee</li><li id=\"item2\">Tea</li></ul></body></html>");
			var ul = document.GetElementById("myList");
			Assert.AreEqual("Coffee Tea", ul.TextContent);
		}
	}
}
#endif