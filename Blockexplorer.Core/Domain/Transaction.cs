﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Blockexplorer.Core.Domain
{
	public class Transaction
	{
		public string TransactionId { get; set; }

		public TransactionType TransactionType { get; set; }

		public string OriginalJson { get; set; }

		public long Confirmations { get; set; }
		public DateTime Time { get; set; }
		public string Blockhash { get; set; }
		public IList<VIn> TransactionIn { get; set; }
		public IList<Out> TransactionsOut { get; set; }
		public bool IsColor { get; set; }
		public bool IsCoinBase { get; set; }
		public string Hex { get; set; }
		public Block Block { get; set; }
		public uint Size { get; set; }

		public decimal TotalOut
		{
			get
			{
				if (TransactionsOut == null) return 0;

				return TransactionsOut.Sum(x => x.Value);
			}
		}

		public decimal Fees
		{
			get
			{
				if (TransactionType == TransactionType.PoS_Reward || TransactionType == TransactionType.PoW_Reward_Coinbase)
					return 0;

				return TransactionIn.Sum(x => x.PrevVOutFetchedValue) - TransactionsOut.Sum(x => x.Value);
			}
		}

		public string GetTransactionTypeText()
		{
			switch (TransactionType)
			{
				case TransactionType.PoW_Reward_Coinbase:
					return "Proof-of-Work Miner Reward";
				case TransactionType.PoS_Reward:
					return "Proof-of-Stake Block Reward";
				case TransactionType.Money:
					return "Obsidian Money Transfer";
				case TransactionType.Unknown:
					return "Unknown";
				default:
					return "Undefined";
			}
		}
	}

	public class VIn
	{

		/// <summary>
		/// Marks the first tx, The data in "coinbase" can be anything; it isn't used. 
		/// Bitcoin puts the current compact-format target and the arbitrary-precision "extraNonce" number there, which increments every time the Nonce field in the block header overflows.
		/// https://en.bitcoin.it/wiki/Transaction#general_format_.28inside_a_block.29_of_each_input_of_a_transaction_-_Txin
		/// </summary>
		public string Coinbase { get; set; }

		/// <summary>
		/// Normally 0xFFFFFFFF/4294967295; irrelevant unless transaction's lock_time is > 0.
		/// </summary>
		public uint Sequence { get; set; }


		public string PrevTxIdPointer { get; set; }
		public string PrevVOutFetchedAddress { get; set; }

		/// <summary>
		/// This index is not from the spec, just for display purposes.
		/// </summary>
		public int Index { get; set; }
		public decimal PrevVOutFetchedValue { get; set; }
		public string AssetId { get; set; }
		public int PrevVOutPointer { get; set; }
		public string ScriptSigHex { get; set; }
	}

	public class Out
	{
		public string TransactionId { get; set; }
		public decimal Value { get; set; }
		public string Address { get; set; }
		public int Index { get; set; }
		public string AssetId { get; set; }
		public long Quantity { get; set; }
	}
}