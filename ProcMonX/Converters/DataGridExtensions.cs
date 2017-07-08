using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProcMonX.Converters {
	static class DataGridExtensions {
		public static IEnumerable<DataGridColumn> GetExtraColumns(DependencyObject obj) {
			return (IEnumerable<DataGridColumn>)obj.GetValue(ExtraColumnsProperty);
		}

		public static void SetExtraColumns(DependencyObject obj, IEnumerable<DataGridColumn> value) {
			obj.SetValue(ExtraColumnsProperty, value);
		}

		public static readonly DependencyProperty ExtraColumnsProperty =
			DependencyProperty.RegisterAttached("ExtraColumns", typeof(IEnumerable<DataGridColumn>), typeof(DataGridExtensions), 
				new PropertyMetadata(null, OnExtraColumnsChanged));

		private static void OnExtraColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is DataGrid dg) {
				dg.Loaded += delegate {
					var columns = e.NewValue as IEnumerable<DataGridColumn>;
					foreach (var column in columns)
						dg.Columns.Add(column);
				};
			}
		}
	}
}
