﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Core.Plugin
{
    internal class ExecutablePlugin : JsonRPCPlugin
    {
        private readonly ProcessStartInfo _startInfo;
        public override string SupportedLanguage { get; set; } = AllowedLanguage.Executable;

        public ExecutablePlugin(string filename)
        {
            _startInfo = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }

        protected override Task<Stream> ExecuteQueryAsync(Query query, CancellationToken token)
        {
            JsonRPCServerRequestModel request = new JsonRPCServerRequestModel
            {
                Method = "query",
                Parameters = new object[] {query.Search},
            };

            _startInfo.Arguments = $"\"{request}\"";

            return ExecuteAsync(_startInfo, token);
        }

        protected override string ExecuteCallback(JsonRPCRequestModel rpcRequest)
        {
            _startInfo.Arguments = $"\"{rpcRequest}\"";
            return Execute(_startInfo);
        }

        protected override string ExecuteContextMenu(Result selectedResult)
        {
            JsonRPCServerRequestModel request = new JsonRPCServerRequestModel
            {
                Method = "contextmenu",
                Parameters = new object[] {selectedResult.ContextData},
            };

            _startInfo.Arguments = $"\"{request}\"";

            return Execute(_startInfo);
        }
    }
}