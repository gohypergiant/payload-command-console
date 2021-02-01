using System;

namespace Hypergiant.HIVE
{
    public delegate void TelemetryDataHandler(TelemetryDataItem source, TelemetryDataValue data);
    public delegate void CommandDataHandler(object source, ICommand command);
}