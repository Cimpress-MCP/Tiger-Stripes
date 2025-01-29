// <copyright file="NearlyOutOfTimeNotifier.cs" company="Cimpress plc">
// Copyright 2024 Cimpress plc
//
// Licensed under the Apache License, Version 2.0 (the "License") –
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Tiger.Stripes;

/// <summary>Notifies when an invocation is nearly out of time.</summary>
[SuppressMessage("Microsoft.Design", "CA1812", Justification = "Instantiated by the host.")]
sealed partial class NearlyOutOfTimeNotifier
{
    readonly NearlyOutOfTimeCounter _counter;
    readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NearlyOutOfTimeNotifier"/> class.
    /// </summary>
    /// <param name="meterFactory">A factory for creating meters.</param>
    /// <param name="loggerFactory">A factory for creating loggers.</param>
    /// <param name="env">The application's host environment.</param>
    public NearlyOutOfTimeNotifier(IMeterFactory meterFactory, ILoggerFactory loggerFactory, IHostEnvironment env)
    {
        using var meter = meterFactory.Create(TelemetrySourceName);
        _counter = Instruments.CreateNearlyOutOfTimeCounter(meter);
        _logger = loggerFactory.CreateApplicationLogger(env);
    }

    /// <summary>Notifies that an invocation is nearly out of time.</summary>
    public void Notify()
    {
        NearlyOutOfTime();
        _counter.Add(1);
    }

    [LoggerMessage(Warning, TigerStripesNearlyOutOfTimeMessage)]
    partial void NearlyOutOfTime();

    static partial class Instruments
    {
        [Counter(Name = TigerStripesNearlyOutOfTime)]
        public static partial NearlyOutOfTimeCounter CreateNearlyOutOfTimeCounter(Meter meter);
    }
}
