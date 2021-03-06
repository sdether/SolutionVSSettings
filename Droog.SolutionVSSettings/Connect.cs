/*
 * Droog.SolutionVSSettings 
 * Copyright (C) 2010 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace Droog.SolutionVSSettings {

    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2 {


        private const string SETTINGS_KEY = "solution.vssettings";
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private OutputWindowPane _debug;

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect() {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom) {
    _applicationObject = (DTE2)application;
    _addInInstance = (AddIn)addInInst;
    _debug = _applicationObject.ToolWindows.OutputWindow.OutputWindowPanes.Add("Solution Settings Loader");
    Output("loaded...");
    _applicationObject.Events.SolutionEvents.Opened += SolutionEvents_Opened;
    Output("listening for solution load...");
}

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom) {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom) {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom) {
        }

void SolutionEvents_Opened() {
    var solution = _applicationObject.Solution;
    Output("loaded solution '{0}'", solution.FileName);

    // check for solution directory override 
    var configFile = Path.Combine(Path.GetDirectoryName(solution.FileName), "solutionsettings.config");
    string settingsFile = null;
    if(File.Exists(configFile)) {
        Output("trying to load config from '{0}'", configFile);
        settingsFile = GetSettingsFile(configFile, settingsFile);
        if(!string.IsNullOrEmpty(settingsFile)) {
            Output("unable to find override '{0}'", settingsFile);
        } else {
            Output("using solutionsettings.config override");
        }
    }

    // check for settings in solution
    if(string.IsNullOrEmpty(settingsFile)) {
        var item = _applicationObject.Solution.FindProjectItem(SETTINGS_KEY);
        if(item != null) {
            settingsFile = item.get_FileNames(1);
            Output("using solution file '{0}'", settingsFile);
        }
    }

    // check for environment override
    if(string.IsNullOrEmpty(settingsFile)) {
        configFile = Environment.GetEnvironmentVariable("solutionsettings.config");
        if(!string.IsNullOrEmpty(configFile)) {
            settingsFile = GetSettingsFile(configFile, settingsFile);
            if(string.IsNullOrEmpty(settingsFile)) {
                Output("unable to find environment override '{0}'", settingsFile);
            } else {
                Output("using environment config override");
            }
        }
    }
    if(string.IsNullOrEmpty(settingsFile)) {
        Output("no custom settings for solution.");
        return;
    }
    var importCommand = string.Format("/import:\"{0}\"", settingsFile);
    try {
        _applicationObject.ExecuteCommand("Tools.ImportandExportSettings", importCommand);
        Output("loaded custom settings\r\n");
    } catch(Exception e) {
        Output("unable to load '{0}': {1}", settingsFile, e.Message);
    }
}

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom) {
        }

        private string GetSettingsFile(string configFile, string settingsFile) {
            try {
                var doc = XDocument.Load(configFile);
                settingsFile = (from x in doc.Descendants("settingsfile") select x.Value).FirstOrDefault();
                if(!string.IsNullOrEmpty(settingsFile) && File.Exists(settingsFile)) {
                    return Path.GetFullPath(settingsFile);
                }
            } catch(Exception e) {
                Output("unable to load config from '{0}': {1}", configFile, e.Message);
            }
            return null;
        }

        private void Output(string format, params object[] args) {
            _debug.OutputString(string.Format(format, args));
            _debug.OutputString("\r\n");
        }
    }
}