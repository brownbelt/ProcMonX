using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace ProcMonX.Models {

	enum EventType {
		None,
		ProcessStart = 100, ProcessStop, ProcessDCStart, ProcessDCStop,
		ThreadStart = 200, ThreadStop, ThreadDCStart, ThreadDCStop,
		VirtualAlloc = 300, VirtualFree,
		RegistryOpenKey = 400, RegistryQueryValue, RegistrySetValue, RegistryCreateKey, RegistryCloseKey, RegistryEnumerateKey, RegistryFlush,
		AlpcSendMessage = 500, AlpcReceiveMessage,
		ImageLoad = 600, ImageUnload,
	}

	class EventInfo {
		public EventType EventType { get; private set; }
		public string AsString { get; private set; }
		public KernelTraceEventParser.Keywords Keyword { get; private set; }

		public string Category { get; private set; }

		public static readonly IReadOnlyList<EventInfo> AllEvents =
			new List<EventInfo> {
				new EventInfo {
					EventType = EventType.ProcessStart,
					AsString = "Process Start",
					Keyword = KernelTraceEventParser.Keywords.Process,
					Category = "Process"
				},
				new EventInfo {
					EventType = EventType.ProcessDCStart,
					AsString = "Process DC Start",
					Keyword = KernelTraceEventParser.Keywords.Process,
					Category = "Process"
				},
				new EventInfo {
					EventType = EventType.ProcessStop,
					AsString = "Process Stop",
					Keyword = KernelTraceEventParser.Keywords.Process,
					Category = "Process"
				},
				new EventInfo {
					EventType = EventType.ThreadStart,
					AsString = "Thread Start",
					Keyword = KernelTraceEventParser.Keywords.Thread,
					Category = "Thread"
				},
				new EventInfo {
					EventType = EventType.ThreadDCStart,
					AsString = "Thread DC Start",
					Keyword = KernelTraceEventParser.Keywords.Thread,
					Category = "Thread"
				},
				new EventInfo {
					EventType = EventType.ThreadStop,
					AsString = "Thread Stop",
					Keyword = KernelTraceEventParser.Keywords.Thread,
					Category = "Thread"
				},
				new EventInfo {
					EventType = EventType.RegistryOpenKey,
					AsString = "Registry Key Open",
					Keyword = KernelTraceEventParser.Keywords.Registry,
					Category = "Registry"
				},
				new EventInfo {
					EventType = EventType.RegistryCreateKey, AsString = "Registry Key Create", Keyword = KernelTraceEventParser.Keywords.Registry, Category = "Registry"
				},
				new EventInfo {
					EventType = EventType.RegistryQueryValue, AsString = "Registry Query Value", Keyword = KernelTraceEventParser.Keywords.Registry, Category = "Registry"
				},
				new EventInfo {
					EventType = EventType.RegistrySetValue, AsString = "Registry Set Value", Keyword = KernelTraceEventParser.Keywords.Registry, Category = "Registry"
				},
				new EventInfo {
					EventType = EventType.RegistryEnumerateKey, AsString = "Registry Enumerate Key", Keyword = KernelTraceEventParser.Keywords.Registry, Category = "Registry"
				},
				new EventInfo {
					EventType = EventType.ImageLoad, AsString = "Image Loaded", Keyword = KernelTraceEventParser.Keywords.ImageLoad, Category = "Image Load"
				},
				new EventInfo {
					EventType = EventType.ImageUnload, AsString = "Image Unloaded", Keyword = KernelTraceEventParser.Keywords.ImageLoad, Category = "Image Load"
				}
			};

		public static readonly IDictionary<EventType, EventInfo> AllEventsByType = AllEvents.ToDictionary(evt => evt.EventType);
		public static readonly IEnumerable<IGrouping<string, EventInfo>> AllEventsByCategory = AllEvents.GroupBy(evt => evt.Category);
	}
}
