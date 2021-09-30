using System;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HANReader.Core;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace HANReader
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var app = new CommandLineApplication();

            var serialPortNameArgument = app.Argument("SerialPort", "Specifies the serial port to be used when reading data.", a => a.IsRequired());
            var baudRateOption = app.Option<int>("-br | --baudrate", "Specifies the serial port baud rate. Default value is '2400'", CommandOptionType.SingleValue);
            var parityOption = app.Option<string>("-p | --parity", "Specifies the serial port parity. Default value is 'none'", CommandOptionType.SingleValue, o => o.Accepts(vb => vb.Values("none", "odd", "even", "mark", "space")));
            var databitsOption = app.Option<int>("-db | --databits", "Specifies the serial port databits. Default value is '8'", CommandOptionType.SingleValue);
            var stopbitsOption = app.Option<string>("-sb | --stopbits", "Specifies the serial port stop bits, Default values is 'one'", CommandOptionType.SingleValue, o => o.Accepts(vb => vb.Values("none", "one", "two", "onepointfive")));
            app.HelpOption("-h|--help");

            app.VersionOption("-v | --version", GetCurrentVersion());

            app.OnExecuteAsync(async token =>
            {
                var reader = new StreamReader(Console.Error);
                var serialPortName = serialPortNameArgument.Value;
                var baudRate = baudRateOption.HasValue() ? baudRateOption.ParsedValue : 2400;
                var parity = parityOption.HasValue() ? Enum.Parse<Parity>(parityOption.ParsedValue, ignoreCase: true) : Parity.None;
                var databits = databitsOption.HasValue() ? databitsOption.ParsedValue : 8;
                var stopBits = stopbitsOption.HasValue() ? Enum.Parse<StopBits>(stopbitsOption.ParsedValue) : StopBits.One;
                var serialPort = new SerialPort(serialPortName, baudRate, parity, databits, stopBits);
                serialPort.Open();

                await reader.StartAsync(serialPort.BaseStream, async (frame) =>
                {
                    var json = JsonConvert.SerializeObject(frame);
                    Console.WriteLine(json);
                });
            });

            return await app.ExecuteAsync(args);
        }

        public static string GetCurrentVersion()
        {
            var versionAttribute = typeof(Program).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single();
            return versionAttribute.InformationalVersion;
        }
    }



}
