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

		public event Action<TraceEvent, EventType> EventTrace;

		public TraceManager() {
		}

		public void Dispose() {
			_session.Dispose();
		}

		public void Start(IEnumerable<EventType> types, bool includeInit) {
			if (EventTrace == null)
				throw new InvalidOperationException("Must register for event notifications");

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
						_parser.ProcessStart += obj => EventTrace(obj.Clone(), EventType.ProcessStart);
						break;

					case EventType.ProcessStop:
						_parser.ProcessStop += obj => EventTrace(obj.Clone(), EventType.ProcessStop);
						break;

					case EventType.ThreadStart:
						_parser.ThreadStart += obj => EventTrace(obj.Clone(), EventType.ThreadStart);
						break;

					case EventType.ThreadStop:
						_parser.ThreadStop += obj => EventTrace(obj.Clone(), EventType.ThreadStop);
						break;

					case EventType.RegistryCreateKey:
						_parser.RegistryCreate += obj => EventTrace(obj.Clone(), EventType.RegistryCreateKey);
						break;

					case EventType.RegistryOpenKey:
						_parser.RegistryOpen += obj => EventTrace(obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.RegistryQueryValue:
						_parser.RegistryQueryValue += obj => EventTrace(obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.RegistrySetValue:
						_parser.RegistrySetValue += obj => EventTrace(obj.Clone(), EventType.RegistryOpenKey);
						break;

					case EventType.ImageLoad:
						_parser.ImageLoad += obj => EventTrace(obj.Clone(), EventType.ImageLoad);
						break;

					case EventType.ImageUnload:
						_parser.ImageUnload += obj => EventTrace(obj.Clone(), EventType.ImageUnload);
						break;

				}
			}
		}

		public int LostEvents => _session?.IsActive == true ? _session.EventsLost : 0;

	}
}
