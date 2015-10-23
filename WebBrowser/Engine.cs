﻿using System;
using System.IO;
using System.Threading;
using WebBrowser.Dom;
using WebBrowser.Environment;
using WebBrowser.ResourceProviders;
using WebBrowser.ScriptExecuting;
using WebBrowser.Tools;

namespace WebBrowser
{
	public class Engine: IDisposable
	{
		private Document _document;
		public IResourceProvider ResourceProvider { get; private set; }
		internal IScriptExecutor ScriptExecutor { get; private set; }

		public DocumentScripting Scripting	{get; private set;}

		public Document Document
		{
			get { return _document; }
			private set
			{
				_document = value;

				if (Scripting != null)
				{
					Scripting.Dispose ();
					Scripting = null;
				}

				if (_document != null)
				{
					Scripting = new DocumentScripting (_document, ScriptExecutor, ResourceProvider);
				}

				if (DocumentChanged != null)
					DocumentChanged();
			}
		}

		public Console Console { get; private set; }
		public Window Window { get; private set; }

		public SynchronizationContext Context { get; private set; }

		internal Engine(IResourceProvider resourceProvider)
		{
			Context = new SignleThreadSynchronizationContext();

			ResourceProvider = resourceProvider;
			Console = new Console();
			Window = new Window(Context, this);
			ScriptExecutor = new ScriptExecutor(this);
			ScriptExecutor.OnException += ex => Console.Log("Unhandled exception in script: " + ex.Message);
		}

		public Engine() : this(new ResourceProvider()) { }

		public Uri Uri { get; private set; }

		public void OpenUrl(string path)
		{
			ScriptExecutor.Clear();
			Uri = new Uri(path);
			ResourceProvider.Root = Uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
			var task = ResourceProvider.GetResourceAsync(path);
			task.Wait();
			var resource = task.Result;
			if (resource.Type == ResourceTypes.Html)
			{
				var httpResponse = resource as HttpResponse;
				if(httpResponse != null)
					Uri = httpResponse.Uri;

				Load(resource.Stream);
			}
		}

		public void Load(Stream stream)
		{
			//todo: fix protocol
			if(Uri == null)
				Uri = new Uri("http://localhost");

			Document = new Document(Context, Window);
			Document.OnNodeException += (node, exception) => Console.Log("Node event handler exception: " + exception.Message);
			//todo: clear js runtime context
			
			DocumentBuilder.Build(Document, stream);
			Document.Complete();
		}

		public event Action DocumentChanged;
		public void Dispose()
		{
			var disposableContext = Context as IDisposable;
			if (disposableContext != null)
				disposableContext.Dispose();
		}
    }
}
