using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	public class BlobTransferFileTransferEntries : IDictionary<string, BlobTransferFileTransferEntry>, ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>, IEnumerable<KeyValuePair<string, BlobTransferFileTransferEntry>>, IDictionary, ICollection, IEnumerable
	{
		private ConcurrentDictionary<string, BlobTransferFileTransferEntry> dictionary = new ConcurrentDictionary<string, BlobTransferFileTransferEntry>();

		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return this.dictionary.IsEmpty;
			}
		}

		public BlobTransferFileTransferEntry this[string key]
		{
			get
			{
				return this.dictionary[key];
			}
			set
			{
				this.dictionary[key] = value;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return this.dictionary.Keys;
			}
		}

		bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>>.IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).IsReadOnly;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return ((ICollection)this.dictionary).IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return ((ICollection)this.dictionary).SyncRoot;
			}
		}

		bool System.Collections.IDictionary.IsFixedSize
		{
			get
			{
				return ((IDictionary)this.dictionary).IsFixedSize;
			}
		}

		bool System.Collections.IDictionary.IsReadOnly
		{
			get
			{
				return ((IDictionary)this.dictionary).IsReadOnly;
			}
		}

		object System.Collections.IDictionary.this[object key]
		{
			get
			{
				return ((IDictionary)this.dictionary)[key];
			}
			set
			{
				((IDictionary)this.dictionary)[key] = value;
			}
		}

		ICollection System.Collections.IDictionary.Keys
		{
			get
			{
				return ((IDictionary)this.dictionary).Keys;
			}
		}

		ICollection System.Collections.IDictionary.Values
		{
			get
			{
				return ((IDictionary)this.dictionary).Values;
			}
		}

		public ICollection<BlobTransferFileTransferEntry> Values
		{
			get
			{
				return this.dictionary.Values;
			}
		}

		public BlobTransferFileTransferEntries()
		{
		}

		public void Clear()
		{
			this.dictionary.Clear();
		}

		public bool ContainsKey(string key)
		{
			return this.dictionary.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<string, BlobTransferFileTransferEntry>> GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}

		void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>>.Add(KeyValuePair<string, BlobTransferFileTransferEntry> item)
		{
			((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Add(item);
		}

		bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>>.Contains(KeyValuePair<string, BlobTransferFileTransferEntry> item)
		{
			return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Contains(item);
		}

		void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>>.CopyTo(KeyValuePair<string, BlobTransferFileTransferEntry>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).CopyTo(array, arrayIndex);
		}

		bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>>.Remove(KeyValuePair<string, BlobTransferFileTransferEntry> item)
		{
			return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Remove(item);
		}

		void System.Collections.Generic.IDictionary<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>.Add(string key, BlobTransferFileTransferEntry value)
		{
			((IDictionary<string, BlobTransferFileTransferEntry>)this.dictionary).Add(key, value);
		}

		bool System.Collections.Generic.IDictionary<System.String,Microsoft.WindowsAzure.Storage.DataMovement.BlobTransferFileTransferEntry>.Remove(string key)
		{
			return ((IDictionary<string, BlobTransferFileTransferEntry>)this.dictionary).Remove(key);
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			((ICollection)this.dictionary).CopyTo(array, index);
		}

		void System.Collections.IDictionary.Add(object key, object value)
		{
			((IDictionary)this.dictionary).Add(key, value);
		}

		void System.Collections.IDictionary.Clear()
		{
			((IDictionary)this.dictionary).Clear();
		}

		bool System.Collections.IDictionary.Contains(object key)
		{
			return ((IDictionary)this.dictionary).Contains(key);
		}

		IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
		{
			return ((IDictionary)this.dictionary).GetEnumerator();
		}

		void System.Collections.IDictionary.Remove(object key)
		{
			((IDictionary)this.dictionary).Remove(key);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.dictionary).GetEnumerator();
		}

		public bool TryGetValue(string key, out BlobTransferFileTransferEntry value)
		{
			return this.dictionary.TryGetValue(key, out value);
		}
	}
}