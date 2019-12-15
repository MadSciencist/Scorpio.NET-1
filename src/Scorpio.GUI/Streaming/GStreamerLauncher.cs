﻿using Microsoft.Extensions.Logging;
using Scorpio.GUI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Scorpio.GUI.Streaming
{
    public class GStreamerLauncher
    {
        private readonly ILogger<GStreamerLauncher> _logger;

        /// string -> arg
        /// process -> process instance
        private readonly Dictionary<string, Process> _processes;

        /// <summary>
        /// Handles GStreamer processes
        /// </summary>
        /// <param name="logger"></param>
        public GStreamerLauncher(ILogger<GStreamerLauncher> logger)
        {
            _logger = logger;
            _processes = new Dictionary<string, Process>();
        }

        /// <summary>
        /// Starts new GStreamer process
        /// </summary>
        /// <param name="arg">GStreamer arguments</param>
        public void Launch(string arg)
        {
            arg.GuardNotNull(nameof(arg));

            if (_processes.ContainsKey(arg))
            {
                _logger.LogWarning("Cannot start the same process twice.");
                return;
            }

            Task.Factory.StartNew(() => DoStart(_processes, arg));
        }

        /// <summary>
        /// Stops GStreamer process which was started with given args
        /// </summary>
        /// <param name="arg">Args the process was started with</param>
        public void Stop(string arg)
        {
            try
            {
                var process = _processes[arg];
                process.GuardNotNull(nameof(arg));
                process.Kill();
                _processes.Remove(arg);
                _logger.LogInformation("GStreamer process was successfully stopped.", arg);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError("Given process was not launched using this service", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("Cannot stop process, because it does not exists", ex);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError("Process argument must nut be null", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot stop process, unknown error", ex);
            }
        }

        private void DoStart(Dictionary<string, Process> processes, string arg)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "gst-launch-1.0.exe", 
                    Arguments = arg, 
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            };

            process.Start();
            processes.Add(arg, process);

            _logger.LogInformation("GStreamer process was successfully started.", arg);
        }
    }
}
