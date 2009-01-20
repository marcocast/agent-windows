﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace AgentForAgent
{

    public partial class VersionChecker : Form
    {
        // provide the URL to XmlTextReader
        private string xmlURL = "http://exp.activeeon.com/PAAgent-version.xml";
        Thread threadChecker;

        //--Delegate ChangeLabel
        private delegate void updateFromServer();

        //--Info
        string urlToDownload = "";
        string newFeaturesText = "";
        string newVersionText = "";
        bool newVersionDetected = false;

        public VersionChecker()
        {
            InitializeComponent();
            currentVersion.Text = Application.ProductVersion;

            threadChecker = new Thread(new ThreadStart(CheckNewVersion));
            threadChecker.Start();

            //checkNewVersion();
        }

        private void CheckNewVersion()
        {

            // version number from server
            Version newVersion = null;

            // url to download the latest version from server
            try
            {
                XmlTextReader reader = new XmlTextReader(xmlURL);
                // simply (and easily) skip the junk at the beginning  
                reader.MoveToContent();
                // internal - as the XmlTextReader moves only  
                // forward, we save current xml element name  
                // in elementName variable. When we parse a  
                // text node, we refer to elementName to check  
                // what was the node name  
                string elementName = "";
                // we check if the xml starts with a proper  
                // "ourfancyapp" element node  
                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "PAAgent"))
                {
                    while (reader.Read())
                    {
                        // when we find an element node,  
                        // we remember its name  
                        if (reader.NodeType == XmlNodeType.Element)
                            elementName = reader.Name;
                        else
                        {
                            // for text nodes...  
                            if ((reader.NodeType == XmlNodeType.Text) &&
                            (reader.HasValue))
                            {
                                // we check what the name of the node was  
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        newVersionText = newVersion.ToString();
                                        break;

                                    case "features":
                                        newFeaturesText = reader.Value;
                                        break;

                                    case "url":
                                        urlToDownload = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error" + e.StackTrace);
            }



            // get the running version  
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            // compare the versions  
            if (curVersion.CompareTo(newVersion) < 0)
            {
                newVersionDetected = true;
                //resultText = "New version detected.";
            }
            else if (curVersion.CompareTo(newVersion) == 0)
            {
                newVersionDetected = false;
                //resultText = "You already have the latest version"; //
            }

            this.Invoke(new updateFromServer(Update));

        }

        private void Update()
        {
            panelWait.Visible = false;
            panelInfo.Visible = true;
            if (newVersionDetected)
            {
                resultLabel.Text= "New version detected.";
                button1.Visible = true;
                textBoxFeatures.Visible = true;
            }
            else
            {
                resultLabel.Text = "You already have the latest version";
            }

            latestVersion.Text = newVersionText;
            textBoxFeatures.Text = newFeaturesText;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(urlToDownload);
        }

        private void VersionChecker_FormClosed(object sender, FormClosedEventArgs e)
        {
            threadChecker.Abort();
        }
    }
}