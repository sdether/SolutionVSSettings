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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Droog.SolutionVSSettings {

    [RunInstaller(true)]
    public class AddInInstaller : Installer {
        public override void Install(IDictionary stateSaver) {
            try {
                base.Install(stateSaver);
                XDocument doc;
                var installationPath = Context.Parameters["AssemblyPath"];
                var addinResourceFile = Assembly.GetExecutingAssembly().GetName().Name + ".Droog.SolutionVSSettings.AddIn";

                using(var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(addinResourceFile)) {
                    doc = XDocument.Load(XmlReader.Create(resourceStream));
                }
                var query = from assemblyNode in doc.Descendants()
                            where assemblyNode.Name.LocalName.Equals("Assembly")
                            select assemblyNode;

                foreach(var assemblyNode in query) {
                    assemblyNode.SetValue(installationPath);
                }
                var addinTargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Visual Studio 2008\Addins");
                var targetFile = Path.Combine(addinTargetPath, "Droog.SolutionVSSettings.AddIn");
                doc.Save(targetFile);
                stateSaver.Add("AddinPath", targetFile);
            } catch(Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

        public override void Rollback(IDictionary savedState) {
            base.Rollback(savedState);

            try {
                var fileName = (string)savedState["AddinPath"];
                if(File.Exists(fileName)) {
                    File.Delete(fileName);
                }
            } catch(Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

        public override void Uninstall(IDictionary savedState) {
            base.Uninstall(savedState);

            try {
                var fileName = (string)savedState["AddinPath"];
                if(File.Exists(fileName)) {
                    File.Delete(fileName);
                }
            } catch(Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

    }
}
