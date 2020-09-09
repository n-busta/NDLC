﻿using NBitcoin.Secp256k1;
using NDLC.Secp256k1;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NDLC.CLI
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			RootCommand root = CreateCommand();
			await root.InvokeAsync(args);
		}

		public static RootCommand CreateCommand()
		{
			RootCommand root = new RootCommand();
			root.Description = "A simple tool to manage DLCs.\r\nTHIS IS EXPERIMENTAL, USE AT YOUR OWN RISKS!";
			root.Add(new Option<string>("--network", "The network type (mainnet, testnet or regtest)")
			{
				Argument = new Argument<string>(),
				IsRequired = false
			});

			Command offer = new Command("offer");
			root.Add(offer);
			offer.Description = "Manage offers";
			Command createOffer = new Command("create")
			{
				Description = "Create a new offer",
			};
			offer.Add(createOffer);
			createOffer.Description = "Create a new offer";
			createOffer.Add(new Option<string>("--oraclepubkey")
			{
				Argument = new Argument<string>(),
				Description = "The oracle's public key",
				IsRequired = true
			});
			createOffer.Add(new Option<string>("--nonce")
			{
				Argument = new Argument<string>(),
				Description = "The oracle's commitment for this bet",
				IsRequired = true
			});
			createOffer.Add(new Option<string[]>("--outcome")
			{
				Argument = new Argument<string[]>()
				{
					Arity = ArgumentArity.OneOrMore
				},
				Description = "The outcomes of the contract (one or multiple)",
				IsRequired = true
			});
			createOffer.Add(new Option<string>("--maturity")
			{
				Argument = new Argument<string>(),
				Description = "The timelock of the contract execution transactions (default: 0)",
				IsRequired = false
			});
			createOffer.Add(new Option<string>("--expiration")
			{
				Argument = new Argument<string>(),
				Description = "The timelock on the refund transaction",
				IsRequired = true
			});
			createOffer.Handler = new CreateOfferCommand();


			Command reviewOffer = new Command("review", "Review an offer");
			offer.Add(reviewOffer);
			reviewOffer.AddOption(new Option<bool>(new[] { "-h", "--human" }, "Show the offer in a human readable way"));
			reviewOffer.AddArgument(new Argument<string>("offer", "The JSON offer to review") { Arity = ArgumentArity.ExactlyOne });
			reviewOffer.Handler = new ReviewOfferCommand();
			return root;
		}
	}
}
