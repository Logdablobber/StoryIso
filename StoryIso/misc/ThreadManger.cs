using System;
using System.Collections.Generic;
using System.Threading;

namespace StoryIso.Misc;

public static class ThreadManager
{
	private static readonly Dictionary<int, EventWaitHandle> _handles = [];
    
    public static void Await(int id)
	{
		if (_handles.TryGetValue(id, out var handle))
		{
			handle.WaitOne();
			return;
		}

		var new_handle = new EventWaitHandle(false, EventResetMode.AutoReset);
		_handles[id] = new_handle;

		new_handle.WaitOne();
	}
    
    public static void Send(int id)
	{
		if (!_handles.TryGetValue(id, out var handle))
		{
			throw new NullReferenceException();
		}

		handle.Set();
	}
}