﻿using System;

namespace LightBDD.Framework.Scenarios.Fluent
{
	public interface IGiven<T>
	{
		IGiven<T> And(T given);

		IWhen<T> When(T when);
	}
}
