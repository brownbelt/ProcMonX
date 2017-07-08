using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ProcMonX.Models;

namespace ProcMonX.Tracing {

	sealed class TraceManager : IDisposable {
		TraceEventSession _session;
		KernelTraceEventParser _parser;
		Thread _processingThread;
		bool _includeInit;

		public event Action<ProcessTraceData, EventType> ProcessTrace;
		public event Action<ThreadTraceData, EventType> ThreadTrace;
		public event Action<RegistryTraceData, EventType> RegistryTrace;
		public event Action<ImageLoadTraceData, EventType> ImageLoadTrace;

		public TraceManager() {
		}

		public void Dispose() {
			_session.Dispose();
		}

		public void Start(IEnumerable<EventType> types, bool includeInit) {
			_includeInit = includeInit;
			_session = new TraceEventSession(KernelTraceEventParser.KernelSessionName) {
				BufferSizeMB = 128,
				CpuSampleIntervalMSec = 10
			};

			var keywords = KernelTraceEventParser.Keywords.None;
			foreach (var type in types)
				keywords |= EventInfo.AllEventsByType[type].Keyword;

			_session.EnableKernelProvider(keywords);

			_processingThread = new Thread(() => {
				_parser = new KernelTraceEventParser(_session.Source);
				SetupCallbacks(types);
				_session.Source.Process();
			});
			_processingThread.Priority = ThreadPriority.Lowest;
			_processingThread.IsBackground = true;
			_processingThread.Start();
		}


		public void Stop() {
			_session.Flush();
			_session.Stop();
		}

		private void SetupCallbacks(IEnumerable<EventType> types) {
			foreach(var type in types) {
				switch (type) {
					case EventType.ProcessStart:
						_parser.ProcessStart += obj => ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStart);
						break;

					case EventType.ProcessStop:
						_parser.ProcessStop += obj => ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStop);
						break;

					case EventType.ThreadStart:
						_parser.ThreadStart += obj => ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStart);
						break;

					case EventType.ThreadStop:
						_parser.ThreadStop += obj => ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStop);
						break;

					case EventType.RegistryCreateKey:
						_parser.RegistryCreate += obj => RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryCreateKey);
						break;

					case EventType.RegistryOpenKey:
						_parser.RegistryOpen += obj => RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.RegistryQueryValue:
						_parser.RegistryQueryValue += obj => RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.RegistrySetValue:
						_parser.RegistrySetValue += obj => RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.ImageLoad:
						_parser.ImageLoad += obj => ImageLoadTrace?.Invoke((ImageLoadTraceData)obj.Clone(), EventType.ImageLoad);
						break;

					case EventType.ImageUnload:
						_parser.ImageUnload += obj => ImageLoadTrace?.Invoke((ImageLoadTraceData)obj.Clone(), EventType.ImageUnload);
						break;
				}
			}
		}


		private void OnProcessDCStart(ProcessTraceData obj) {
			var data = (ProcessTraceData)obj.Clone();
			ProcessTrace?.Invoke(data, EventType.ProcessDCStart);
		}

		private void OnThreadStop(ThreadTraceData obj) {
			ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStop);
		}

		private void OnProcessStop(ProcessTraceData obj) {
			ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStop);
		}

		private void OnThreadStart(ThreadTraceData obj) {
			ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStart);
		}

		private void OnProcessStart(ProcessTraceData obj) {
			ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStart);
		}
	}
}
