﻿using System.IO;
using System.Text;
using Knyaz.Optimus.TestingTools;
using Knyaz.Optimus.Tests.TestingTools;
using Knyaz.Optimus.Tests.Tools;
using NBench;

namespace Knyaz.Optimus.Tests.Performance
{
	public class ComputedStylePerformanceTest
	{
		private Counter _counter;
		
		[PerfSetup]
		public void SetUp(BenchmarkContext ctx) => _counter = ctx.GetCounter("Counter");

		
		private Engine Load(string html)
		{
			var engine = TestingEngine.BuildJintCss();
			engine.Load(new MemoryStream(Encoding.UTF8.GetBytes(html)));
			return engine;
		}
		
		[PerfBenchmark(NumberOfIterations = 3, RunTimeMilliseconds = 1000, RunMode = RunMode.Throughput)]
		[CounterMeasurement("Counter")]
		[MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
		public void GetComputedStyleTest()
		{
			var styles = @"strong {font-family:""Arial""} .a string {font-family:""Curier New""}";
			var html = "<div class a><span><strong id=test></strong><span></div>";
			
			var engine = Load("<head><style>" + styles + "</style></head><body>" + html + "</body>");
			var doc = engine.Document;
			var elt = doc.GetElementById("test");
			elt.GetComputedStyle().GetPropertyValue("font-family");
			_counter.Increment();
		}
	}
}
