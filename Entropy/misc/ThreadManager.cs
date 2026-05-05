using System;
using System.Collections.Generic;
using System.Threading;

namespace Entropy.Misc;

public static class ThreadManager
{
	private const string SCOPE = "Entropy/";
    
    public static void Await(int id)
	{
		var handle = new EventWaitHandle(false, EventResetMode.AutoReset, name:SCOPE + id);

		handle.WaitOne();
	}
    
    public static void Send(int id)
    {
	    var handle = new EventWaitHandle(false, EventResetMode.AutoReset, SCOPE + id);

	    handle.Set();
    }
}