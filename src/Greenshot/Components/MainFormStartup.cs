﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Dapplo.CaliburnMicro;
using Dapplo.Log;
using Greenshot.Addons.Components;
using Greenshot.Addons.Core;
using Greenshot.Forms;

#endregion

namespace Greenshot.Components
{
    /// <summary>
    /// This startup action starts the MainForm
    /// </summary>
    [UiStartupAction(StartupOrder = (int)GreenshotUiStartupOrder.TrayIcon), UiShutdownAction]
    public class MainFormStartup : IUiStartupAction, IUiShutdownAction
    {
        private static readonly LogSource Log = new LogSource();
        private readonly ICoreConfiguration _coreConfiguration;
        private readonly MainForm _mainForm;
        private readonly WindowHandle _windowHandle;

        [ImportingConstructor]
        public MainFormStartup(ICoreConfiguration coreConfiguration, MainForm mainForm, WindowHandle windowHandle)
        {
            _coreConfiguration = coreConfiguration;
            _mainForm = mainForm;
            _windowHandle = windowHandle;
        }

        public void Start()
        {
            Log.Debug().WriteLine("Starting MainForm");

            // if language is not set, show language dialog
            if (string.IsNullOrEmpty(_coreConfiguration.Language))
            {
                var languageDialog = LanguageDialog.GetInstance();
                languageDialog.ShowDialog();
                _coreConfiguration.Language = languageDialog.SelectedLanguage;
            }

            // This makes sure the MainForm can initialize, calling show first would create the "Handle" and causing e.g. the DPI Handler to be to late.
            _mainForm.Initialize();
            _mainForm.Show();
            _windowHandle.Handle = _mainForm.Handle;
            Log.Debug().WriteLine("Started MainForm");
        }

        public void Shutdown()
        {
            Log.Debug().WriteLine("Stopping MainForm");

            // Close all open forms, use a separate List to make sure we don't get a "InvalidOperationException: Collection was modified"
            foreach (var form in Application.OpenForms.Cast<Form>().ToList())
            {
                try
                {
                    Log.Info().WriteLine("Closing form: {0}", form.Name);
                    form.Close();
                    form.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error().WriteLine(e, "Error closing form!");
                }
            }
        }
    }
}
