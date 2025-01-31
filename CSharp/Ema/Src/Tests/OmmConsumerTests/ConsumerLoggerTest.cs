﻿/*|-----------------------------------------------------------------------------
 *|            This source code is provided under the Apache 2.0 license      --
 *|  and is provided AS IS with no warranty or guarantee of fit for purpose.  --
 *|                See the project's LICENSE.md for details.                  --
 *|           Copyright (C) 2023 Refinitiv. All rights reserved.              --
 *|-----------------------------------------------------------------------------
 */

using System;
using System.IO;
using LSEG.Eta.ValueAdd.Reactor;
using Xunit.Abstractions;

namespace LSEG.Ema.Access.Tests.OmmConsumerTests
{
    public class ConsumerLoggerTest
    {
        private static readonly string NL = Environment.NewLine;

        private static readonly string EMA_FILE_PATH = "../../../OmmConsumerTests/EmaConfigLoggerTest.xml";

        private static readonly string VERBOSE_LOG = $"{NL}TRACE|: loggerMsg{NL}    ClientName: Consumer_1_Verbose{NL}    Severity: Trace    Text:    " +
            $"Print out active configuration detail.\n\tConfiguredName: Consumer_1_Verbose\n\tInstanceName: Consumer_1_Verbose\n\tItemCountHint: 100000\n\t" +
            $"ServiceCountHint: 513\n\tMaxDispatchCountApiThread: 100\n\tMaxDispatchCountUserThread: 100\n\tRequestTimeout: 15000\n\tXmlTraceToStdout: False\n\t" +
            $"ObeyOpenWindow: True\n\tPostAckTimeout: 15000\n\tMaxOutstandingPosts: 100000\n\tDispatchMode: API_DISPATCH\n\tReconnectAttemptLimit: -1\n\t" +
            $"ReconnectMinDelay: 5000\n\tReconnectMaxDelay: 5000\n\tMsgKeyInUpdates: True\n\tDirectoryRequestTimeOut: 45000\n\tDictionaryRequestTimeOut: 45000\n\t" +
            $"RestRequestTimeOut: 15000\n\tLoginRequestTimeOut: 3000{NL}loggerMsgEnd";

        private static readonly string VERBOSE_LOG_1 = $"{NL}TRACE|: loggerMsg{NL}    ClientName: Consumer_1_Verbose{NL}    Severity: Trace    Text:" +
            $"    Successfully created Reactor.{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}    ClientName: LoginCallbackClient{NL}    Severity: Trace" +
            $"    Text:    RDMLogin request message was populated with this info: LoginRequest: \n\tstreamId: 1{NL}\tuserName: user{NL}\tstreaming:" +
            $" true{NL}\tnameType: NAME{NL}\tapplicationId: 256\n\tapplicationName: ema\n\tposition:";

        private static readonly string VERBOSE_LOG_2 = $"    ClientName: DirectoryCallbackClient{NL}    Severity: Trace    Text:    RDMDirectoryRequest" +
            $" message was populated with Filter(s){NL}\tRDM_DIRECTORY_SERVICE_INFO_FILTER{NL}\tRDM_DIRECTORY_SERVICE_STATE_FILTER{NL}\tRDM_DIRECTORY_SERVICE_GROUP_FILTER" +
            $"{NL}\tRDM_DIRECTORY_SERVICE_LOAD_FILTER{NL}\tRDM_DIRECTORY_SERVICE_DATA_FILTER{NL}\tRDM_DIRECTORY_SERVICE_LINK_FILTER{NL}\trequesting all services{NL}" +
            $"loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}    ClientName: DictionaryCallbackClient{NL}    Severity: Trace    Text:    Successfully loaded local dictionaries:" +
            $" \n\tRDMFieldDictionary file named ../../../../../../etc/RDMFieldDictionary\n\tEnumTypeDef file named ../../../../../../etc/enumtype.def{NL}loggerMsgEnd{NL}{NL}" +
            $"TRACE|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    Severity: Trace    Text:    Created ChannelCallbackClient{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}" +
            $"    ClientName: ChannelCallbackClient{NL}    Severity: Trace    Text:    Attempt to connect using{NL}\t1] SOCKET{NL}\tChannel name Channel_1{NL}\tInstance name" +
            $" Consumer_1_Verbose{NL}\tReactor";

        private static readonly string VERBOSE_LOG_3 = $"CompressionType None{NL}\tTcpNodelay True{NL}\tEnableSessionMgnt False{NL}\tLocation us-east-1{NL}\t" +
            $"ReconnectAttemptLimit -1{NL}\tReconnectMinDelay 5000 msec{NL}\tReconnectMaxDelay 5000 msec{NL}\tGuaranteedOutputBuffers 5000{NL}\tNumInputBuffers 100{NL}\tSysRecvBufSize" +
            $" 0{NL}\tSysSendBufSize 0{NL}\tConnectionPingTimeout 30 sec{NL}\tInitializationTimeout 60 sec{NL}{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    " +
            $"Severity: Trace    Text:    Received ChannelOpened event on channel Channel_1{NL}\tInstance Name Consumer_1_Verbose{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}    " +
            $"ClientName: ChannelCallbackClient{NL}    Severity: Trace    Text:    Successfully created a Reactor and Channel(s){NL}\tChannel name(s) Channel_1{NL}\tInstance name " +
            $"Consumer_1_Verbose{NL}{NL}loggerMsgEnd{NL}{NL}INFO|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    Severity: Info    Text:    Received ChannelUp event on " +
            $"channel Channel_1{NL}\tInstance Name Consumer_1_Verbose{NL}\tComponent Version etacsharp";

        private static readonly string VERBOSE_LOG_4 = $"{NL}loggerMsgEnd" +
            $"{NL}{NL}TRACE|: loggerMsg{NL}    ClientName: LoginCallbackClient{NL}    Severity: Trace    Text:    RDMLogin stream was open with refresh message{NL}" +
            $"LoginRefresh: \n\tstreamId: 1{NL}\tname: user{NL}\tnameType: NAME{NL}\tState: Open/Ok/None - text: \"Login accepted by host localhost\"{NL}\tisSolicited:" +
            $" True{NL}\tapplicationId: 256\n\tapplicationName: ETA Provider Test\n\tposition:";

        private static readonly string VERBOSE_LOG_5 = $"\n\tsingleOpen: 1\n\tallowSuspectData: 1\n\tSupportBatchRequests: 1\n\tSupportOMMPost: 1\n" +
            $"\tSupportEnhancedSymbolList: 1\n{NL}{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}    ClientName: DirectoryCallbackClient{NL}    Severity: Trace" +
            $"    Text:    RDMDirectory stream state was open with refresh message {NL}\tState: Open/Ok/None - text: \"\"{NL}loggerMsgEnd{NL}{NL}TRACE|: loggerMsg{NL}" +
            $"    ClientName: ChannelCallbackClient{NL}    Severity: Trace    Text:    Received ChannelReady event on channel Channel_1{NL}\tInstance Name" +
            $" Consumer_1_Verbose{NL}loggerMsgEnd{NL}";

        private static readonly string SUCCESS_LOG = $"{NL}INFO|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    Severity: Info    Text:    Received ChannelUp event on" +
            $" channel Channel_1{NL}\tInstance Name Consumer_1_Success{NL}\tComponent Version";

        private static readonly string ERROR_LOG = $"{NL}ERROR|: loggerMsg{NL}    ClientName: Consumer_1_Error{NL}    Severity: Error    Text:    login failed (timed out after" +
            $" waiting 3000 milliseconds) for localhost:5555){NL}loggerMsgEnd{NL}{NL}ERROR|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    Severity: Error    Text:    Received ChannelDown" +
            $" event on channel Channel_1{NL}\tInstance Name Consumer_1_Error{NL}\tChannel is null{NL}\tError Id SUCCESS{NL}\tInternal sysError 0{NL}\tError Location {NL}\tError text {NL}loggerMsgEnd{NL}";

        private static readonly string WARNING_LOG = $"{NL}WARN|: loggerMsg{NL}    ClientName: ChannelCallbackClient{NL}    Severity: Warning    Text:" +
            $"    Received ChannelDownReconnecting event on channel Channel_1{NL}\tInstance Name Consumer_1_Warning{NL}\tChannel is null{NL}\tError Id SUCCESS{NL}" +
            $"\tInternal sysError 0{NL}\tError Location Reactor.Connect{NL}\tError text No such host is known.{NL}";

        ITestOutputHelper output;

        public ConsumerLoggerTest(ITestOutputHelper output)
        {
            this.output = output;

        }

        [Theory]
        [InlineData("Verbose")]
        [InlineData("Success")]
        [InlineData("Warning")]
        [InlineData("Error")]
        [InlineData("NoLogMsg")]
        public void ConsumerFileLoggerTest(string logLevel)
        {
            ProviderSessionOptions providerSessionOpts = new ();
            providerSessionOpts.SendLoginReject = false;
            ProviderTest providerTest = new (providerSessionOpts, output);
            ReactorOptions reactorOptions = new ();
            providerTest.Initialize(reactorOptions);

            int serverPort = logLevel.Equals("Error") ? 5555 : providerTest.ServerPort;

            string hostString = $"localhost:{serverPort}";

            output.WriteLine($"Connect with {hostString}");

            OmmConsumerConfig config = new (EMA_FILE_PATH);
            config.UserName("user");

            OmmException? exception = null;
            OmmConsumer? consumer = null;

            try
            {
                config.ConsumerName($"Consumer_2_{logLevel}");

                // Skip generating an instance ID
                OmmBaseImpl<IOmmConsumerClient>.GENERATE_INSTANCE_ID = false;

                if (logLevel.Equals("Warning"))
                    consumer = new OmmConsumer(config.Host("Invalidhost.abc:14002"));
                else
                    consumer = new OmmConsumer(config.Host(hostString));
            }
            catch (OmmException ommException)
            {
                exception = ommException;
            }

            string? logOutput = null;
            int processId = System.Environment.ProcessId;
            string filePath = $"emaLog_{logLevel}_{processId}.log";

            if (logLevel.Equals("Error") || logLevel.Equals("Warning"))
            {
                Assert.NotNull(exception);

                logOutput = File.ReadAllText(filePath);
            }
            else if (logLevel.Equals("NoLogMsg"))
            {
                Assert.Null(exception);
                Assert.False(File.Exists(filePath));
                logOutput = string.Empty;
            }
            else
            {
                Assert.Null(exception);

                logOutput = File.ReadAllText(filePath);
            }

            Assert.NotNull(logOutput);

            if (logLevel.Equals("Success"))
            {
                Assert.Contains(SUCCESS_LOG.Replace("Consumer_1_Success", "Consumer_2_Success"), logOutput);
            }
            else if (logLevel.Equals("NoLogMsg"))
            {
                Assert.Equal(string.Empty, logOutput);
            }
            else if (logLevel.Equals("Error"))
            {
                Assert.Equal(ERROR_LOG.Replace("Consumer_1", "Consumer_2"), logOutput);
            }
            else if (logLevel.Equals("Verbose"))
            {
                Assert.Contains(VERBOSE_LOG.Replace("Consumer_1", "Consumer_2"), logOutput);
                Assert.Contains(VERBOSE_LOG_1.Replace("Consumer_1", "Consumer_2"), logOutput);
                Assert.Contains(VERBOSE_LOG_2.Replace("Consumer_1", "Consumer_2"), logOutput);
                Assert.Contains(VERBOSE_LOG_3.Replace("Consumer_1", "Consumer_2"), logOutput);
                Assert.Contains(VERBOSE_LOG_4.Replace("Consumer_1", "Consumer_2"), logOutput);
                Assert.Contains(VERBOSE_LOG_5.Replace("Consumer_1", "Consumer_2"), logOutput);
            }
            else if (logLevel.Equals("Warning"))
            {
                Assert.Contains(WARNING_LOG.Replace("Consumer_1", "Consumer_2"), logOutput);
            }

            consumer?.Uninitialize();
            providerTest.UnInitialize();
        }

        [Theory]
        [InlineData("Verbose")]
        [InlineData("Success")]
        [InlineData("Warning")]
        [InlineData("Error")]
        [InlineData("NoLogMsg")]
        public void ConsumerConsoleLoggerTest(string logLevel)
        {
            ProviderSessionOptions providerSessionOpts = new();
            providerSessionOpts.SendLoginReject = false;
            ProviderTest providerTest = new(providerSessionOpts, output);
            ReactorOptions reactorOptions = new();
            providerTest.Initialize(reactorOptions);

            int serverPort = logLevel.Equals("Error") ? 5555 : providerTest.ServerPort;

            string hostString = $"localhost:{serverPort}";

            output.WriteLine($"Connect with {hostString}");

            OmmConsumerConfig config = new(EMA_FILE_PATH);
            config.UserName("user");

            OmmException? exception = null;
            OmmConsumer? consumer = null;

            MemoryStream memoryStream = new (8192);
            StreamWriter streamWriter = new (memoryStream);
            System.Console.SetOut(streamWriter);

            try
            {
                config.ConsumerName($"Consumer_1_{logLevel}");
                
                // Skip generating an instance ID
                OmmBaseImpl<IOmmConsumerClient>.GENERATE_INSTANCE_ID = false;

                if (logLevel.Equals("Warning"))
                    consumer = new OmmConsumer(config.Host("Invalidhost.abc:14002"));
                else
                    consumer = new OmmConsumer(config.Host(hostString));
            }
            catch (OmmException ommException)
            {
                exception = ommException;
            }

            if (logLevel.Equals("Error") || logLevel.Equals("Warning"))
            {
                Assert.NotNull(exception);
            }
            else
            {
                Assert.Null(exception);
            }

            string logOutput = System.Text.Encoding.ASCII.GetString(memoryStream.GetBuffer(), 0, 
                (int)memoryStream.Length);

            Assert.NotNull(logOutput);

            if (logLevel.Equals("Success"))
            {
                Assert.Contains(SUCCESS_LOG, logOutput);
            }
            else if(logLevel.Equals("NoLogMsg"))
            {
                Assert.Equal(string.Empty, logOutput);
            }
            else if (logLevel.Equals("Error"))
            {
                Assert.Equal(ERROR_LOG, logOutput);
            }
            else if (logLevel.Equals("Verbose"))
            {
                Assert.Contains(VERBOSE_LOG, logOutput);
                Assert.Contains(VERBOSE_LOG_1, logOutput);
                Assert.Contains(VERBOSE_LOG_2, logOutput);
                Assert.Contains(VERBOSE_LOG_3, logOutput);
                Assert.Contains(VERBOSE_LOG_4, logOutput);
                Assert.Contains(VERBOSE_LOG_5, logOutput);
            }
            else if (logLevel.Equals("Warning"))
            {
                Assert.Contains(WARNING_LOG, logOutput);
            }

            consumer?.Uninitialize();
            providerTest.UnInitialize();
        }

        [Fact]
        public void ConsumerMultipleIncludedDateFilesLoggerTest()
        {
            ProviderSessionOptions providerSessionOpts = new();
            providerSessionOpts.SendLoginReject = false;
            ProviderTest providerTest = new(providerSessionOpts, output);
            ReactorOptions reactorOptions = new();
            providerTest.Initialize(reactorOptions);

            int serverPort = providerTest.ServerPort;

            string hostString = $"localhost:{serverPort}";

            output.WriteLine($"Connect with {hostString}");

            OmmConsumerConfig config = new(EMA_FILE_PATH);
            OmmException? exception = null;
            OmmConsumer? consumer = null;

            try
            {
                config.ConsumerName("Consumer_2_MultipleFiles");

                // Skip generating an instance ID
                OmmBaseImpl<IOmmConsumerClient>.GENERATE_INSTANCE_ID = false;

                consumer = new OmmConsumer(config.Host(hostString));
            }
            catch (OmmException ommException)
            {
                exception = ommException;
            }

            Assert.Null(exception);

            int processId = System.Environment.ProcessId;
            string filePath = $"emaLog_MultipleFiles_{processId}.log";
            string filePath0 = $"emaLog_MultipleFiles_{processId}.0.log";
            string filePath1 = $"emaLog_MultipleFiles_{processId}.1.log";

            consumer?.Uninitialize();
            providerTest.UnInitialize();

            Assert.True(File.Exists(filePath));
            Assert.True(File.Exists(filePath0));
            Assert.True(File.Exists(filePath1));

            string logOutput = File.ReadAllText(filePath);
            string logOutput0 = File.ReadAllText(filePath0);
            string logOutput1 = File.ReadAllText(filePath1);

            DateTime dateTime = DateTime.Now;

            string expectedDate = $"{dateTime.Year}-";
            expectedDate += (dateTime.Month < 10) ? $"0{dateTime.Month}-" : $"{dateTime.Month}-";
            expectedDate += (dateTime.Day < 10) ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            Assert.Contains(expectedDate, logOutput1);
            Assert.Contains("RDMLogin stream was open with refresh message", logOutput1);
            Assert.Contains("Login accepted by host localhost", logOutput1);
            Assert.Contains(expectedDate, logOutput0);
            Assert.Contains("RDMDirectory stream state was open with refresh message", logOutput0);
            Assert.Contains("Received ChannelReady event on channel Channel_1", logOutput0);
            Assert.Contains(expectedDate, logOutput);
            Assert.Contains("Received Delete action for RDMService", logOutput);

            Assert.True(logOutput.Length > 0);
        }
    }
}
