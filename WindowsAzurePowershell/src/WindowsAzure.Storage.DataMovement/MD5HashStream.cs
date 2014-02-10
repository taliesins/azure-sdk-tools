using Microsoft.WindowsAzure.Storage.DataMovement.Exceptions;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal class MD5HashStream : IDisposable
	{
		private static int maxRetryCount;

		private Stream stream;

		private SemaphoreSlim semaphore;

		private volatile bool finishedSeparateMd5Calculator;

		private bool succeededSeparateMd5Calculator;

		private MD5CryptoServiceProvider md5hash;

		private long md5hashOffset;

		public bool CheckMd5Hash
		{
			get
			{
				return null != this.md5hash;
			}
		}

		public bool FinishedSeparateMd5Calculator
		{
			get
			{
				return this.finishedSeparateMd5Calculator;
			}
		}

		public byte[] Hash
		{
			get
			{
				if (this.md5hash == null)
				{
					return null;
				}
				return this.md5hash.Hash;
			}
		}

		public bool SucceededSeparateMd5Calculator
		{
			get
			{
				this.WaitMD5CalculationToFinish();
				return this.succeededSeparateMd5Calculator;
			}
		}

		static MD5HashStream()
		{
			MD5HashStream.maxRetryCount = 10;
		}

		public MD5HashStream(Stream stream, long lastTransferOffset, bool md5hashCheck)
		{
			this.stream = stream;
			this.md5hashOffset = lastTransferOffset;
			if ((long)0 == this.md5hashOffset || !md5hashCheck)
			{
				this.finishedSeparateMd5Calculator = true;
				this.succeededSeparateMd5Calculator = true;
			}
			else
			{
				this.semaphore = new SemaphoreSlim(1, 1);
			}
			if (md5hashCheck)
			{
				this.md5hash = new MD5CryptoServiceProvider();
			}
			if (!this.finishedSeparateMd5Calculator && !this.stream.CanRead)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string streamMustSupportReadException = Resources.StreamMustSupportReadException;
				object[] objArray = new object[] { "Stream" };
				throw new NotSupportedException(string.Format(invariantCulture, streamMustSupportReadException, objArray));
			}
			if (!this.stream.CanSeek)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				string streamMustSupportSeekException = Resources.StreamMustSupportSeekException;
				object[] objArray1 = new object[] { "Stream" };
				throw new NotSupportedException(string.Format(cultureInfo, streamMustSupportSeekException, objArray1));
			}
		}

		private IAsyncResult AsyncBegin(Func<IAsyncResult> asyncCall)
		{
			IAsyncResult asyncResult;
			this.WaitOnSemaphore();
			try
			{
				asyncResult = asyncCall();
			}
			catch (Exception exception)
			{
				this.ReleaseSemaphore();
				throw;
			}
			return asyncResult;
		}

		public IAsyncResult BeginRead(long readOffset, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this.AsyncBegin(() => {
				this.stream.Position = readOffset;
				return this.stream.BeginRead(buffer, offset, count, (IAsyncResult asyncResult) => callback(asyncResult), state);
			});
		}

		public IAsyncResult BeginWrite(long writeOffset, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return this.AsyncBegin(() => {
				this.stream.Position = writeOffset;
				return this.stream.BeginWrite(buffer, offset, count, (IAsyncResult asyncResult) => callback(asyncResult), state);
			});
		}

		public void CalculateMd5(MemoryManager memoryManager)
		{
			if (this.md5hash == null)
			{
				return;
			}
			byte[] numArray = memoryManager.RequireBuffer();
			if (numArray == null)
			{
				for (int i = 0; i < MD5HashStream.maxRetryCount && numArray == null; i++)
				{
					Thread.Sleep(200);
					numArray = memoryManager.RequireBuffer();
				}
				if (numArray == null)
				{
					lock (this.md5hash)
					{
						this.finishedSeparateMd5Calculator = true;
					}
					throw new BlobTransferException(BlobTransferErrorCode.FailToAllocateMemory, Resources.FailedToAllocateMemoryException);
				}
			}
			long num = (long)0;
			int num1 = 0;
			while (true)
			{
				lock (this.md5hash)
				{
					if (num < this.md5hashOffset)
					{
						num1 = (int)Math.Min(this.md5hashOffset - num, (long)((int)numArray.Length));
					}
					else
					{
						this.succeededSeparateMd5Calculator = true;
						this.finishedSeparateMd5Calculator = true;
						break;
					}
				}
				try
				{
					num1 = this.Read(num, numArray, 0, num1);
				}
				catch (Exception exception)
				{
					lock (this.md5hash)
					{
						this.finishedSeparateMd5Calculator = true;
					}
					memoryManager.ReleaseBuffer(numArray);
					throw;
				}
				lock (this.md5hash)
				{
					this.md5hash.TransformBlock(numArray, 0, num1, null, 0);
				}
				num = num + (long)num1;
			}
			memoryManager.ReleaseBuffer(numArray);
		}

		public virtual void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.md5hash != null)
				{
					this.md5hash.Clear();
					this.md5hash = null;
				}
				this.stream = null;
			}
		}

		public int EndRead(IAsyncResult asyncResult)
		{
			int num;
			try
			{
				num = this.stream.EndRead(asyncResult);
			}
			finally
			{
				this.ReleaseSemaphore();
			}
			return num;
		}

		public void EndWrite(IAsyncResult asyncResult)
		{
			try
			{
				this.stream.EndWrite(asyncResult);
			}
			finally
			{
				this.ReleaseSemaphore();
			}
		}

		public bool MD5HashTransformBlock(long streamOffset, byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			bool flag;
			int num;
			if (this.md5hash == null)
			{
				return true;
			}
			bool booleanu0020modrequ0028Systemu002eRuntimeu002eCompilerServicesu002eIsVolatileu0029 = this.finishedSeparateMd5Calculator;
			if (!this.finishedSeparateMd5Calculator)
			{
				lock (this.md5hash)
				{
					bool booleanu0020modrequ0028Systemu002eRuntimeu002eCompilerServicesu002eIsVolatileu00291 = this.finishedSeparateMd5Calculator;
					if (!this.finishedSeparateMd5Calculator)
					{
						if (streamOffset == this.md5hashOffset)
						{
							MD5HashStream mD5HashStream = this;
							mD5HashStream.md5hashOffset = mD5HashStream.md5hashOffset + (long)inputCount;
						}
						flag = true;
					}
					else if (this.succeededSeparateMd5Calculator)
					{
						if (streamOffset >= this.md5hashOffset)
						{
							num = this.md5hash.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
						}
						return true;
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
			if (streamOffset >= this.md5hashOffset)
			{
				num = this.md5hash.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
			}
			return true;
		}

		public byte[] MD5HashTransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			this.WaitMD5CalculationToFinish();
			if (!this.succeededSeparateMd5Calculator)
			{
				return null;
			}
			if (this.md5hash == null)
			{
				return null;
			}
			return this.md5hash.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
		}

		private int Read(long readOffset, byte[] buffer, int offset, int count)
		{
			int num;
			this.WaitOnSemaphore();
			try
			{
				this.stream.Position = readOffset;
				num = this.stream.Read(buffer, offset, count);
			}
			finally
			{
				this.ReleaseSemaphore();
			}
			return num;
		}

		private void ReleaseSemaphore()
		{
			if (!this.finishedSeparateMd5Calculator)
			{
				this.semaphore.Release();
			}
		}

		private void WaitMD5CalculationToFinish()
		{
			if (this.finishedSeparateMd5Calculator != null)
			{
				return;
			}
			SpinWait spinWait = new SpinWait();
			while (this.finishedSeparateMd5Calculator == null)
			{
				spinWait.SpinOnce();
			}
			spinWait.Reset();
		}

		private void WaitOnSemaphore()
		{
			if (!this.finishedSeparateMd5Calculator)
			{
				this.semaphore.Wait();
			}
		}
	}
}