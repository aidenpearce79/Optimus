﻿using System;
using System.Threading;
using System.Windows.Forms;

namespace WebBrowser.WfApp.Controls
{
	public partial class AddressBar : UserControl
	{
		public AddressBar()
		{
			InitializeComponent();
		}

		public Engine Engine { get; set; }

		private void textBoxUrl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				new Thread(new ThreadStart(() => Engine.OpenUrl(textBoxUrl.Text))).Start();
				;
			}
		}
	}
}