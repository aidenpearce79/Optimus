﻿#if NUNIT
using System.IO;
using System.Text;
using Knyaz.Optimus.Dom.Css;
using NUnit.Framework;

namespace Knyaz.Optimus.Tests.Dom.Css
{
	[TestFixture]
	public class CssSelectorTests
	{
		private Engine Load(string html)
		{
			var engine = new Engine() { ComputedStylesEnabled = true };
			engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(html)));
			return engine;
		}

		[TestCase("*", @"<body><div class=""pointsPanel""><h2><strong name=match></strong></h2></div></body>")]
		[TestCase(".pointsPanel strong", @"<body><div class=""pointsPanel""><strong name=match></strong></div></body>")]
		[TestCase(".pointsPanel strong", @"<body><div class=""pointsPanel""><h2><strong name=match></strong></h2></div></body>")]
		[TestCase(".pointsPanel h2 > strong", @"<body><div class=""pointsPanel""><h2><strong name=match></strong></h2></div></body>")]
		[TestCase(".pointsPanel *", @"<body><div class=""pointsPanel""><h2><strong name=match></strong></h2></div></body>")]
		[TestCase("div *", "<body name=nomatch><div name=nomatch><strong name=match><h2 name=match>a</h2></strong></div></body>")]
		[TestCase(".button.save", "<body><div class='button save' name=match></div></body>")]
		[TestCase(".button.save", "<body><div class='button' name=nomatch></div></body>")]
		[TestCase(".button.save", "<body><div class='save' name=nomatch></div></body>")]
		[TestCase(".button .save", "<body><div class='button save' name=nomatch></div></body>")]
		[TestCase(".resultsTable table thead tr","<div class='resultsTable'><table><thead><tr name=match><td name=nomatch></td></tr></thead></table></div>")]
		[TestCase("ul.left", "<ul class='left' name=match><li class='left' name=nomatch></li></ul><ul name=nomatch></ul>")]
		[TestCase("ul .left", "<ul class='left' name=nomatch><li class='left' name=match></li></ul><ul name=nomatch></ul>")]
		[TestCase("#m.left", "<ul id=m class='left' name=match><li class='left' name=nomatch></li></ul><ul name=nomatch></ul>")]
		[TestCase("#m .left", "<ul id=m class='left' name=nomatch><li class='left' name=match></li></ul><ul name=nomatch></ul>")]
		[TestCase("#m div", "<div name=nomatch></div><div id=m name=nomatch><div name=match></div></div>")]
		[TestCase("#m > div", "<div name=nomatch></div><div id=m name=nomatch><div name=match><div name=nomatch></div></div></div>")]
		[TestCase("#m > div a", "<div name=nomatch></div><div id=m name=nomatch><div name=nomatch><a name=match></a></div></div>")]
		[TestCase("#m > div a,\r\n#m > div span", "<div name=nomatch></div><div id=m name=nomatch><div name=nomatch><a name=match></a><span name=match></span><div name=nomatch</div></div></div>")]
		[TestCase(".form-signin", "<div class='form' name=nomatch></div><div class='form-signin'name=match></div>")]
		[TestCase("[custom]", "<div name=nomatch></div><div custom name=match></div><div custom=123 name=match></div>")]
		[TestCase("[custom=\"123\"]", "<div name=nomatch custom></div><div name=match custom=123></div>")]
		[TestCase("[custom~=\"123\"]", "<div name=nomatch custom></div><div name=match custom=123></div><div name=match custom='123 456'></div>")]
		[TestCase("[custom~=\"123 456\"]", "<div name=nomatch custom=123></div><div name=nomatch custom=\"123 456\"></div>")]
		[TestCase("[custom~=\"\"]", "<div name=nomatch custom></div><div name=nomatch custom=\"\"></div>")]
		[TestCase("[custom|=\"123\"]", "<div name=nomatch custom=1234></div><div name=match custom=123></div><div name=match custom=\"123-456\"></div>")]
		[TestCase("[custom^=\"123\"]", "<div name=nomatch custom=0123></div><div name=match custom=1234></div><div name=match custom=123></div><div name=match custom=\"123-456\"></div>")]
		[TestCase("[custom$=\"123\"]", "<div name=match custom=0123></div><div name=nomatch custom=1234></div><div name=match custom=123></div><div name=match custom=\"456-123\"></div>")]
		[TestCase("[custom*=\"123\"]", "<div name=nomatch custom=12></div><div name=match custom=0123></div><div name=match custom=1234></div><div name=match custom=123></div><div name=match custom=\"456-123\"></div>")]
		[TestCase("[=\"asd\"]", "<div name=nomatch></div>")]
		[TestCase("[=]", "<div name=nomatch></div>")]
		[TestCase("p+p", "<p name=nomatch></p><p name=match></p>text<p name=match></p>")]
		[TestCase("p + p", "<p name=nomatch></p><p name=match></p>text<p name=match></p>")]
		[TestCase("div+p", "<p name=nomatch></p><p name=nomatch></p><div></div><p name=match></p>")]
		[TestCase("p+p", "TEXT<p name=nomatch></p>")]
		public void MatchChildTest(string selectorText, string html)
		{
			var engine = Load(html);
			var selector = new CssSelector(selectorText);
			var matchElts = engine.Document.GetElementsByName("match");
			foreach (var matchElt in matchElts)
			{
				Assert.IsTrue(selector.IsMatches(matchElt), "Have to match: " + matchElt.ToString());
			}


			var notMatchElt = engine.Document.GetElementsByName("nomatch");
			foreach (var elt in notMatchElt)
			{
				Assert.IsFalse(selector.IsMatches(elt), elt.ToString());
			}
		}

		[TestCase("*", 0)]
		[TestCase("li", 1)]
		[TestCase("li:first-line", 2)]
		[TestCase("ul li", 2)]
		[TestCase("ul ol+li", 3)]
		[TestCase("h1 + *[rel=up]", 0x0101)]  /* a=0 b=0 c=1 d=1 -> specificity = 0,0,1,1 */
		[TestCase("ul ol li.red", 0x0103)]  /* a=0 b=0 c=1 d=3 -> specificity = 0,0,1,3 */
		[TestCase("li.red.level", 0x0201)]  /* a=0 b=0 c=2 d=1 -> specificity = 0,0,2,1 */
		[TestCase("#x34y", 0x010000)]           /* a=0 b=1 c=0 d=0 -> specificity = 0,1,0,0 */
 		public void SpecifityTests(string selectorText, int expectedSpecifity)
		{
			Assert.AreEqual(expectedSpecifity, new CssSelector(selectorText).Specifity);
		}
	}
}
#endif