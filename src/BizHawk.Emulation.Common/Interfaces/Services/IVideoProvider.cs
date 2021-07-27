﻿using System;

namespace BizHawk.Emulation.Common
{
	/// <summary>
	/// This service provides the ability to pass video output to the client
	/// If available the client will display video output to the user,
	/// If unavailable the client will fall back to a default video implementation, presumably
	/// a black screen of some arbitrary size
	/// </summary>
	public interface IVideoProvider : IEmulatorService
	{
		/// <summary>
		/// Returns a frame buffer of the current video content
		/// </summary>
		int[] GetVideoBuffer();

		// put together, these describe a metric on the screen
		// they should define the smallest size that the buffer can be placed inside such that:
		// 1. no actual pixel data is lost
		// 2. aspect ratio is accurate

		/// <summary>
		/// Gets a value that together with <seealso cref="VirtualHeight"/>, describes a metric on the screen
		/// they should define the smallest size that the buffer can be placed inside such that:
		/// 1. no actual pixel data is lost
		/// 2. aspect ratio is accurate
		/// </summary>
		int VirtualWidth { get; }

		/// <summary>
		/// Gets a value that together with <seealso cref="VirtualWidth"/>, describes a metric on the screen
		/// they should define the smallest size that the buffer can be placed inside such that:
		/// 1. no actual pixel data is lost
		/// 2. aspect ratio is accurate
		/// </summary>
		int VirtualHeight { get; }

		/// <summary>
		/// Gets the width of the frame buffer
		/// </summary>
		int BufferWidth { get; }

		/// <summary>
		/// Gets the height of the frame buffer
		/// </summary>
		int BufferHeight { get; }

		/// <summary>
		/// Gets the vsync Numerator. Combined with the <seealso cref="VsyncDenominator"/> can be used to calculate a precise vsync rate.
		/// </summary>
		int VsyncNumerator { get; }

		/// <summary>
		/// Gets the vsync Denominator. Combined with the <seealso cref="VsyncNumerator"/> can be used to calculate a precise vsync rate.
		/// </summary>
		int VsyncDenominator { get; }

		/// <summary>
		/// Gets the default color when no other color is applied
		/// Often cores will set this to something other than black
		/// to show that the core is in fact loaded and frames are rendering
		/// which is less obvious if it is the same as the default video output
		/// </summary>
		int BackgroundColor { get; }
	}

	public static class VideoProviderExtensions
	{
		/// <summary>
		/// Sets the frame buffer to the given frame buffer
		/// Note: This sets the value returned by <see cref="IVideoProvider.GetVideoBuffer" />
		/// which relies on the core to send a reference to the frame buffer instead of a copy,
		/// in order to work
		/// </summary>
		public static void PopulateFromBuffer(this IVideoProvider videoProvider, int[] frameBuffer)
		{
			var b1 = frameBuffer;
			var b2 = videoProvider.GetVideoBuffer();
			int len = Math.Min(b1.Length, b2.Length);
			for (int i = 0; i < len; i++)
			{
				b2[i] = b1[i];
			}
		}
	}
}
