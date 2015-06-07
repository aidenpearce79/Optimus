﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebBrowser.Dom.Elements;
using WebBrowser.Environment;
using WebBrowser.ScriptExecuting;

namespace WebBrowser.Dom
{
	class StubSynchronizationContext : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object state)
		{
			d(state);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			d(state);
		}
	}

	public enum DocumentReadyStates
	{
		Loading, Interactive, Complete
	}

	/// <summary>
	/// http://www.w3.org/html/wg/drafts/html/master/dom.html#document
	/// http://dev.w3.org/html5/spec-preview/dom.html
	/// all idls http://www.w3.org/TR/REC-DOM-Level-1/idl-definitions.html
	/// </summary>
	public class Document : DocumentFragment, IDocument
	{
		private readonly IResourceProvider _resourceProvider;
		public readonly SynchronizationContext Context;
		private readonly IScriptExecutor _scriptExecutor;

		internal Document() :this(null, null, null, null)
		{
			ReadyState = DocumentReadyStates.Loading;
		}

		internal Document(IResourceProvider resourceProvider, SynchronizationContext context, IScriptExecutor scriptExecutor, 
			Window window):base(null)
		{
			_resourceProvider = resourceProvider;
			Context = context ?? new StubSynchronizationContext();
			_scriptExecutor = scriptExecutor;
			_unresolvedDelayedResources = new List<IDelayedResource>();
			NodeType = DOCUMENT_NODE;

			DocumentElement = new Element(this, "html"){ParentNode = this};
			ChildNodes = new List<Node> { DocumentElement };

			EventTarget = new EventTarget(this, () => window);
		}

		public Element DocumentElement { get; private set; }
		public DocumentReadyStates ReadyState { get; private set; }
		public override string NodeName { get { return "#document"; }}

		private readonly List<IDelayedResource> _unresolvedDelayedResources;
		
		public void Write(string text)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				DocumentBuilder.Build(this, stream);
			}
		}

		internal void Complete()
		{
			Trigger("DOMContentLoaded");
			//todo: check is it right
			ReadyState = DocumentReadyStates.Complete;

			ResolveDelayedContent();

			RunScripts(ChildNodes.SelectMany(x => x.Flatten()).OfType<Script>());
		}

		private void Trigger(string type)
		{
			var evt = CreateEvent("Event");
			evt.InitEvent(type, false, false);
			Context.Post(state => DispatchEvent(evt), null);
		}

		public void ResolveDelayedContent()
		{
			Task.WaitAll(_unresolvedDelayedResources.Where(x => !x.Loaded).Select(x => x.LoadAsync(_resourceProvider)).ToArray());
			_unresolvedDelayedResources.Clear();
			Trigger("load");
		}

		internal void RunScripts(IEnumerable<Script> scripts)
		{
				//todo: what we should do if some script changes ChildNodes?
				//todo: optimize (create queue of not executed scripts);
			foreach (var documentElement in scripts.ToArray())
			{
				if (documentElement.Executed) continue;
				if(!string.IsNullOrEmpty(documentElement.Text))
					_scriptExecutor.Execute(documentElement.Type, documentElement.Text);
				documentElement.Executed = true;
			}
		}

		public Element CreateElement(string tagName)
		{
			if(tagName == null)
				throw new ArgumentNullException("tagName");
			if(tagName == string.Empty)
				throw new ArgumentOutOfRangeException("tagName");

			var invariantTagName = tagName.ToLowerInvariant();

			switch (invariantTagName)
			{
				//todo: fill the list
				case "div":
				case "span":
				case "b": return new HtmlElement(this, tagName);
				case "input": return new HtmlInputElement(this);
				case "script": return new Script(this);
				case "head":return new Head(this);
				case"body":return new Body(this);
			}

			return new Element(this, tagName) { OwnerDocument = this};
		}

		public Attr CreateAttribute(string name)
		{
			return new Attr(name){OwnerDocument = this};
		}

		public Element GetElementById(string id)
		{
			//todo: create index;
			return DocumentElement.Flatten().OfType<Element>().FirstOrDefault(x => x.Id == id);
		}

		public DocumentFragment CreateDocumentFragment()
		{
			return new DocumentFragment(this);
		}

		public Text CreateTextNode(string data)
		{
			return new Text{Data = data, OwnerDocument = this};	
		}

		public Comment CreateComment(string data)
		{
			return new Comment { Data = data, OwnerDocument = this };
		}

		public Element Body
		{
			get { return DocumentElement.GetElementsByTagName("body").FirstOrDefault(); }
		}

		public Head Head
		{
			get { return (Head)DocumentElement.GetElementsByTagName("head").FirstOrDefault(); }
		}

		public Event CreateEvent(string type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if(type == "Event")
				return new Event();

			if(type == "CustomEvent")
				return new CustomEvent();

			if(type == "MutationEvent")
				return new MutationEvent();

			//todo: UIEvent

			throw new NotSupportedException("Specified event type is not supported: " + type);
		}

		public event Action<Node, Node> DomNodeInserted;

		internal void HandleNodeAdded(Node node, Node newChild)
		{
			if (DomNodeInserted != null)
				DomNodeInserted(node, newChild);

			foreach (var script in newChild.Flatten().OfType<Script>())
			{
				if (!script.Async && script.Defer && ReadyState != DocumentReadyStates.Complete)
				{
					_unresolvedDelayedResources.AddRange(newChild.Flatten()
						.OfType<IDelayedResource>()
						.Where(delayed => delayed != null && delayed.HasDelayedContent && !delayed.Loaded));
				}
				else
				{
					Task task = null;
					if (script.HasDelayedContent)
					{
						task = script
							.LoadAsync(_resourceProvider)
							.ContinueWith((t, s) => ((Script) s).Execute(_scriptExecutor), script);
					}
					else if (!string.IsNullOrEmpty(script.Text))
					{
						task = new Task(s => ((Script)s).Execute(_scriptExecutor), script);
						task.Start(TaskScheduler.Default);
					}
					if (task != null)
					{
						if (!script.Async)
							task.Wait();
					}
				}
			}
		}

		internal void HandleNodeEventException(Node node, Exception exception)
		{
			if (OnNodeException != null)
				OnNodeException(node, exception);
		}

		public event Action<Node, Exception> OnNodeException;
	}

	[DomItem]
	public interface IDocument
	{
		Element CreateElement(string tagName);
		Element DocumentElement { get; }
		void Write(string text);
		Event CreateEvent(string type);
		Head Head { get; }
		Element Body { get; }
		Comment CreateComment(string data);
		Text CreateTextNode(string data);
		DocumentFragment CreateDocumentFragment();
		Element GetElementById(string id);
		Attr CreateAttribute(string name);
	}
}
