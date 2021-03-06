﻿using NBitcoin.Secp256k1;
using NDLC.CLI.Events;
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
			root.Add(new Option<string>("--datadir", "The data directory")
			{
				Argument = new Argument<string>(),
				IsRequired = false
			});

			Command info = new Command("info", "Show informations");
			root.Add(info);
			info.Handler = new ShowInfoCommand();

			Command offer = new Command("offer", "Manage offers");
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


			Command oracle = new Command("oracle", "Oracle commands");
			Command oracleAdd = new Command("add", "Add a new oracle")
				{
					new Argument<string>("name", "The oracle name"),
					new Argument<string>("pubkey", "The oracle pubkey"),
				};
			oracleAdd.Handler = new AddSetOracleCommand();
			Command oracleSet = new Command("set", "Modify an oracle")
				{
					new Argument<string>("name", "The oracle name"),
					new Argument<string>("pubkey", "The oracle pubkey"),
				};
			oracleSet.Handler = new AddSetOracleCommand() { Set = true };
			Command oracleRemove = new Command("remove", "Remove an oracle")
				{
					new Argument<string>("name", "The oracle name")
				};
			oracleRemove.Handler = new RemoveOracleCommand();
			Command oracleList = new Command("list", "List oracles");
			oracleList.Handler = new ListOracleCommand();
			Command oracleShow = new Command("show", "Show an oracle")
				{
					new Argument<string>("name", "The oracle name")
				};
			oracleShow.Add(new Option<bool>("--show-sensitive", "Show sensitive informations (like private keys)"));
			oracleShow.Handler = new ShowOracleCommand();
			Command oracleCreate = new Command("generate", "Generate a new oracle (the private key will be stored locally)")
				{
					new Argument<string>("name", "The oracle name")
				};
			oracleCreate.Handler = new GenerateOracleCommand();
			root.Add(oracle);
			oracle.Add(oracleAdd);
			oracle.Add(oracleSet);
			oracle.Add(oracleRemove);
			oracle.Add(oracleList);
			oracle.Add(oracleShow);
			oracle.Add(oracleCreate);

			Command evts = new Command("event", "Manage events");
			Command addEvent = new Command("add", "Add a new event");
			addEvent.Add(new Argument<string>("name", "The event full name, in format 'oraclename/name'")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			addEvent.Add(new Argument<string>("nonce", "The event nonce, as specified by the oracle")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			addEvent.Add(new Argument<string>("outcomes", "The outcomes, as specified by the oracle")
			{
				Arity = ArgumentArity.OneOrMore
			});
			addEvent.Handler = new AddEventCommand();
			Command listEvent = new Command("list", "List events");
			listEvent.Add(new Option<string>("--oracle", "Filter events of this specific oracle")
			{
				Argument = new Argument<string>()
				{
					Arity = ArgumentArity.ExactlyOne
				},
				IsRequired = false
			});
			listEvent.Handler = new ListEventsCommand();
			Command showEvents = new Command("show", "Show details of an event");
			showEvents.Add(new Argument<string>("name", "The full name of the event"));
			showEvents.Handler = new ShowEventCommand();

			Command generateEvents = new Command("generate", "Generate a new event");
			generateEvents.Add(new Argument<string>("name", "The event full name, in format 'oraclename/name'")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			generateEvents.Add(new Argument<string>("outcomes", "The outcomes, as specified by the oracle")
			{
				Arity = ArgumentArity.OneOrMore
			});
			generateEvents.Handler = new GenerateEventCommand();

			Command attestEvent = new Command("attest", "Attest an event");
			Command attestSignEvent = new Command("sign", "Sign an attestation");
			attestSignEvent.Add(new Argument<string>("name", "The event full name, in format 'oraclename/name'")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			attestSignEvent.Add(new Argument<string>("outcome", "The outcome to attest")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			attestSignEvent.Add(new Option<bool>(new[] { "--force", "-f" }, "Force this action")
			{
				Argument = new Argument<bool>(),
				IsRequired = false
			});
			attestSignEvent.Handler = new AttestSignCommand();
			Command attestAddEvent = new Command("add", "Add an attestation received by an oracle");
			attestAddEvent.Add(new Argument<string>("name", "The event full name, in format 'oraclename/name'")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			attestAddEvent.Add(new Argument<string>("attestation", "The received attestation")
			{
				Arity = ArgumentArity.ExactlyOne
			});
			attestAddEvent.Handler = new AttestAddCommand();
			attestEvent.Add(attestAddEvent);
			attestEvent.Add(attestSignEvent);
			root.Add(evts);
			evts.Add(addEvent);
			evts.Add(listEvent);
			evts.Add(showEvents);
			evts.Add(generateEvents);
			evts.Add(attestEvent);

			Command reviewOffer = new Command("review", "Review an offer");
			offer.Add(reviewOffer);
			reviewOffer.AddOption(new Option<bool>(new[] { "-h", "--human" }, "Show the offer in a human readable way"));
			reviewOffer.AddArgument(new Argument<string>("offer", "The JSON offer to review") { Arity = ArgumentArity.ExactlyOne });
			reviewOffer.Handler = new ReviewOfferCommand();
			return root;
		}
	}
}
