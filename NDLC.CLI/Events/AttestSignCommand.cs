﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NDLC.Secp256k1;
using NBitcoin.Secp256k1;
using NBitcoin;
using NDLC.Messages;

namespace NDLC.CLI.Events
{
	public class AttestSignCommand : CommandBase
	{
		protected override async Task InvokeAsyncBase(InvocationContext context)
		{
			var outcome = context.ParseResult.CommandResult.GetArgumentValueOrDefault<string>("outcome")?.Trim();
			var force = context.ParseResult.ValueForOption<bool>("force");
			if (outcome is null)
				throw new CommandOptionRequiredException("outcome");
			EventFullName evt = context.GetEventName();
			var oracle = await Repository.GetOracle(evt.OracleName);
			if (oracle is null)
				throw new CommandException("name", "This oracle does not exists");

			var discreteOutcome = new DiscreteOutcome(outcome);
			var evtObj = await Repository.GetEvent(evt);
			if (evtObj?.Nonce is null)
				throw new CommandException("name", "This event does not exists");
			if (evtObj?.NonceKeyPath is null)
				throw new CommandException("name", "You did not generated this event");
			outcome = evtObj.Outcomes.FirstOrDefault(o => o.Equals(outcome, StringComparison.OrdinalIgnoreCase));
			if (outcome is null)
				throw new CommandException("outcome", "This outcome does not exists in this event");
			var key = oracle.RootedKeyPath is RootedKeyPath ? await Repository.GetKey(oracle.RootedKeyPath) : null;
			if (key is null)
				throw new CommandException("name", "You do not own the keys of this oracle");
			if (evtObj.Attestations?.ContainsKey(outcome) is true)
				throw new CommandException("outcome", "This outcome has already been attested");
			if (evtObj.Attestations != null && evtObj.Attestations.Count > 0 && !force)
				throw new CommandException("outcome", "An outcome has already been attested, attesting another one could leak the private key of your oracle. Use -f to force your action.");
			var kValue = await Repository.GetKey(evtObj.NonceKeyPath);
			key.ToECPrivKey().TrySignBIP140(discreteOutcome.Hash, new PrecomputedNonceFunctionHardened(kValue.ToECPrivKey().ToBytes()), out var sig);
			var oracleAttestation = new Key(sig!.s.ToBytes());
			if (await Repository.AddAttestation(evt, oracleAttestation) != outcome)
				throw new InvalidOperationException("Error while validating reveal");
			context.Console.Out.Write(oracleAttestation.ToHex());
		}
	}
}
