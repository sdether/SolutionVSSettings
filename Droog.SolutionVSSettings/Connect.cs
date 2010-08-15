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
using Extensibility;
using EnvDTE;
using EnvDTE80;

namespace Droog.SolutionVSSettings {

    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2 {

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
            _debug.OutputString("loaded...\r\n");
            _applicationObject.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            _debug.OutputString("listening for solution load...\r\n");
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
            _debug.OutputString(string.Format("loaded solution '{0}'\r\n", solution.FileName));
            var item = _applicationObject.Solution.FindProjectItem("custom.vssettings");
            if(item == null) {
                _debug.OutputString("did not find 'custom.vssettings'\r\n");
                return;
            }
            var importCommand = string.Format("/import:\"{0}\"", item.get_FileNames(1));
            _applicationObject.ExecuteCommand("Tools.ImportandExportSettings", importCommand);
            _debug.OutputString("loaded custom settings\r\n");
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom) {
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private OutputWindowPane _debug;
    }
}