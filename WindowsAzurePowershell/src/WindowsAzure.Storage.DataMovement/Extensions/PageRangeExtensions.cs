using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Extensions
{
	internal static class PageRangeExtensions
	{
		public static IEnumerable<PageRange> SplitRanges(this PageRange pageRange, long maxPageRangeSize)
		{
			long num = pageRange.StartOffset;
			long endOffset = pageRange.EndOffset - pageRange.StartOffset + (long)1;
			do
			{
				PageRange pageRange1 = new PageRange(num, num + Math.Min(endOffset, maxPageRangeSize) - (long)1);
				num = num + maxPageRangeSize;
				endOffset = endOffset - maxPageRangeSize;
				yield return pageRange1;
			}
			while (endOffset > (long)0);
		}

		public static IEnumerable<PageRange> SplitRanges(this IEnumerable<PageRange> pageRanges, long maxPageRangeSize)
		{
			foreach (PageRange pageRange in pageRanges)
			{
				foreach (PageRange pageRange1 in pageRange.SplitRanges(maxPageRangeSize))
				{
					yield return pageRange1;
				}
			}
		}
	}
}