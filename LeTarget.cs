// 
// Copyright (c) 2010-2012 Logentries, Jlizard
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Logentries nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Mark Lacomber <marklacomber@gmail.com>
// Viliam Holub <vilda@logentries.com>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;

using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Internal.NetworkSenders;
using NLog.Layouts;
using NLog.Targets;

namespace Le
{
    /// <summary>
    /// Logentries agent for remote logging
    /// </summary>
    [Target("Logentries")]
    public sealed class LeTarget : TargetWithLayout
    {
        private SslStream sslSock = null;
        private TcpClient leSocket = null;
        private System.Text.UTF8Encoding encoding;

        public LeTarget()
        {

        }

        [RequiredParameter]
        public string Key { get; set; }

        [RequiredParameter]
        public string Location { get; set; }
        
        [RequiredParameter]
        public bool Debug { get; set; }

        public bool KeepConnection { get; set; }

        /// <summary>
        /// Opens a secure connection with the Logentries server.
        /// </summary>
        /// <param name="key">Account key</param>
        /// <param name="location">Location of the destination log</param>
        private void createSocket(String key, String location)
        {
            this.leSocket = new TcpClient("api.logentries.com", 443);
            this.leSocket.NoDelay = true;
            this.sslSock = new SslStream(this.leSocket.GetStream());
            this.encoding = new System.Text.UTF8Encoding();

            this.sslSock.AuthenticateAsClient("logentries.com");

            String output = "PUT /" + key + "/hosts/" + location + "/?realtime=1 HTTP/1.1\r\n\r\n";
            this.sslSock.Write(this.encoding.GetBytes(output), 0, output.Length);
        }

        /// <summary>
        /// Converts the log entry given into byte array.
        /// </summary>
        /// <param name="logEvent">Log event to convert</param>
        /// <returns>Log entry in byte array</returns>
        private byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text = this.Layout.Render(logEvent) + "\r\n";

            return this.encoding.GetBytes(text);
        }

        /// <summary>
        /// Sends the events given to Logentries.
        /// <summary>
        /// <param name="logEvent">Log event to send</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (this.sslSock == null)
            {
                try
                {
                    this.createSocket(this.Key, this.Location);
                }
                catch (Exception e)
                {
                     WriteDebugMessages("Error connecting to Logentries", e);
                }
            }

            byte[] message = this.GetBytesToWrite(logEvent);

            try
            {
                this.sendToLogentries(message);
            }
            catch (Exception)
            {
                try
                {
                    this.createSocket(this.Key, this.Location);
                    this.sendToLogentries(message);
                }
                catch (Exception ex)
                {
                     WriteDebugMessages("Error sending log to Logentries", ex);
                }
            }
        }

        /// <summary>
        /// Sends the message in bytes to Logentries server.
        /// </summary>
        /// <param name="message">Message to send in bytes</param>
        private void sendToLogentries(byte[] message)
        {
            this.sslSock.Write(message, 0, message.Length);
        }
        
        /// <summary>
        /// Writes debug message on console. This method is called to display
        /// error messages.
        /// </summary>
	/// <param name="message">Message to write</param>
	/// <param name="e">Exception to write</param>
        private void WriteDebugMessages(string message, Exception e)
        {
            if (!this.Debug) return;
            string[] messages = {message, e.ToString()};
            foreach (var msg in messages)
            {
                System.Diagnostics.Debug.WriteLine(msg);
                Console.Error.WriteLine(msg);
            }
        }
    }
}
