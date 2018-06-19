﻿using NUnit.Framework;

namespace Knyaz.Optimus.Tests.JsTests
{
	[TestFixture]
	class DocumentJsTests
	{
		[TestCase("ImplementationCreateHtmlDocument")]
		[TestCase("ImplementationCreateDocumentType")]
		[TestCase("ImplementationCreateDocumentWithDocTypeSvg")]
		[TestCase("RemoveDocType")]
		[TestCase("TextContentIsNull")]
		[TestCase("DefaultViewIsWindow")]
		[TestCase("DomBuildOrder")]
		[TestCase("SetBody")]
		[TestCase("SetBodyDiv")]
		[TestCase("SetBodyNull")]
		public void DocumentTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("CommentRemove")]
		[TestCase("TextRemove")]
		public void NodeTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("Clone")]
		public void DocumentFragmentTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("AppendScriptTwice")]
		[TestCase("AppendScriptClone")]
		[TestCase("AppendClonesOfScript")]
		[TestCase("OnloadOnEmbeddedScript")]
		[TestCase("OnloadOnExternalScript")]
		[TestCase("CloneScript")]
		[TestCase("DeepCloneScript")]
		[TestCase("TextContent")]
		[TestCase("ParseFromHtml")]
		[TestCase("SetInnerHtml")]
		[TestCase("TextOfExternal")]
		public void ScriptTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("Clone")]
		[TestCase("TextContentIsNull")]
		public void DocTypeTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("CallThisIsObj")]
		[TestCase("CallThisIsNull")]
		[TestCase("CallThisIsUndefined")]
		[TestCase("CallThisIsNullStrict")]
		[TestCase("CallThisIsUndefinedStrict")]
		[TestCase("CompareObjectWithWindow")]
		[TestCase("CompareObjectWithWindowInverted")]
		[TestCase("CallAndCompareThisIsUndefined")]
		public void JavaScriptTests(string testName) => JsTestsRunner.Run(testName);
		
		[TestCase("Clone")]
		[TestCase("TextContent")]
		[TestCase("AppendData")]
		[TestCase("AppendDataNull")]
		[TestCase("DeleteData")]
		[TestCase("InsertData")]
		[TestCase("ReplaceData")]
		[TestCase("SubstringData")]
		[TestCase("LengthTest")]
		[TestCase("AppendChildThrows")]
		public void CommentTests(string testName) => JsTestsRunner.Run(testName);

		[TestCase("ChildElements")]
		[TestCase("DefaultEnctype")]
		public void FormTests(string testName) => JsTestsRunner.Run(testName);
		
		[TestCase("EventConstructor")]
		[TestCase("EventConstructorWithInit")]
		[TestCase("AddEventListenerCallOnce")]
		[TestCase("AddEventListenerPassiveTrue")]
		[TestCase("AddEventListenerPassiveFalse")]
		[TestCase("AddEventListenerCaptureOptionTrue")]
		[TestCase("AddEventListenerCaptureOptionFalse")]
		[TestCase("AddEventListenerTwice")]
		[TestCase("AddEventListenerTwiceAndRemove")]
		public void EventTests(string testName) => JsTestsRunner.Run(testName);
	}
}
