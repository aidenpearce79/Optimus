﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Knyaz.Optimus.ResourceProviders
{
	class HttpResourceProvider : IHttpResourceProvider
	{
		private readonly CookieContainer _cookies;

		public HttpResourceProvider(CookieContainer cookies)
		{
			_cookies = cookies;
		}

		public string Root { get; set; }
		public IRequest CreateRequest(string url)
		{
			return new HttpRequest("GET", url);
		}

		private async Task<IResource> SendRequestEx(IRequest request)
		{
			var httpRequest = request as HttpRequest;

			var req = MakeWebRequest(httpRequest);
			
			using (var client = new HttpClient(new HttpClientHandler { CookieContainer = _cookies })
			{
				Timeout = httpRequest != null && httpRequest.Timeout > 0 ? TimeSpan.FromMilliseconds(httpRequest.Timeout) : Timeout.InfiniteTimeSpan
			})
			using (var response = await client.SendAsync(req))
			using (var content = response.Content)
			{
				var result = await content.ReadAsByteArrayAsync();
				return new HttpResponse(
					response.StatusCode,
					new MemoryStream(result),
					content.Headers.ToString(),
					content.Headers.ContentType.MediaType,
					content.Headers.ContentLocation);
			}
		}

		public Task<IResource> SendRequestAsync(IRequest request)
		{
			return SendRequestEx(request);
		}

		private HttpRequestMessage MakeWebRequest(HttpRequest request)
		{
			var u = MakeUri(request.Url);
			var result = new HttpRequestMessage(new HttpMethod(request.Method.ToUpperInvariant()), u);

			if (request.Data != null && result.Method.Method != "GET")
			{
				result.Content = new StreamContent(new MemoryStream(request.Data));
			}
			
			if(request.Headers != null)
			foreach (var keyValue in request.Headers)
			{
				switch (keyValue.Key)
				{
					case "Content-Type":
						result.Content.Headers.ContentType = new MediaTypeHeaderValue(keyValue.Value);
						break;
					default:
						result.Content.Headers.Add(keyValue.Key, keyValue.Value);
						break;
				}
			}

			return result;
		}

		private Uri MakeUri(string uri)
		{
			if (uri.Substring(0, 2) == "./")
				uri = uri.Remove(0, 2);

			var u = Uri.IsWellFormedUriString(uri, UriKind.Absolute) ? new Uri(uri) : new Uri(new Uri(Root), uri);
			return u;
		}
	}

	public interface IHttpResourceProvider : ISpecResourceProvider
	{
		string Root { get; set; }
	}

	public class HttpRequest : IRequest
	{
		public string Method;
		public string Url { get; set; }
		public Dictionary<string, string> Headers;
		public int Timeout { get; set; }

		public byte[] Data;

		public HttpRequest(string method, string url)
		{
			Headers = new Dictionary<string, string>();
			Method = method;
			Url = url;
		}
	}

	public interface IRequest
	{
		string Url { get; }
	}

	public class HttpResponse : IResource
	{
		public HttpResponse(HttpStatusCode statusCode, Stream stream, string headers, string contentType, Uri uri)
		{
			StatusCode = statusCode;
			Stream = stream;
			Headers = headers;
			Type = contentType;
			Uri = uri;
		}

		public HttpResponse(HttpStatusCode statusCode, Stream stream, string headers)
			:this(statusCode, stream, headers, ResourceTypes.Html, null) { }

		public HttpStatusCode StatusCode { get; private set; }
		public string Headers { get; private set; }
		public string Type { get; private set; }
		public Stream Stream { get; private set; }
		public Uri Uri { get; private set; }
	}
}