﻿using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LipUI.Models.Lip;

internal class LipConsole
{
    public LipConsole(string executablePath, string workingDir)
    {
        ExecutablePath = executablePath;
        WorkingPath = workingDir;
    }
    public string ExecutablePath { get; }
    public string WorkingPath { get; }

    public event Action<string>? Error;
    public event Action<string>? Output;

    public async ValueTask<int> Run(string command, Action<LipConsoleInstance>? getInstance = null, CancellationToken cancellationToken = default)
    {
        var ins = new LipConsoleInstance(
            ExecutablePath,
            WorkingPath,
            command,
            cancellationToken,
            output =>
            {
                Output?.Invoke(output);
            },
            error =>
            {
                Error?.Invoke(error);
            },
            out Process? process);
        getInstance?.Invoke(ins);

        while (ins.HasExited is false) await Task.Delay(100, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return ins.ExitCode;
    }

    public async ValueTask<int> Run(LipCommandContext command, Action<LipConsoleInstance>? getInstance = null, CancellationToken cancellationToken = default)
        => await Run(command.ToString(), getInstance, cancellationToken);

    public async ValueTask<string> RunAndGetString(string command, Action<LipConsoleInstance>? getInstance = null, CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();
        var ins = new LipConsoleInstance(
            ExecutablePath,
            WorkingPath,
            command,
            cancellationToken,
            output =>
            {
                Output?.Invoke(output);
                builder.AppendLine(output);
            },
            error =>
            {
                Error?.Invoke(error);
                builder.AppendLine(error);
            },
            out Process? process);
        getInstance?.Invoke(ins);

        while (ins.HasExited is false)
        {
            try { await Task.Delay(100, cancellationToken); } catch { }
            ins.KillIfCanceled();
        }
        cancellationToken.ThrowIfCancellationRequested();

        return builder.ToString();
    }

    public async ValueTask<string> RunAndGetString(LipCommandContext command, Action<LipConsoleInstance>? getInstance = null, CancellationToken cancellationToken = default)
        => await RunAndGetString(command.ToString(), getInstance, cancellationToken);
}