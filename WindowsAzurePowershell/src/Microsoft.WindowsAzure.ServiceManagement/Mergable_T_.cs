using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.ServiceManagement
{
	[DataContract]
	public abstract class Mergable<T> : IResolvable, IMergable<T>, IMergable, IExtensibleDataObject
	where T : Mergable<T>
	{
		private Dictionary<string, object> propertyStore;

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		private Dictionary<string, object> PropertyStore
		{
			get
			{
				if (this.propertyStore == null)
				{
					this.propertyStore = new Dictionary<string, object>();
				}
				return this.propertyStore;
			}
		}

		protected Mergable()
		{
		}

		protected TValue Convert<TValue>()
		{
			TValue tValue;
			DataContractSerializer dataContractSerializer = new DataContractSerializer(this.GetType());
			DataContractSerializer dataContractSerializer1 = new DataContractSerializer(typeof(TValue));
			using (MemoryStream memoryStream = new MemoryStream())
			{
				dataContractSerializer.WriteObject(memoryStream, this);
				memoryStream.Position = (long)0;
				tValue = (TValue)dataContractSerializer1.ReadObject(memoryStream);
			}
			return tValue;
		}

		protected Nullable<TValue> GetField<TValue>(string fieldName)
		where TValue : struct
		{
			object obj;
			if (this.PropertyStore.TryGetValue(fieldName, out obj))
			{
				return new Nullable<TValue>((TValue)obj);
			}
			return null;
		}

		protected TValue GetValue<TValue>(string fieldName)
		{
			object obj;
			if (this.PropertyStore.TryGetValue(fieldName, out obj))
			{
				return (TValue)obj;
			}
			return default(TValue);
		}

		public void Merge(object other)
		{
			((IMergable<T>)this).Merge((T)other);
		}

		public void Merge(T other)
		{
			object obj;
			foreach (KeyValuePair<string, object> propertyStore in other.PropertyStore)
			{
				if (this.PropertyStore.TryGetValue(propertyStore.Key, out obj))
				{
					IMergable mergable = obj as IMergable;
					if (mergable != null)
					{
						mergable.Merge(propertyStore.Value);
						continue;
					}
				}
				this.PropertyStore[propertyStore.Key] = propertyStore.Value;
			}
		}

		public virtual object ResolveType()
		{
			return this;
		}

		protected void SetField<TValue>(string fieldName, Nullable<TValue> value)
		where TValue : struct
		{
			if (value.HasValue)
			{
				this.PropertyStore[fieldName] = value.Value;
			}
		}

		protected void SetValue<TValue>(string fieldName, TValue value)
		{
			this.PropertyStore[fieldName] = value;
		}
	}
}