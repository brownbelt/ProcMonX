using Microsoft.Diagnostics.Tracing;
using ProcMonX.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProcMonX.Models;

namespace ProcMonX.ViewModels {
	class TraceEventDataViewModel {
		static int _globalIndex;
		public int Index { get; }
		public TraceEvent Data { get; }
		public EventType Type { get; }
		public string TypeAsString { get; }

		public readonly EventInfo Info;

		public string Icon { get; }

		public int? ThreadId => Data.ThreadID < 0 ? (int?)null : Data.ThreadID;

		public string MoreInfo { get; }

		public TraceEventDataViewModel(TraceEvent evt, EventType type, string moreInfo) {
			Data = evt;
			Type = type;
			Info = EventInfo.AllEventsByType[type];
			TypeAsString = Info.AsString ?? type.ToString();
			Index = Interlocked.Increment(ref _globalIndex);
			Icon = $"/icons/events/{type.ToString()}.ico";
			MoreInfo = moreInfo;
		}
	}
}
