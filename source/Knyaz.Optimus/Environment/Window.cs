﻿using System;
using Jint.Runtime;
using Knyaz.Optimus.Dom;
using Knyaz.Optimus.Dom.Css;
using Knyaz.Optimus.Dom.Elements;
using Knyaz.Optimus.Dom.Events;
using Knyaz.Optimus.Dom.Interfaces;
using Knyaz.Optimus.ResourceProviders;
using Knyaz.Optimus.ScriptExecuting;
using Knyaz.Optimus.Tools;

namespace Knyaz.Optimus.Environment
{
	/// <summary>
	/// http://www.w3.org/TR/html5/browsers.html#window
	/// </summary>
	public class Window : IWindowEx
	{
		private Engine _engine;
		private readonly Action<string, string, string> _openWindow;
		private readonly EventTarget _eventTarget;

		public WindowTimers Timers => _timers;
		
		public IConsole Console { get; }
		public Document Document => _engine.Document;
		public Storage LocalStorage { get; } = new Storage();
		public Storage SessionStorage { get; }= new Storage();

		internal Window(
			Func<object> getSyncObj, 
			Action<string, string, string> openWindow,
			INavigator navigator,
			IConsole console,
			Func<Engine> getEngine)
		{
			_getEngine = getEngine;
			Console = console ?? throw new ArgumentNullException(nameof(console));
			_openWindow = openWindow ?? ((x,y,z) => {});
			Screen = new Screen
				{
					Width = 1024,
					Height = 768,
					AvailWidth = 1024,
					AvailHeight = 768,
					ColorDepth = 24,
					PixelDepth = 24
				};

			InnerWidth = 1024;
			InnerHeight = 768;
			Navigator = navigator;
			
			History = new History(OnSetState);
			Location = new Location(History, () => _engine.Uri, s => _engine.OpenUrl(s));

			_timers = new WindowTimers(getSyncObj);
			_timers.OnException += exception =>
				{
					if (exception is JavaScriptException jsEx)
					{
						Console.Log($"JavaScript error in timer handler function (Line:{jsEx.LineNumber}, Col:{jsEx.Column}, Source: {jsEx.Source}): {jsEx.Error}");
					}
					else
					{
						Console.Log("Unhandled exception in timer handler function: " + exception.ToString());
					}
				};

			_eventTarget = new EventTarget(this, () => null, () => _engine.Document);
		}
		
		internal Engine Engine
		{
			get => _engine;
			set => _engine = value;
		}

		private void OnSetState(string url, string title)
		{
			_engine.Uri = UriHelper.IsAbsolute(url) ? new Uri(url) : new Uri(new Uri(_engine.Uri.GetLeftPart(UriPartial.Authority)), url);
			if (title != null)
				_engine.Document.Title = title;
		}

		/// <summary>
		/// Width (in pixels) of the browser window viewport including, if rendered, the vertical scrollbar.
		/// </summary>
		public int InnerWidth { get; set; }

		/// <summary>
		/// Height (in pixels) of the browser window viewport including, if rendered, the horizontal scrollbar.
		/// </summary>
		public int InnerHeight { get; set; }
		
		/// <summary>
		/// Returns a reference to the <see cref="Screen"/> object associated with the window
		/// </summary>
		public IScreen Screen { get; private set; }
		
		/// <summary>
		/// Represents the location (URL) of the object it is linked to. Changes done on it are reflected on the object it relates to. 
		/// </summary>
		public Location Location { get; private set; }
		
		/// <summary>
		/// Gets a reference to the Navigator object, which can be queried for information about the application running the script.
		/// </summary>
		public INavigator Navigator { get; private set; }
		
		/// <summary>
		/// Gets a reference to the History object, which provides an interface for manipulating the browser session history (pages visited in the tab or frame that the current page is loaded in).
		/// </summary>
		public IHistory History { get; private set; }

		private readonly WindowTimers _timers;

		/// <summary>
		/// Sets a timer which executes a function or specified piece of code once after the timer expires.
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="delay">The time, in milliseconds, the timer should wait before the specified function or code is executed. If this parameter is omitted, a value of 0 is used, meaning execute "immediately", or more accurately, as soon as possible. </param>
		/// <param name="data">Variables to be passed to the handler.</param>
		/// <returns></returns>
		public int SetTimeout([JsExpandArray]Action<object[]> handler, double? delay = null, params object[] data) =>
			_timers.SetTimeout(handler, (int)(delay ?? 1), data);

		/// <summary>
		/// Cancels a timeout previously established by calling <see cref="SetTimeout"/>.
		/// </summary>
		/// <param name="handle"></param>
		public void ClearTimeout(int handle) => _timers.ClearTimeout(handle);

		/// <summary>
		/// Repeatedly calls a function or executes a code snippet, with a fixed time delay between each call.
		/// </summary>
		/// <returns>It returns an interval ID which uniquely identifies the interval, so you can remove it later by calling <see cref="ClearInterval"/></returns>
		public int SetInterval([JsExpandArray]Action<object[]> handler, double? delay = null, params object[] data) =>
			_timers.SetInterval(handler, (int)(delay ?? 1), data);

		/// <summary>
		/// Cancels a timed, repeating action which was previously established by a call to <see cref="Window.SetInterval"/>.
		/// </summary>
		/// <param name="handle">The interval ID to be cancelled.</param>
		public void ClearInterval(int handle) => _timers.ClearTimeout(handle);

		public void AddEventListener(string type, Action<Event> listener, EventListenerOptions options) =>
			_eventTarget.AddEventListener(type, listener, options);
		
		/// <summary>
		/// Adds the specified EventListener-compatible object to the list of event listeners for the specified event type on the EventTarget on which it is called. 
		/// </summary>
		/// <param name="type">A string representing the event type to listen for.</param>
		/// <param name="listener">Event handler.</param>
		/// <param name="useCapture">Indicates whether events of this type will be dispatched to the registered listener before being dispatched to any EventTarget beneath it in the DOM tree.</param>
		public void AddEventListener(string type, Action<Event> listener, bool useCapture) =>
			_eventTarget.AddEventListener(type, listener, useCapture);

		/// <summary>
		/// Removes from the EventTarget an event listener previously registered with <see cref="AddEventListener"/>
		/// </summary>
		public void RemoveEventListener(string type, Action<Event> listener, bool useCapture) =>
			_eventTarget.RemoveEventListener(type, listener, useCapture);
		
		/// <summary>
		/// Removes previously registered event handler.
		/// </summary>
		/// <param name="type">The type name of event.</param>
		/// <param name="listener">The handler to be removed.</param>
		/// <param name="options">The options with which the listener was added.</param>
		public void RemoveEventListener(string type, Action<Event> listener, EventListenerOptions options) =>
			_eventTarget.RemoveEventListener(type, listener, options);

		/// <summary>
		/// Dispatches an Event at the specified EventTarget, invoking the affected EventListeners in the appropriate order.
		/// </summary>
		/// <param name="evt"></param>
		/// <returns></returns>
		public bool DispatchEvent(Event evt) => _eventTarget.DispatchEvent(evt);

		/// <summary>
		/// Displays an alert dialog with the optional specified content and an OK button.
		/// </summary>
		/// <param name="message"></param>
		public void Alert(string message) => OnAlert?.Invoke(message);

		/// <summary>
		/// Callback to attach the handler of 'alert' method calls.
		/// </summary>
		public event Action<string> OnAlert;

		/// <summary>
		/// Gives the values of all the CSS properties of an element after applying the active stylesheets and resolving any basic computation those values may contain.
		/// </summary>
		public ICssStyleDeclaration GetComputedStyle(IElement element) => GetComputedStyle(element, null);

		/// <summary>
		/// Gives the values of all the CSS properties of an element after applying the active stylesheets and resolving any basic computation those values may contain. 
		/// </summary>
		/// <param name="element">The <see cref="Element"/> for which to get the computed style.</param>
		/// <param name="pseudoElt">A string specifying the pseudo-element to match. Must be omitted (or null) for regular elements.</param>
		public ICssStyleDeclaration GetComputedStyle(IElement element, string pseudoElt)
		{
			var styling = _engine.Styling;
			if (styling != null)
				return styling.GetComputedStyle(element);

			if (element is HtmlElement htmlElement)
				return htmlElement.Style;
			
			return new CssStyleDeclaration();
		}

		/// <summary>
		/// Returns a new MediaQueryList object representing the parsed results of the specified media query string.
		/// </summary>
		public MediaQueryList MatchMedia(string media)
			=> new MediaQueryList(media, () => _engine.CurrentMedia);

		/// <summary>
		/// Disposes the window object.
		/// </summary>
		public void Dispose() => _timers.Dispose();

		/// <summary>
		/// Loads the specified resource into the browsing context (window, &lt;iframe&gt; or tab) with the specified name.
		/// </summary>
		/// <param name="url">The URL of the resource to be loaded.</param>
		/// <param name="windowName">The name of the browsing context (window, &lt;iframe&gt; or tab) into which to load the specified resource.</param>
		/// <param name="features">The comma-separated list of window features given with their corresponding values in the form "name=value"</param>
		public void Open(string url = null, string windowName = null, string features = null) => 
			_openWindow(url, windowName, features);

		private Func<Engine> _getEngine;
		
		/// <summary> Creates new <see cref="XmlHttpRequest"/> </summary>
		[JsCtor("XMLHttpRequest")]
		public XmlHttpRequest NewXmlHttpRequest()
		{
			var engine = _getEngine();
			return new XmlHttpRequest(engine.ResourceProvider, () => Document, Document, engine.CreateRequest);
		}

		[JsCtor("Image")]
		public HtmlImageElement NewImage(int? width = 0, int? height = 0)
		{
			var img = (HtmlImageElement)Document.CreateElement("img");
			img.Width = width ?? 0;
			img.Height = height ?? 0;
			return img;
		}

		[JsCtor("Event")]
		public Event NewEvent(string eventType = null, EventInitOptions options = null)
		{
			var evt = Document.CreateEvent("Event");
			evt.InitEvent(eventType, options != null && options.Bubbles, options != null && options.Cancelable);
			return evt;
		}
	}
}
