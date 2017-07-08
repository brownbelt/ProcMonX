using Microsoft.Diagnostics.Tracing;
using ProcMonX.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;

namespace ProcMonX.ViewModels {
	class EventsTabViewModel : TabViewModelBase {
		readonly CollectionViewSource _cvs;

		public ICollectionView Items => _cvs.View;

		public EventsTabViewModel(IEnumerable<TraceEventDataViewModel> events, Func<TraceEventDataViewModel, bool> filter = null) {
			_cvs = new CollectionViewSource() {
				Source = events
			};
			_cvs.View.Filter = filter == null ? default(Predicate<object>) : o => filter((TraceEventDataViewModel)o);
		}

		public IEnumerable<TraceEventDataViewModel> Events { get; }

		public bool IsGeneric => !(IsProcessesOnly || IsThreadsOnly || IsModulesOnly || IsRegistryOnly || IsImagesOnly);

		public bool IsProcessesOnly { get; set; }
		public bool IsThreadsOnly { get; set; }
		public bool IsModulesOnly { get; set; }
		public bool IsRegistryOnly { get; set; }
		public bool IsImagesOnly { get; set; }
	}

}
