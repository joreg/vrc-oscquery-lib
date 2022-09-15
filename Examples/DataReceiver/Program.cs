﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using MeaMod.DNS.Model;
using MeaMod.DNS.Multicast;
using Newtonsoft.Json;
using Terminal.Gui;

namespace VRC.OSCQuery.Examples.DataReceiver
{
    class DataReceiver
    {
        
        private static ILog Logger;
        
        static void Main ()
        {
#pragma warning disable CA1416
            Console.SetWindowSize(50,15);
#pragma warning restore CA1416
            Application.Init ();
            
            LogManager.Adapter = new StatusLoggerFactoryAdapter();

            Application.Top.Add (new FindServiceDialog());
            
            Application.Run ();
            Application.Shutdown ();
        }

        public class ListServiceData : Window
        {
            private int _tcpPort;
            private TextView _textView;
            private ServiceProfile? _profile;
            private SRVRecord _srvRecord;
            
            public ListServiceData(ServiceProfile? profile)
            {
                _profile = profile;
                _srvRecord = profile?.Resources.OfType<SRVRecord>().First()!;
                _tcpPort = _srvRecord.Port;

                _textView = new TextView()
                {
                    X = 1,
                    Y = 1,
                    Width = Dim.Fill(2),
                    Height = Dim.Fill(2),
                    ReadOnly = true,
                };

                Title = $"{_profile?.InstanceName} on {_srvRecord.Port}";
                
                Add(_textView);
#pragma warning disable 4014
                RefreshData();
#pragma warning restore 4014
            }

            private async Task RefreshData()
            {
                var response = await new HttpClient().GetAsync($"http://localhost:{_tcpPort}/");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OSCQueryRootNode>(responseString);

                    var sb = new StringBuilder();
                    foreach (var pair in result.Contents)
                    {
                        sb.AppendLine($"{pair.Key}: {pair.Value.Value}");
                    }

                    _textView.Text = sb.ToString();
                }
                
                await Task.Delay(500); // poll every half second
#pragma warning disable 4014
                RefreshData();
#pragma warning restore 4014
            } 
        }
    }
}