using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class PingGroundCommand : HGSGroundCommandBase
    {
        public const int DefaultCount = 1;
        public const int DefaultSize = 20;
        public const int DefaultTimeout = 1000;

        public string Destination { get; }
        public int Count { get; }
        public int Size { get; }
        public int Timeout { get; }

        public PingGroundCommand(Guid commandID, JsonElement e)
            : base(commandID)
        {
            /*
            {
                "command":"ping",
                "address":"192.168.10.70",
                "count":1,
                "timeout":10000,
                "size":40
            }
            */
            if (!e.TryGetProperty("command", out JsonElement commandP))
            {
                throw new ArgumentException("invalid Ping command data");
            }
            Destination = e.GetStringProperty("address", null);
            if (Destination == null)
            {
                throw new ArgumentException("No destination address provided");
            }
            Count = e.GetInt32Property("count", DefaultCount);
            Size = e.GetInt32Property("size", DefaultSize);
            Timeout = e.GetInt32Property("timeout", DefaultTimeout);
        }

        public override Task<IEnumerable<CommandHistory>> Execute(ISatelliteCommsService commsService)
        {
            var state = new List<CommandHistory>();
            state.Add(new CommandHistory(this.CommandID)
            {
                NewState = CommandState.Executing,
                Data = $"Pinging {Destination} with {Size} bytes"
            });


            byte[] buffer = null;

            if (Size != DefaultSize)
            {
                buffer = new byte[Size];
                for (int i = 0; i < Size; i++)
                {
                    switch (i % 4)
                    {
                        case 0:
                            buffer[i] = (byte)'h';
                            break;
                        case 1:
                            buffer[i] = (byte)'i';
                            break;
                        case 2:
                            buffer[i] = (byte)'v';
                            break;
                        default:
                            buffer[i] = (byte)'e';
                            break;
                    }
                }
            }

            try
            {
                using (var ping = new Ping())
                {
                    for (int i = 0; i < Count; i++)
                    {
                        PingReply reply;

                        if (buffer != null)
                        {
                            reply = ping.Send(Destination, Timeout, buffer);
                        }
                        else
                        {
                            reply = ping.Send(Destination, Timeout);
                        }

                        switch (reply.Status)
                        {
                            case IPStatus.Success:
                                state.Add(new CommandHistory(this.CommandID)
                                {
                                    Data = $"Reply took {reply.RoundtripTime}ms"
                                });

                                break;
                            default:
                                state.Add(new CommandHistory(this.CommandID)
                                {
                                    Data = reply.Status.ToString()
                                });
                                break;
                        }
                    }
                }
            }
            catch (PingException ex)
            {
                if (ex.InnerException != null)
                {
                    state.Add(new CommandHistory(this.CommandID)
                    {
                        Data = ex.InnerException.Message
                    });
                }
                else
                {
                    state.Add(new CommandHistory(this.CommandID)
                    {
                        Data = ex.Message
                    });
                }
            }

            state.Add(new CommandHistory(this.CommandID)
            {
                NewState = CommandState.ExecutionComplete
            });

            return Task.FromResult(state as IEnumerable<CommandHistory>);
        }
    }
}
