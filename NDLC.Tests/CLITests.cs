﻿using NBitcoin;
using NDLC.Messages;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NDLC.Tests
{
	public class CLITests
	{
		public CLITests(ITestOutputHelper log)
		{
			Log = log;
			Tester = new CommandTester(log);
		}

		public CommandTester Tester { get; }
		public ITestOutputHelper Log { get; }


		[Fact]
		public async Task CanCreateAndReviewOffer()
		{
			await Tester.AssertInvokeSuccess(new string[]
			{
				"--network", "testnet",
				"offer", "create",
				"--oraclepubkey", "57caa081b0a0e9e9413cf4fb72ddc2630d609bdf6a912b98c4cfd358a4ce1496",
				"--nonce", "92ba989222e76cf0cb263fedd67587812110bde1dc1468bef63c8f6974692ea1",
				"--funding", "cHNidP8BALwCAAAAAhyfislzQS3Q4xtaCVbSKztXgCbkXHvqoN0ZQPVwLtiRAQAAAAD+////0sWvPdN8zMIlXWZ5TP2LCmS2tXn0mq5Jy/H+Dr3sVXcAAAAAAP7///8DVwQAAAAAAAAWABRG14/IKu1cwguA1Zpf38w8Uo2WEvFdIAAAAAAAFgAUPDZOW4cQt7HtQ6LTckTs/EdezyIAWmICAAAAABl2qRQCVQOSzWKNBuwbxvjMoah8d4g5wIisI/EbAAABAR/6lFEBAAAAABYAFJwzhGgakahc93MQnjbvxXmq9K/hIgYDsP0UpR8Mgp0gv1V7iuQ64o/esYaR0uVt1w6vso9uL14Yo4MFf1QAAIABAACAAAAAgAEAAAACAAAAAAEBHwAtMQEAAAAAFgAUGArwzyUB0DY+N9TI0lFh2DeURigiBgK4w7vZjBER+7G0ky5uQmb6c20ESovjwPaOS7K/pfhtbxijgwV/VAAAgAEAAIAAAACAAAAAAAYAAAAAIgICzVzQn25qzI10r2VfcIJ6/wedvUpz1QO3LCSN1FIEWKoYo4MFf1QAAIABAACAAAAAgAAAAAAHAAAAACICApmHY8GtEZA8G/dKfXrqg31ArW+8kKK+5PXdMYnbMafKGKODBX9UAACAAQAAgAAAAIABAAAABAAAAAAA",
				"--outcome", "Republican_win:10000sats",
				"--outcome", "Democrats_win:0",
				"--outcome", "other:0.2",
				"--expiration", "1000"
			});
			var offer = Tester.GetResult<Offer>();
			Assert.Equal(Money.Satoshis(10000), offer.ContractInfo[0].Sats);
			Assert.Equal(Money.Coins(0.2m), offer.ContractInfo[2].Sats);
			Assert.Equal("tb1q8smyuku8zzmmrm2r5tfhy38vl3r4anezy6ftse", offer.ChangeAddress.ToString());
			await Tester.AssertInvokeSuccess(new string[]
			{
				"--network", "testnet",
				"offer", "review",
				Tester.GetLastOutput()
			});
		}
	}
}
