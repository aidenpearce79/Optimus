﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using WebBrowser.Dom;
using WebBrowser.Dom.Elements;
using WebBrowser.Environment;
using WebBrowser.ResourceProviders;
using WebBrowser.ScriptExecuting;

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

		internal Engine(IResourceProvider resourceProvider)
		{
			ResourceProvider = resourceProvider;
			Console = new Console();
			Window = new Window(() => Document, this);
			ScriptExecutor = new ScriptExecutor(this);
			ScriptExecutor.OnException += ex => Console.Log("Unhandled exception in script: " + ex.Message);
			Document = new Document(Window);
			Document.OnNodeException += (node, exception) => Console.Log("Node event handler exception: " + exception.Message);
			Document.OnFormSubmit+=OnFormSubmit;
		}

		/// <summary>
		/// todo: rewrite and complete the stuff
		/// </summary>
		/// <param name="form"></param>
		private async void OnFormSubmit(HtmlFormElement form)
		{
			if (string.IsNullOrEmpty(form.Action))
				return;
			//todo: we should consider the case when button clicked and the button have 'method' or other attributes.
			
			var dataElements = form.Elements.OfType<IFormElement>().Where(x => !string.IsNullOrEmpty(x.Name));

			var replaceSpaces = form.Method != "post" || form.Enctype != "multipart/form-data";
			
			var data = string.Join("&", dataElements.Select(x => 
				x.Name + "=" + (x.Value != null ? (replaceSpaces ? x.Value.Replace(' ', '+') : x.Value) : "")
				));

			if (form.Method.ToLowerInvariant() == "get")
			{
				//todo: escape specialchars
				var url = form.Action + "?" + data;

				Document = new Document(Window);
				var request = ResourceProvider.CreateRequest(url);
				var response = await ResourceProvider.GetResourceAsync(request);
				LoadFromResponse(response);
			}
			else
			{
				if (!string.IsNullOrEmpty(form.Target))
				{
					var targetFrame = Document.GetElementById(form.Target) as HtmlIFrameElement;
					if (targetFrame != null)
					{
						var document = new Document(Window);
						targetFrame.ContentDocument = document;

						var request = ResourceProvider.CreateRequest(form.Action);
						var httpRequest = request as HttpRequest;
						if (httpRequest != null)
						{
							//todo: use right encoding and enctype
							httpRequest.Data = Encoding.UTF8.GetBytes(data);
						}

						var response = await ResourceProvider.GetResourceAsync(request);

						//what should we do if the frame is not found?
						if (response.Type == ResourceTypes.Html)
						{
							//todo: clear js runtime context
							DocumentBuilder.Build(document, response.Stream);
							document.Complete();
						}
					}
				}
				else if (form.Action != "about:blank")
				{
					Document = new Document(Window);

					var request = ResourceProvider.CreateRequest(form.Action);
					var httpRequest = request as HttpRequest;
					if (httpRequest != null)
					{
						//todo: use right encoding and enctype
						httpRequest.Data = Encoding.UTF8.GetBytes(data);
					}

					var response = await ResourceProvider.GetResourceAsync(request);

					LoadFromResponse(response);
				}
				//todo: handle 'about:blank'
			}
		}

		public Engine() : this(new ResourceProvider()) { }

		public Uri Uri { get; private set; }

		public async void OpenUrl(string path)
		{
			ScriptExecutor.Clear();
			Uri = new Uri(path);
			ResourceProvider.Root = Uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
			var resource = await ResourceProvider.GetResourceAsync(path);
			LoadFromResponse(resource);
		}

		private void LoadFromResponse(IResource resource)
		{
			if (resource.Type == ResourceTypes.Html)
			{
				var httpResponse = resource as HttpResponse;
				if (httpResponse != null && httpResponse.Uri != null)
					Uri = httpResponse.Uri;

				Load(resource.Stream);
			}
		}

		public void Load(Stream stream)
		{
			//todo: fix protocol
			if(Uri == null)
				Uri = new Uri("http://localhost");

			//todo: clear js runtime context
			
			DocumentBuilder.Build(Document, stream);
			Document.Complete();
		}

		public event Action DocumentChanged;
		public void Dispose()
		{
		}
    }
}
