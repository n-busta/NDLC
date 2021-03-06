﻿using NBitcoin.Logging;
using NDLC.CLI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace NDLC.Tests
{
	public class CommandTester
	{
		public CommandTester(ITestOutputHelper log)
		{
			Log = log;
			SpyConsole = new SpyConsole(log);
		}
		SpyConsole SpyConsole;
		public ITestOutputHelper Log { get; }

		public async Task AssertInvokeSuccess(string[] args)
		{
			await AssertInvoke(args, 0);
		}
		public async Task AssertInvoke(string[] args, int expectedResult)
		{
			Log.WriteLine("------------------------------------");
			SpyConsole.Clear();
			var command = Program.CreateCommand();
			Log.WriteLine("Command: " + string.Join(' ', args));
			await command.AssertInvoke(args, SpyConsole, expectedResult);
			var result = command.Parse(args);
			LastCommand = (CommandBase)((Command)result.CommandResult.Command).Handler;
			Log.WriteLine("------------------------------------");
		}
		public CommandBase LastCommand { get; set; }
		public string DataDirectoryParameter { get; internal set; }

		public T GetResult<T>()
		{
			if (LastCommand is null)
				throw new InvalidOperationException("No command ran");
			return JsonConvert.DeserializeObject<T>(GetLastOutput(), LastCommand.JsonSerializerSettings);
		}

		public string GetLastOutput()
		{
			return SpyConsole.GetOutput();
		}
	}
}
