﻿using NBitcoin.DataEncoders;
using NDLC.Messages.JsonConverters;
using NDLC.Secp256k1;
using NBitcoin.Policy;
using NBitcoin.Secp256k1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;
using NBitcoin;
using NBitcoin.Crypto;

namespace NDLC.Messages
{
	public class Offer : FundingInformation
	{
		[JsonProperty(Order = -2)]
		public ContractInfo[]? ContractInfo { get; set; }
		[JsonConverter(typeof(OracleInfoJsonConverter))]
		[JsonProperty(Order = -1)]
		public OracleInfo? OracleInfo { get; set; }
		[JsonConverter(typeof(FeeRateJsonConverter))]
		[JsonProperty(Order = 102)]
		public FeeRate? FeeRate { get; set; }
		[JsonProperty(Order = 103)]
		public Timeouts? Timeouts { get; set; }
		[JsonProperty(Order = 104, DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string? EventId { get; set; }

		public DiscretePayoffs ToDiscretePayoffs()
		{
			if (ContractInfo is null || ContractInfo.Length is 0)
				throw new InvalidOperationException("contractInfo is required");
			return base.ToDiscretePayoffs(ContractInfo);
		}
	}

	public class OracleInfo
	{
		public static OracleInfo Parse(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));
			if (!TryParse(str, out var t) || t is null)
				throw new FormatException("Invalid oracleInfo");
			return t;
		}
		public static bool TryParse(string str, out OracleInfo? oracleInfo)
		{
			oracleInfo = null;
			if (str == null)
				throw new ArgumentNullException(nameof(str));
			var bytes = Encoders.Hex.DecodeData(str);
			if (bytes.Length != 64)
				return false;
			if (!ECXOnlyPubKey.TryCreate(bytes.AsSpan().Slice(0, 32), Context.Instance, out var pubkey) || pubkey is null)
				return false;
			if (!SchnorrNonce.TryCreate(bytes.AsSpan().Slice(32), out var rValue) || rValue is null)
				return false;
			oracleInfo = new OracleInfo(pubkey, rValue);
			return true;
		}
		public OracleInfo(ECXOnlyPubKey pubKey, SchnorrNonce rValue)
		{
			if (pubKey == null)
				throw new ArgumentNullException(nameof(pubKey));
			if (rValue is null)
				throw new ArgumentNullException(nameof(rValue));
			RValue = rValue;
			PubKey = pubKey;
		}
		public SchnorrNonce RValue { get; }
		public ECXOnlyPubKey PubKey { get; }

		public bool TryComputeSigpoint(DiscreteOutcome outcome, out ECPubKey? sigpoint)
		{
			return PubKey.TryComputeSigPoint(outcome.Hash, RValue, out sigpoint);
		}
		public void WriteToBytes(Span<byte> out64)
		{
			PubKey.WriteToSpan(out64);
			RValue.WriteToSpan(out64.Slice(32));
		}

		public override string ToString()
		{
			Span<byte> buf = stackalloc byte[64];
			WriteToBytes(buf);
			return Encoders.Hex.EncodeData(buf);
		}
	}

	public class Timeouts
	{
		[JsonConverter(typeof(LocktimeJsonConverter))]
		public LockTime ContractMaturity { get; set; }
		[JsonConverter(typeof(LocktimeJsonConverter))]
		public LockTime ContractTimeout { get; set; }
	}
	public class PubKeyObject
	{
		public PubKey? FundingKey { get; set; }
		public BitcoinAddress? PayoutAddress { get; set; }
	}
	public class FundingInput
	{
		public FundingInput()
		{

		}
		public FundingInput(Coin c)
		{
			Outpoint = c.Outpoint;
			Output = c.TxOut;
		}
		[JsonConverter(typeof(NBitcoin.JsonConverters.OutpointJsonConverter))]
		public OutPoint Outpoint { get; set; } = OutPoint.Zero;
		public TxOut Output { get; set; } = new TxOut();
		public Coin AsCoin()
		{
			if (Outpoint is null || Output is null)
				throw new InvalidOperationException("Funding input is missing some information");
			return new Coin(Outpoint, Output);
		}
	}
	public class ContractInfo
	{
		public ContractInfo(DiscreteOutcome outcome, Money payout)
		{
			Payout = payout;
			Outcome = outcome;
		}
		public DiscreteOutcome Outcome { get; }
		public Money Payout { get; }
	}
}
