using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	internal class MemoryManager
	{
		private MemoryManager.MemoryPool memoryPool;

		public MemoryManager(long capacity, int bufferSize)
		{
			long num = capacity / (long)bufferSize;
			int num1 = (int)Math.Min((long)8192, num);
			this.memoryPool = new MemoryManager.MemoryPool(num1, bufferSize);
		}

		public void ReleaseBuffer(byte[] buffer)
		{
			this.memoryPool.AddBuffer(buffer);
		}

		public byte[] RequireBuffer()
		{
			return this.memoryPool.GetBuffer();
		}

		private class MemoryCell
		{
			private byte[] buffer;

			public byte[] Buffer
			{
				get
				{
					return this.buffer;
				}
			}

			public MemoryManager.MemoryCell NextCell
			{
				get;
				set;
			}

			public MemoryCell(int size)
			{
				this.buffer = new byte[size];
			}
		}

		private class MemoryPool
		{
			public readonly int BufferSize;

			private int availableCells;

			private int allocatedCells;

			private object cellsListLock;

			private MemoryManager.MemoryCell cellsListHeadCell;

			private ConcurrentDictionary<byte[], MemoryManager.MemoryCell> cellsInUse;

			public MemoryPool(int cellsCount, int bufferSize)
			{
				this.BufferSize = bufferSize;
				this.availableCells = cellsCount;
				this.allocatedCells = 0;
				this.cellsListLock = new object();
				this.cellsListHeadCell = null;
				this.cellsInUse = new ConcurrentDictionary<byte[], MemoryManager.MemoryCell>();
			}

			public void AddBuffer(byte[] buffer)
			{
				MemoryManager.MemoryCell memoryCell;
				if (buffer == null)
				{
					throw new ArgumentNullException("buffer");
				}
				if (!this.cellsInUse.TryRemove(buffer, out memoryCell))
				{
					throw new ArgumentException(Resources.BufferNotAllocatedThroughMemoryManagerException, "buffer");
				}
				lock (this.cellsListLock)
				{
					memoryCell.NextCell = this.cellsListHeadCell;
					this.cellsListHeadCell = memoryCell;
					MemoryManager.MemoryPool memoryPool = this;
					memoryPool.availableCells = memoryPool.availableCells + 1;
				}
			}

			public byte[] GetBuffer()
			{
				if (this.availableCells > 0)
				{
					MemoryManager.MemoryCell memoryCell = null;
					lock (this.cellsListLock)
					{
						if (this.availableCells > 0)
						{
							if (this.cellsListHeadCell == null)
							{
								memoryCell = new MemoryManager.MemoryCell(this.BufferSize);
								MemoryManager.MemoryPool memoryPool = this;
								memoryPool.allocatedCells = memoryPool.allocatedCells + 1;
							}
							else
							{
								memoryCell = this.cellsListHeadCell;
								this.cellsListHeadCell = memoryCell.NextCell;
								memoryCell.NextCell = null;
							}
							MemoryManager.MemoryPool memoryPool1 = this;
							memoryPool1.availableCells = memoryPool1.availableCells - 1;
						}
					}
					if (memoryCell != null)
					{
						this.cellsInUse.TryAdd(memoryCell.Buffer, memoryCell);
						return memoryCell.Buffer;
					}
				}
				return null;
			}
		}
	}
}