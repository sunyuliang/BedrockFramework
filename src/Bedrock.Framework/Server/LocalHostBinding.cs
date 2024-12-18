﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Connections;

namespace Bedrock.Framework;

public class LocalHostBinding(int port, ConnectionDelegate application, IConnectionListenerFactory connectionListenerFactory) : ServerBinding
{
    public override ConnectionDelegate Application => application;

    public override async IAsyncEnumerable<IConnectionListener> BindAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();

        IConnectionListener ipv6Listener = null;
        IConnectionListener ipv4Listener = null;

        try
        {
            ipv6Listener = await connectionListenerFactory.BindAsync(new IPEndPoint(IPAddress.IPv6Loopback, port), cancellationToken);
        }
        catch (Exception ex) when (ex is not IOException)
        {
            exceptions.Add(ex);
        }

        if (ipv6Listener != null)
        {
            yield return ipv6Listener;
        }

        try
        {
            ipv4Listener = await connectionListenerFactory.BindAsync(new IPEndPoint(IPAddress.Loopback, port), cancellationToken);
        }
        catch (Exception ex) when (ex is not IOException)
        {
            exceptions.Add(ex);
        }

        if (exceptions.Count == 2)
        {
            throw new IOException($"Failed to bind to {this}", new AggregateException(exceptions));
        }

        if (ipv4Listener != null)
        {
            yield return ipv4Listener;
        }
    }

    public override string ToString()
    {
        return $"localhost:{port}";
    }
}
