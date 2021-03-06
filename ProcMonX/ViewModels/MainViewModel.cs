﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using Prism.Mvvm;
using ProcMonX.Models;
using Prism.Commands;
using Zodiacon.WPF;
using ProcMonX.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using Microsoft.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace ProcMonX.ViewModels {
	class MainViewModel : BindableBase {
		TraceManager _traceManager = new TraceManager();
		ObservableCollection<TabViewModelBase> _tabs = new ObservableCollection<TabViewModelBase>();
		ObservableCollection<TraceEventDataViewModel> _events = new ObservableCollection<TraceEventDataViewModel>();
		ObservableCollection<EventType> _eventTypes = new ObservableCollection<EventType>();

		List<TraceEventDataViewModel> _tempEvents = new List<TraceEventDataViewModel>(128);
		DispatcherTimer _updateTimer;

		public AppOptions Options { get; } = new AppOptions();

		public IList<TabViewModelBase> Tabs => _tabs;

		public IList<EventType> EventTypes => _eventTypes;

		public IList<TraceEventDataViewModel> Events => _events;

		public string Title => "Process Monitor X (C)2017 by Pavel Yosifovich";
		public string Icon => "/icons/app.ico";

		public readonly IUIServices UI;
		public readonly Dispatcher Dispatcher;

		public MainViewModel(IUIServices ui) {
			UI = ui;
			Dispatcher = Dispatcher.CurrentDispatcher;

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			AddEventTypes(
				EventType.ProcessStart, EventType.ProcessStop, 
				EventType.ImageLoad, EventType.ImageUnload, 
				EventType.RegistryQueryValue, EventType.RegistryCreateKey, EventType.RegistrySetValue, EventType.RegistryOpenKey,
				EventType.ThreadStart, EventType.ThreadStop);

			HookupEvents();
			Init();

			_updateTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.ApplicationIdle, (_, __) => Update(), Dispatcher);
			_updateTimer.Start();
		}

		public void AddEventTypes(params EventType[] types) {
			foreach (var type in types)
				_eventTypes.Add(type);
		}

		private void Init() {
			var mainTab = new EventsTabViewModel(_events) {
				Header = "All Events",
				Icon = "/icons/tabs/event.ico",
			};
			AddTab(mainTab, true);
			AddTab(new EventsTabViewModel(_events, evt => evt.Data is ProcessTraceData) {
				Header = "Processes",
				Icon = "/icons/tabs/processes.ico",
				IsProcessesOnly = true
			});

			AddTab(new EventsTabViewModel(_events, evt => evt.Data is ImageLoadTraceData) {
				Header = "Images",
				Icon = "/icons/tabs/image.ico",
				IsImagesOnly = true
			});

			AddTab(new EventsTabViewModel(_events, evt => evt.Data is ThreadTraceData) {
				Header = "Threads",
				Icon = "/icons/tabs/threads.ico",
				IsThreadsOnly = true
			});

			AddTab(new EventsTabViewModel(_events, evt => evt.Data is RegistryTraceData) {
				Header = "Registry",
				Icon = "/icons/tabs/registry.ico",
				IsRegistryOnly = true
			});

			AddTab(new EventsTabViewModel(_events, evt => evt.Type == EventType.AlpcReceiveMessage || evt.Type == EventType.AlpcSendMessage) {
				Header = "ALPC",
				Icon = "/icons/tabs/alpc.ico"
			});
		}

		private void HookupEvents() {
			_traceManager.EventTrace += (evt, type) => {
				lock(_tempEvents)
					_tempEvents.Add(new TraceEventDataViewModel(evt, type, GetMoreInfo(evt)));
			};

		}

		private string GetMoreInfo(TraceEvent evt) {
			switch (evt) {
				case ProcessTraceData p:
					return $"Parent PID: {p.ParentID}; Command Line={p.CommandLine}; Process Flags: {p.Flags}; Bitness: {p.PointerSize * 4}";
			}
			return string.Empty;
		}

		void Update() {
			Debug.WriteLine($"{Environment.TickCount} Updating collection");

			lock (_tempEvents) {
				for(int i = 0; i < _tempEvents.Count; i++)
					_events.Add(_tempEvents[i]);
				_tempEvents.Clear();
			}
			RaisePropertyChanged(nameof(LostEvents));
		}

		public ICommand AlwaysOnTopCommand => new DelegateCommand<DependencyObject>(element =>
			Window.GetWindow(element).Topmost = Options.AlwaysOnTop);

		public ICommand ExitCommand => new DelegateCommand(() => Application.Current.Shutdown());

		private TabViewModelBase _selectedTab;

		public TabViewModelBase SelectedTab {
			get { return _selectedTab; }
			set { SetProperty(ref _selectedTab, value); }
		}

		public void AddTab(TabViewModelBase item, bool activate = false) {
			_tabs.Add(item);
			if (activate)
				SelectedTab = item;
		}

		private bool _isMonitoring;

		public bool IsMonitoring {
			get { return _isMonitoring; }
			set { SetProperty(ref _isMonitoring, value); }
		}

		public DelegateCommandBase GoCommand => new DelegateCommand(
			() => ResumeMonitoring(),
			() => !IsMonitoring)
			.ObservesProperty(() => IsMonitoring);

		public DelegateCommandBase StopCommand => new DelegateCommand(
			() => StopMonitoring(),
			() => IsMonitoring && !IsBusy)
			.ObservesProperty(() => IsMonitoring). ObservesProperty(() => IsBusy);

		private bool _isBusy;

		public bool IsBusy {
			get => _isBusy;
			set => SetProperty(ref _isBusy, value);
		}

		private async void StopMonitoring() {
			IsBusy = true;
			await Task.Run(() => _traceManager.Stop());
			IsBusy = false;
			IsMonitoring = false;
		}

		private void ResumeMonitoring() {
			_traceManager.Start(EventTypes, false);
			IsMonitoring = true;
		}

		public int LostEvents => _traceManager.LostEvents;
	}
}